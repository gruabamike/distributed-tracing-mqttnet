using MQTTnet;
using SmartReplenishment.Messaging.Mqtt;
using SmartReplenishment.Messaging.Mqtt.Messages;
using System.Text.Json;
using System.Text;

namespace SmartReplenishment.Services.Replenishment.BackgroundServices;

public class Worker : BackgroundService
{
  private readonly ILogger<Worker> _logger;
  private readonly IMqttClient _mqttClient;
  private readonly IMqttSettings _mqttSettings;

  public Worker(ILogger<Worker> logger, IMqttClient mqttClient, IMqttSettings mqttSettings)
  {
    _logger = logger;
    _mqttClient = mqttClient;
    _mqttSettings = mqttSettings;
  }

  public override async Task StartAsync(CancellationToken cancellationToken)
  {
    _mqttClient.ApplicationMessageReceivedAsync += OnApplicationMessageReceivedAsync;

    var mqttClientOptions = MqttClientOptionsProvider.Get(_mqttSettings);
    await _mqttClient.ConnectAsync(mqttClientOptions, cancellationToken);

    var mqttSubscribeOptions = MqttClientSubscribeOptionsProvider.Get(_mqttSettings.TopicNameSubscribe);
    await _mqttClient.SubscribeAsync(mqttSubscribeOptions, cancellationToken);


    await base.StartAsync(cancellationToken);
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    while (!stoppingToken.IsCancellationRequested)
    {
      //if (_logger.IsEnabled(LogLevel.Information))
      //{
      //  _logger.LogInformation(
      //    "Emitting Stock Level Decrease: {productName} decreased by {stockDecreaseAmount} at {time}",
      //    _stockLevelEmitterSettings.ProductName,
      //    _stockLevelEmitterSettings.StockDecreaseAmount,
      //    DateTimeOffset.Now);
      //}

      var mqttClientOptions = MqttClientOptionsProvider.Get(_mqttSettings);
      var response = await _mqttClient.ConnectAsync(mqttClientOptions, stoppingToken);
      if (response.ResultCode != MqttClientConnectResultCode.Success)
        continue;

      using var activity = InstrumentationSources.ActivitySource.StartActivity("Emitting Stock Level Decrease");
      activity?.SetBaggage("product.name", _stockLevelEmitterSettings.ProductName);
      activity?.SetTag("product.name", _stockLevelEmitterSettings.ProductName);
      activity?.SetTag("product.stock_decrease_amount", _stockLevelEmitterSettings.StockDecreaseAmount);

      var message = new MqttMessageStockLevelChanged(_stockLevelEmitterSettings.ProductName, _stockLevelEmitterSettings.StockDecreaseAmount);
      var mqttMessage = MqttApplicationMessageProvider.Get(
        _mqttSettings.TopicNamePublish ?? throw new InvalidOperationException($"No {nameof(IMqttSettings.TopicNamePublish)} specified."),
        message);
      await _mqttClient.PublishAsync(mqttMessage, stoppingToken);
      await _mqttClient.DisconnectAsync();
      await Task.Delay(1000, stoppingToken);
    }
  }

  private async Task OnApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args)
  {
    var payloadString = Encoding.UTF8.GetString(args.ApplicationMessage.Payload);
    var message = JsonSerializer.Deserialize<MqttMessageStockLow>(payloadString);
    if (message is null)
    {
      return;
    }

    // Simulate some work
    await Task.Delay(350);

    await PublishMessageReplenishmentRequest(message.ProductName);
  }

  private async Task PublishMessageReplenishmentRequest(string productName)
  {
    if (_mqttClient.IsConnected)
      return;

    var message = new MqttMessageReplenishmentRequest(productName, 100);
    var mqttMessage = MqttApplicationMessageProvider.Get(
      _mqttSettings.TopicNamePublish ?? throw new InvalidOperationException($"No {nameof(IMqttSettings.TopicNamePublish)} specified."),
      message);
    await _mqttClient.PublishAsync(mqttMessage);
  }
}
