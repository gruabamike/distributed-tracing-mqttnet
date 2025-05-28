using MQTTnet;
using SmartReplenishment.Messaging.Http;
using SmartReplenishment.Messaging.Mqtt;
using SmartReplenishment.Messaging.Mqtt.Messages;
using SmartReplenishment.Services.Fulfillment.Instrumentation;
using System.Net.Http.Json;

namespace SmartReplenishment.Services.Fulfillment.BackgroundServices;

public class Worker : BackgroundService
{
  private readonly ILogger<Worker> _logger;
  private readonly IMqttClient _mqttClient;
  private readonly IMqttSettings _mqttSettings;
  private readonly IHttpClientFactory _httpClientFactory;

  private readonly CancellationTokenSource _internalCancellationTokenSource = new();

  public Worker(
    ILogger<Worker> logger,
    IMqttClient mqttClient,
    IMqttSettings mqttSettings,
    IHttpClientFactory httpClientFactory)
  {
    _logger = logger;
    _mqttClient = mqttClient;
    _mqttSettings = mqttSettings;
    _httpClientFactory = httpClientFactory;
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
    var message = MqttApplicationMessageProvider.GetSubscribeMqttApplicationMessage<MqttMessageReplenishmentRequest>(args.ApplicationMessage);
    if (message is null)
    {
      _logger.LogWarning("Mqtt application message from {topic} by {clientId} is null", args.ApplicationMessage.Topic, args.ClientId);
      return;
    }

    await ProcessArticleStockFulfillment(message);
    await PublishMessageReplenishmentRequest(message);
  }

  private async Task PublishMessageReplenishmentRequest(MqttMessageReplenishmentRequest requestMessage)
  {
    if (!_mqttClient.IsConnected)
    {
      _logger.LogCritical("Could not publish message {messageType}: no connection to the broker", nameof(MqttMessageReplenishmentRequest));
      return;
    }

    var mqttMessage = MqttApplicationMessageProvider.GetPublishMqttApplicationMessage(
        _mqttSettings.GetRequiredTopicNamePublish(),
        new MqttMessageReplenishmentCompleted(requestMessage.ArticleId, requestMessage.ArticleName, requestMessage.StockIncreaseAmount));

    await _mqttClient.PublishAsync(mqttMessage);
  }

  private async Task ProcessArticleStockFulfillment(MqttMessageReplenishmentRequest requestMessage)
  {
    var name = ActivitySourceProvider.ActivitySource.Name;
    var version = ActivitySourceProvider.ActivitySource.Version;
    using var activity = ActivitySourceProvider.ActivitySource.StartActivity(nameof(ProcessArticleStockFulfillment));
    using var httpClient = _httpClientFactory.CreateClient();
    httpClient.BaseAddress = new Uri("http://localhost:5285");

    try
    {
      var httpResponseMessage = await httpClient.PutAsync(
        $"stock/{requestMessage.ArticleId}/increase",
        JsonContent.Create(new StockAmountIncrease(requestMessage.StockIncreaseAmount)));
      httpResponseMessage.EnsureSuccessStatusCode();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error during increasing article stock");
    }
  }
}
