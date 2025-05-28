using MQTTnet;
using SmartReplenishment.Messaging.Mqtt;
using SmartReplenishment.Messaging.Mqtt.Messages;
using SmartReplenishment.Services.NotificationSMS.Instrumentation;

namespace SmartReplenishment.Services.NotificationSMS.BackgroundServices;

public class Worker : BackgroundService
{
  private readonly ILogger<Worker> _logger;
  private readonly IMqttClient _mqttClient;
  private readonly IMqttSettings _mqttSettings;

  private readonly CancellationTokenSource _internalCancellationTokenSource = new();

  public Worker(
    ILogger<Worker> logger,
    IMqttClient mqttClient,
    IMqttSettings mqttSettings)
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
    var message = MqttApplicationMessageProvider.GetSubscribeMqttApplicationMessage<MqttMessageReplenishmentCompleted>(args.ApplicationMessage);
    if (message is null)
    {
      _logger.LogWarning("Mqtt application message from {topic} by {clientId} is null", args.ApplicationMessage.Topic, args.ClientId);
      return;
    }

    using var activity = ActivitySourceProvider.ActivitySource.StartActivity("SMS");
    await Task.Delay(100);
  }
}
