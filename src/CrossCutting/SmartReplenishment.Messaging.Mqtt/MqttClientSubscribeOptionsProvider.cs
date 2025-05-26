using MQTTnet;

namespace SmartReplenishment.Messaging.Mqtt;

public static class MqttClientSubscribeOptionsProvider
{
  public static MqttClientSubscribeOptions Get(string topic) =>
    new MqttClientSubscribeOptionsBuilder()
    .WithTopicFilter(topic)
    .Build();
}
