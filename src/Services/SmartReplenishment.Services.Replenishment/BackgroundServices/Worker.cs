using MQTTnet;
using SmartReplenishment.Messaging.Mqtt;
using SmartReplenishment.Messaging.Mqtt.Messages;
using System.Text.Json;
using System.Text;
using SmartReplenishment.Services.Replenishment.Instrumentation;

namespace SmartReplenishment.Services.Replenishment.BackgroundServices;

public class Worker : BackgroundService
{
  private readonly ILogger<Worker> _logger;
  private readonly IMqttClient _mqttClient;
  private readonly IMqttSettings _mqttSettings;

  private readonly CancellationTokenSource _internalCancellationTokenSource = new();

  public Worker(ILogger<Worker> logger, IMqttClient mqttClient, IMqttSettings mqttSettings)
  {
    _logger = logger;
    _mqttClient = mqttClient;
    _mqttSettings = mqttSettings;
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
      var mqttSubscribeOptions = MqttClientSubscribeOptionsProvider.Get(_mqttSettings.GetRequiredTopicNameSubscribe());
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
    using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, _internalCancellationTokenSource.Token);
    var combinedToken = linkedCts.Token;

    while (!combinedToken.IsCancellationRequested)
    {
      await Task.Delay(1000, combinedToken);
    }
  }

  private async Task OnApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args)
  {
    var message = MqttApplicationMessageProvider.GetSubscribeMqttApplicationMessage<MqttMessageStockLow>(args.ApplicationMessage);
    if (message is null)
    {
      _logger.LogWarning("Mqtt application message from {topic} by {clientId} is null", args.ApplicationMessage.Topic, args.ClientId);
      return;
    }

    await ProcessLowArticleStockReplenishment();
    await PublishMessageReplenishmentRequest(message);
  }

  private async Task PublishMessageReplenishmentRequest(MqttMessageStockLow message)
  {
    if (!_mqttClient.IsConnected)
    {
      _logger.LogCritical("Could not publish message {messageType}: no connection to the broker", nameof(MqttMessageReplenishmentRequest));
      return;
    }

    var mqttMessage = MqttApplicationMessageProvider.GetPublishMqttApplicationMessage(
        _mqttSettings.GetRequiredTopicNamePublish(),
        new MqttMessageReplenishmentRequest(message.ArticleId, message.ArticleName, 25));

    await _mqttClient.PublishAsync(mqttMessage);
  }

  private async Task ProcessLowArticleStockReplenishment()
  {
    using var activity = ActivitySourceProvider.ActivitySource.StartActivity(nameof(ProcessLowArticleStockReplenishment));
    await Task.Delay(275); // simulate some work
  }
}
