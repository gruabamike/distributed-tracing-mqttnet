using MQTTnet;
using Polly;
using SmartReplenishment.Clients.StockLevelEmitter.Instrumentation;
using SmartReplenishment.Clients.StockLevelEmitter.Settings;
using SmartReplenishment.Messaging.Mqtt;
using SmartReplenishment.Messaging.Mqtt.Messages;

namespace SmartReplenishment.Clients.StockLevelEmitter.BackgroundServices;

public class Worker : BackgroundService
{
  private static readonly Random Random = new Random();
  private const int MinimumDelayInMilliseconds = 1000;
  private const int MaximumDelayInMilliseconds = 5000;

  private readonly ILogger<Worker> _logger;
  private readonly IMqttClient _mqttClient;
  private readonly IMqttSettings _mqttSettings;
  private readonly IStockLevelEmitterSettings _stockLevelEmitterSettings;

  private readonly CancellationTokenSource _internalCancellationTokenSource = new();

  public Worker(ILogger<Worker> logger,
    IMqttClient mqttClient,
    IMqttSettings settings,
    IStockLevelEmitterSettings stockLevelEmitterSettings)
  {
    _logger = logger;
    _mqttClient = mqttClient;
    _mqttSettings = settings;
    _stockLevelEmitterSettings = stockLevelEmitterSettings;
  }

  public override async Task StartAsync(CancellationToken cancellationToken)
  {
    var connectionResult = await MqttRetryPolicies.GetMqttConnectRetryPolicy(_logger)
      .ExecuteAsync(async () => await _mqttClient.ConnectAsync(
        MqttClientOptionsProvider.Get(_mqttSettings),
        cancellationToken));

    if (connectionResult.ResultCode != MqttClientConnectResultCode.Success)
    {
      _logger.LogCritical("Failed to connect to MQTT broker after retries. Stopping service...");
      _internalCancellationTokenSource.Cancel();
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
      await PublishMessageStockLevelChanged(combinedToken);

      int delayInMilliseconds = Random.Next(MinimumDelayInMilliseconds, MaximumDelayInMilliseconds);
      await Task.Delay(delayInMilliseconds, combinedToken);
    }
  }

  private async Task PublishMessageStockLevelChanged(CancellationToken cancellationToken)
  {
    if (!_mqttClient.IsConnected)
    {
      _logger.LogCritical("Could not publish message {messageType}: no connection to the broker", nameof(MqttMessageStockLevelChanged));
      return;
    }

    _logger.LogInformation(
      "Emitting Article Stock Level Decrease: {articleName} decreased by {stockDecreaseAmount} at {time}",
      _stockLevelEmitterSettings.ArticleName,
      _stockLevelEmitterSettings.StockDecreaseAmount,
      DateTimeOffset.Now);

    using var activity = ActivitySourceProvider.ActivitySource.StartActivity("Emitting Article Stock Level Decrease");
    activity?.SetBaggage("article.name", _stockLevelEmitterSettings.ArticleName);
    activity?.SetTag("article.name", _stockLevelEmitterSettings.ArticleName);
    activity?.SetTag("article.stock_decrease_amount", _stockLevelEmitterSettings.StockDecreaseAmount);

    var mqttMessage = MqttApplicationMessageProvider.GetPublishMqttApplicationMessage(
      _mqttSettings.GetRequiredTopicNamePublish(),
      new MqttMessageStockLevelChanged(_stockLevelEmitterSettings.ArticleName, _stockLevelEmitterSettings.StockDecreaseAmount));
    
    await _mqttClient.PublishAsync(mqttMessage, cancellationToken);
  }
}
