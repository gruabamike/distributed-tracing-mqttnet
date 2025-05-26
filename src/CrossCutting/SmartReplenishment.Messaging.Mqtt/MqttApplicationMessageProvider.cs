using MQTTnet;
using MQTTnet.Protocol;
using SmartReplenishment.Messaging.Mqtt.Messages;
using System.Text.Json;

namespace SmartReplenishment.Messaging.Mqtt;

public static class MqttApplicationMessageProvider
{
  private static readonly JsonSerializerOptions JsonSerializerOptionsDefault = new()
  {
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    WriteIndented = true,
  };

  public static MqttApplicationMessage Get(
    string topic,
    IMqttMessage message,
    MqttQualityOfServiceLevel qos = MqttQualityOfServiceLevel.ExactlyOnce)
    =>
    new MqttApplicationMessageBuilder()
    .WithTopic(topic)
    .WithPayload(JsonSerializer.Serialize(message, JsonSerializerOptionsDefault))
    .WithQualityOfServiceLevel(qos)
    .Build();
}
