using MQTTnet;

namespace SmartReplenishment.Services.SMSNotification.BackgroundServices;

public class Worker : BackgroundService
{
  private readonly ILogger<Worker> _logger;

  public Worker(ILogger<Worker> logger)
  {
    _logger = logger;
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    var mqttFactory = new MqttClientFactory();
    using var mqttClient = mqttFactory.CreateMqttClient();
    var mqttClientOptions = new MqttClientOptionsBuilder()
        .WithTcpServer("localhost")
        .WithCredentials("root", "root")
        .WithClientId(nameof(SMSNotification))
        .WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V500)
        .Build();

    mqttClient.ApplicationMessageReceivedAsync += OnApplicationMessageReceivedAsync;
    await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

    var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder().
        WithTopicFilter("replenishment/confirmation")
        .Build();

    await mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);

    while (!stoppingToken.IsCancellationRequested)
    {
      if (_logger.IsEnabled(LogLevel.Information))
      {
        _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
      }
      await Task.Delay(1000, stoppingToken);
    }
  }

  private Task OnApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
  {
    _logger.LogInformation("Received application message");
    _logger.LogInformation(e.ApplicationMessage.UserProperties.FirstOrDefault()?.Value ?? "no user property");
    return Task.CompletedTask;
  }

}
