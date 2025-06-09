using MQTTnet;
using System.Diagnostics;
using static SmartReplenishment.OTel.Instrumentation.MqttNetClientListener.TraceSemanticConventions;

namespace SmartReplenishment.OTel.Instrumentation.MqttNetClientListener;

internal static class MqttNetClientActivityHelper
{
  public static KeyValuePair<string, object?>[] PublishTags(MqttApplicationMessage msg, MqttClientOptions opt) =>
    [
      .. CommonTags(msg, opt),
      new(AttributeMessagingOperationName, MessagingOperationNamePublish),
      new(AttributeMessagingOperationType, MessagingOperationTypeSend),
    ];

  public static KeyValuePair<string, object?>[] ConsumeTags(MqttApplicationMessage msg, MqttClientOptions opt) =>
    [
      .. CommonTags(msg, opt),
      new(AttributeMessagingOperationName, MessagingOperationNameConsume),
      new(AttributeMessagingOperationType, MessagingOperationTypeReceive),
    ];

  private static KeyValuePair<string, object?>[] CommonTags(MqttApplicationMessage msg, MqttClientOptions opt) =>
    [
      new(AttributeMessagingSystem, MessagingSystemMqtt),
      new(AttributeMessagingDestinationName, msg.Topic),
      new(AttributeServerAddress, opt.ChannelOptions.GetHost()),
      new(AttributeServerPort, opt.ChannelOptions.GetPort()),
    ];

  public static string ActivityNamePublish(string topic) => $"{MessagingOperationNamePublish} {topic}";
  public static string ActivityNameConsume(string topic) => $"{MessagingOperationNameConsume} {topic}";

  public static void AddAdditionalTags(Activity activity, MqttApplicationMessage msg, MqttClientOptions opt)
  {
    activity?.SetTag(AttributeMessagingMessageBodySize, msg.Payload.Length);
    activity?.SetTag(AttributeMessagingClientId, opt.ClientId);
    activity?.SetTag(AttributeMessagingMqttRetain, msg.Retain);
    activity?.SetTag(AttributeMessagingMqttQoS, msg.QualityOfServiceLevel);
    activity?.SetTag(AttributeMessagingMqttTopicAlias, msg.TopicAlias);
    activity?.SetTag(AttributeMessagingMqttProtocolVersion, opt.ProtocolVersion);
  }
}