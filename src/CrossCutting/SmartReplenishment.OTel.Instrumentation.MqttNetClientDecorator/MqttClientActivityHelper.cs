using MQTTnet;
using System.Diagnostics;
using static SmartReplenishment.OTel.Instrumentation.MqttNetClientDecorator.TraceSemanticConventions;

namespace SmartReplenishment.OTel.Instrumentation.MqttNetClientDecorator;

internal static class MqttClientActivityHelper
{
  public static IEnumerable<KeyValuePair<string, object?>> PublishTags(MqttApplicationMessage msg, MqttClientOptions opt) =>
    [
      .. CommonTags(msg, opt),
      new(AttributeMessagingOperationName, MessagingOperationNamePublish),
      new(AttributeMessagingOperationType, MessagingOperationTypeSend),
    ];

  public static IEnumerable<KeyValuePair<string, object?>> ConsumeTags(MqttApplicationMessage msg, MqttClientOptions opt) =>
    [
      .. CommonTags(msg, opt),
      new(AttributeMessagingOperationName, MessagingOperationNameConsume),
      new(AttributeMessagingOperationType, MessagingOperationTypeReceive),
    ];

  private static IEnumerable<KeyValuePair<string, object?>> CommonTags(MqttApplicationMessage msg, MqttClientOptions opt) =>
    [
      new(AttributeMessagingSystem, MessagingSystemMqtt),
      new(AttributeMessagingDestinationName, msg.Topic),
      new(AttributeServerAddress, opt.ChannelOptions.GetHost()),
      new(AttributeServerPort, opt.ChannelOptions.GetPort()),
    ];

  public static string GetActivityNamePublish(string topic) => $"{MessagingOperationNamePublish} {topic}";
  public static string GetActivityNameConsume(string topic) => $"{MessagingOperationNameConsume} {topic}";

  public static void AddAdditionalTags(Activity act, MqttApplicationMessage msg, MqttClientOptions opt)
  {
    act?.SetTag(AttributeMessagingMessageBodySize, msg.Payload.Length);
    act?.SetTag(AttributeMessagingClientId, opt.ClientId);
    act?.SetTag(AttributeMessagingMqttRetain, msg.Retain);
    act?.SetTag(AttributeMessagingMqttQoS, (int)msg.QualityOfServiceLevel);
    act?.SetTag(AttributeMessagingMqttTopicAlias, msg.TopicAlias);
    act?.SetTag(AttributeMessagingMqttProtocolVersion, (int)opt.ProtocolVersion);
  }
}