using MQTTnet;
using MQTTnet.Protocol;
using SmartReplenishment.Messaging.Mqtt.Messages;
using System.Text;
using System.Text.Json;

namespace SmartReplenishment.Messaging.Mqtt;

public static class MqttApplicationMessageProvider
{
  private static readonly JsonSerializerOptions JsonSerializerOptionsDefault = new()
  {
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
  };

  public static MqttApplicationMessage GetPublishMqttApplicationMessage<TMessage>(
    string topic,
    TMessage message,
    MqttQualityOfServiceLevel qos = MqttQualityOfServiceLevel.ExactlyOnce) where TMessage : IMqttMessage
    =>
    new MqttApplicationMessageBuilder()
    .WithTopic(topic)
    .WithPayload(JsonSerializer.Serialize(message, JsonSerializerOptionsDefault))
    .WithQualityOfServiceLevel(qos)
    .Build();

  public static TMessage? GetSubscribeMqttApplicationMessage<TMessage>(
    MqttApplicationMessage message)
  {
    var payloadString = Encoding.UTF8.GetString(message.Payload);
    return JsonSerializer.Deserialize<TMessage>(payloadString, JsonSerializerOptionsDefault);
  }
}
