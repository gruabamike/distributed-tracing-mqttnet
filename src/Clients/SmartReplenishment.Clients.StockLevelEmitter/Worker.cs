using MQTTnet;
using System.Text.Json;

namespace SmartReplenishment.Clients.StockLevelEmitter;

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
            .WithClientId(nameof(StockLevelEmitter))
            .WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V500)
            .Build();

        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }

            var response = await mqttClient.ConnectAsync(mqttClientOptions, stoppingToken);
            var applicationMessage = new MqttApplicationMessageBuilder()
                .WithTopic("inventory/stock-levels")
                .WithUserProperty("hi", Guid.NewGuid().ToString("N"))
                .WithPayload(JsonSerializer.Serialize(new { ProductId = "1234", CurrentStock = 15 }, new JsonSerializerOptions()
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                }))
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce)
                .Build();

            await mqttClient.PublishAsync(applicationMessage, stoppingToken);
            await mqttClient.DisconnectAsync();

            await Task.Delay(1000, stoppingToken);
        }
    }
}
