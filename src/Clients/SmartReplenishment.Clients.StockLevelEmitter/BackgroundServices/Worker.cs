using MQTTnet;
using SmartReplenishment.Clients.StockLevelEmitter.Instrumentation;
using SmartReplenishment.Clients.StockLevelEmitter.Settings;
using SmartReplenishment.Messaging.Mqtt;
using SmartReplenishment.Messaging.Mqtt.Messages;

namespace SmartReplenishment.Clients.StockLevelEmitter.BackgroundServices;

public class Worker : BackgroundService
{
  private static readonly Random Random = new Random();
  private const int MinimumDelayInMilliseconds = 1000;
  private const int MaximumDelayInMilliseonds = 5000;

  private readonly ILogger<Worker> _logger;
  private readonly IMqttClient _mqttClient;
  private readonly IMqttSettings _mqttSettings;
  private readonly IStockLevelEmitterSettings _stockLevelEmitterSettings;

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
    var mqttClientOptions = MqttClientOptionsProvider.Get(_mqttSettings);
    var response = await _mqttClient.ConnectAsync(mqttClientOptions, cancellationToken);
    if (response.ResultCode != MqttClientConnectResultCode.Success)
    {
      _logger.LogCritical("Could not connect to mqtt broker");
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
      await PublishMessageStockLevelChanged(stoppingToken);

      int delayInMilliseconds = Random.Next(MinimumDelayInMilliseconds, MaximumDelayInMilliseonds);
      await Task.Delay(delayInMilliseconds, stoppingToken);
    }
  }

  private async Task PublishMessageStockLevelChanged(CancellationToken cancellationToken)
  {
    if (!_mqttClient.IsConnected)
      return;

    if (_logger.IsEnabled(LogLevel.Information))
    {
      _logger.LogInformation(
        "Emitting Stock Level Decrease: {productName} decreased by {stockDecreaseAmount} at {time}",
        _stockLevelEmitterSettings.ProductName,
        _stockLevelEmitterSettings.StockDecreaseAmount,
        DateTimeOffset.Now);
    }

    using var activity = InstrumentationSources.ActivitySource.StartActivity("Emitting Stock Level Decrease");
    activity?.SetBaggage("product.name", _stockLevelEmitterSettings.ProductName);
    activity?.SetTag("product.name", _stockLevelEmitterSettings.ProductName);
    activity?.SetTag("product.stock_decrease_amount", _stockLevelEmitterSettings.StockDecreaseAmount);

    var message = new MqttMessageStockLevelChanged(_stockLevelEmitterSettings.ProductName, _stockLevelEmitterSettings.StockDecreaseAmount);
    var mqttMessage = MqttApplicationMessageProvider.Get(
      _mqttSettings.TopicNamePublish ?? throw new InvalidOperationException($"No {nameof(IMqttSettings.TopicNamePublish)} specified."),
      message);
    await _mqttClient.PublishAsync(mqttMessage, cancellationToken);
  }
}
