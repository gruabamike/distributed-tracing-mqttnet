using Microsoft.EntityFrameworkCore;
using MQTTnet;
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
    private readonly InventoryDbContext _context;

    public Worker(
      ILogger<Worker> logger,
      IMqttClient mqttClient,
      IMqttSettings mqttSettings,
      InventoryDbContext context)
    {
      _logger = logger;
      _mqttClient = mqttClient;
      _mqttSettings = mqttSettings;
      _context = context;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
      _mqttClient.ApplicationMessageReceivedAsync += OnApplicationMessageReceivedAsync;

      var mqttClientOptions = MqttClientOptionsProvider.Get(_mqttSettings);
      var response = await _mqttClient.ConnectAsync(mqttClientOptions, cancellationToken);
      if (response.ResultCode != MqttClientConnectResultCode.Success)
      {
        _logger.LogCritical("Could not connect to mqtt broker");
      }

      if (_mqttSettings.TopicNameSubscribe is null)
        throw new InvalidOperationException("TODO");

      if (_mqttClient.IsConnected)
      {
        var mqttSubscribeOptions = MqttClientSubscribeOptionsProvider.Get(_mqttSettings.TopicNameSubscribe);
        await _mqttClient.SubscribeAsync(mqttSubscribeOptions, cancellationToken);
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
      while (!stoppingToken.IsCancellationRequested)
      {
        if (_logger.IsEnabled(LogLevel.Information))
        {
          _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
        }

        await Task.Delay(1000, stoppingToken);
      }
    }

    private async Task OnApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args)
    {
      var payloadString = Encoding.UTF8.GetString(args.ApplicationMessage.Payload);
      var message = JsonSerializer.Deserialize<MqttMessageStockLevelChanged>(payloadString);
      if (message is null)
      {
        return;
      }

      using var activity = InstrumentationSources.ActivitySource.StartActivity("receiving message");
      _logger.LogInformation("Received application message");

      var stockProduct = _context.StockProducts
        .Include(sp => sp.StockConfiguration)
        .FirstOrDefault(sp => sp.Name == message.ProductName);

      if (stockProduct is null || stockProduct.StockConfiguration is null)
      {
        return;
      }

      stockProduct.Amount -= message.StockDecreaseAmount;
      if (stockProduct.Amount < 0)
        stockProduct.Amount = 0;

      await _context.SaveChangesAsync();

      if (stockProduct.Amount <= stockProduct.StockConfiguration.MinStockThreshold)
      {
        await PublishMessage(stockProduct);
      }
    }

    private async Task PublishMessage(StockProduct stockProduct)
    {
      //var mqttClientOptions = MqttClientOptionsProvider.Get(_mqttSettings);
      //var response = await _mqttClient.ConnectAsync(mqttClientOptions);
      //if (response.ResultCode != MqttClientConnectResultCode.Success)
      //  return;

      var message = new MqttMessageStockLow(stockProduct.Name, stockProduct.Amount);
      var mqttMessage = MqttApplicationMessageProvider.Get(
        _mqttSettings.TopicNamePublish ?? throw new InvalidOperationException($"No {nameof(IMqttSettings.TopicNamePublish)} specified."),
        message);
      await _mqttClient.PublishAsync(mqttMessage);
      //await _mqttClient.DisconnectAsync();
    }
  }
}
