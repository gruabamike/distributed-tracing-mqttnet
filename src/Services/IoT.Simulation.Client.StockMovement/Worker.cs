using MQTTnet;

namespace IoT.Simulation.Client.StockMovement
{
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
                .WithClientId(nameof(StockMovement))
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
                    .WithTopic("stock-movement")
                    .WithUserProperty("hi", Guid.NewGuid().ToString("N"))
                    .WithPayload("Das ist eine Testnachricht")
                    .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce)
                    .Build();

                await mqttClient.PublishAsync(applicationMessage, stoppingToken);
                await mqttClient.DisconnectAsync();

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
