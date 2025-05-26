using MQTTnet;

namespace SmartReplenishment.Messaging.Mqtt;

public static class MqttClientOptionsProvider
{
  public static MqttClientOptions Get(IMqttSettings mqttSettings) =>
    new MqttClientOptionsBuilder()
    .WithTcpServer(mqttSettings.Host, mqttSettings.Port)
    .WithCredentials(mqttSettings.Username, mqttSettings.Password)
    .WithClientId(mqttSettings.ClientId)
    .WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V500)
    .Build();
}
