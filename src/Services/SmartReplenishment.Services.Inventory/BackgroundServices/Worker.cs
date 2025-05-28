using Microsoft.EntityFrameworkCore;
using MQTTnet;
using Polly;
using SmartReplenishment.Messaging.Mqtt;
using SmartReplenishment.Messaging.Mqtt.Messages;
using SmartReplenishment.Services.Inventory.Data;
using SmartReplenishment.Services.Inventory.Instrumentation;
using SmartReplenishment.Shared.Model;
using System.Text;
using System.Text.Json;

namespace SmartReplenishment.Services.Inventory.BackgroundServices
{
  public class Worker : BackgroundService
  {
    private readonly ILogger<Worker> _logger;
    private readonly IMqttClient _mqttClient;
    private readonly IMqttSettings _mqttSettings;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    private readonly CancellationTokenSource _internalCancellationTokenSource = new();

    public Worker(
      ILogger<Worker> logger,
      IMqttClient mqttClient,
      IMqttSettings mqttSettings,
      IServiceScopeFactory serviceScopeFactory)
    {
      _logger = logger;
      _mqttClient = mqttClient;
      _mqttSettings = mqttSettings;
      _serviceScopeFactory = serviceScopeFactory;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
      _mqttClient.ApplicationMessageReceivedAsync += OnApplicationMessageReceivedAsync;

      var connectionResult = await MqttRetryPolicies.GetMqttConnectRetryPolicy(_logger)
      .ExecuteAsync(async () => await _mqttClient.ConnectAsync(
        MqttClientOptionsProvider.Get(_mqttSettings),
        cancellationToken));

      if (connectionResult.ResultCode != MqttClientConnectResultCode.Success)
      {
        _logger.LogCritical("Failed to connect to MQTT broker after retries. Stopping service...");
        _internalCancellationTokenSource.Cancel();
      }

      if (_mqttClient.IsConnected)
      {
        await _mqttClient.SubscribeAsync(
          MqttClientSubscribeOptionsProvider.Get(_mqttSettings.GetRequiredTopicNameSubscribe()),
          cancellationToken);
      }

      await base.StartAsync(cancellationToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
      if (_mqttClient.IsConnected)
        await _mqttClient.DisconnectAsync(MqttClientDisconnectOptionsReason.NormalDisconnection, cancellationToken: cancellationToken);

      await base.StopAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, _internalCancellationTokenSource.Token);
      var combinedToken = linkedCts.Token;

      while (!combinedToken.IsCancellationRequested)
      {
        await Task.Delay(1000, combinedToken);
      }
    }

    private async Task OnApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args)
    {
      var message = MqttApplicationMessageProvider.GetSubscribeMqttApplicationMessage<MqttMessageStockLevelChanged>(args.ApplicationMessage);
      if (message is null)
      {
        _logger.LogWarning("Mqtt application message from {topic} by {clientId} is null", args.ApplicationMessage.Topic, args.ClientId);
        return;
      }

      _logger.LogInformation(
        "Updating Article Stock Level Decrease: {articleName} decreased by {stockDecreaseAmount} at {time}",
        message.ArticleName,
        message.StockDecreaseAmount,
        DateTimeOffset.Now);

      using var activity = ActivitySourceProvider.ActivitySource.StartActivity("Updating Article Stock Level Decrease");
      
      await using var scope = _serviceScopeFactory.CreateAsyncScope();
      await using var context = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

      var stockArticle = await FetchStockArticle(context, message.ArticleName);
      if (stockArticle is null || stockArticle.StockConfiguration is null)
      {
        _logger.LogWarning($"Article {message.ArticleName} not found in database");
        return;
      }

      await UpdateArticleStockAmount(context, stockArticle, message.StockDecreaseAmount);
      if (stockArticle.Amount <= stockArticle.StockConfiguration.MinStockThreshold)
      {
        await PublishMessageStockLow(stockArticle);
      }
    }

    private async Task<StockArticle?> FetchStockArticle(InventoryDbContext context, string articleName)
    {
      using var activity = ActivitySourceProvider.ActivitySource.StartActivity(nameof(FetchStockArticle));
      return await context.StockArticles
        .Include(sp => sp.StockConfiguration)
        .FirstOrDefaultAsync(sp => sp.Name == articleName);
    }

    private async Task PublishMessageStockLow(StockArticle stockArticle)
    {
      if (!_mqttClient.IsConnected)
      {
        _logger.LogCritical("Could not publish message {messageType}: no connection to the broker", nameof(MqttMessageStockLow));
        return;
      }

      var mqttMessage = MqttApplicationMessageProvider.GetPublishMqttApplicationMessage(
        _mqttSettings.GetRequiredTopicNamePublish(),
        new MqttMessageStockLow(stockArticle.Id, stockArticle.Name, stockArticle.Amount));

      await _mqttClient.PublishAsync(mqttMessage);
    }

    private async Task UpdateArticleStockAmount(InventoryDbContext context, StockArticle stockArticle, int stockDecreaseAmount)
    {
      using var activity = ActivitySourceProvider.ActivitySource.StartActivity(nameof(UpdateArticleStockAmount));

      stockArticle.Amount -= stockDecreaseAmount;
      if (stockArticle.Amount < 0)
        stockArticle.Amount = 0;

      await context.SaveChangesAsync();
    }
  }
}
