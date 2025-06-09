namespace SmartReplenishment.OTel.Instrumentation.MqttNetClientDecorator;

internal static class TraceSemanticConventions
{
  public const string AttributeMessagingSystem = "messaging.system";
  public const string AttributeMessagingOperationName = "messaging.operation.name";
  public const string AttributeMessagingOperationType = "messaging.operation.type";
  public const string AttributeMessagingDestinationName = "messaging.destination.name";
  public const string AttributeMessagingMessageBodySize = "messaging.message.body.size";
  public const string AttributeMessagingClientId = "messaging.client.id";

  public const string AttributeMessagingMqttRetain = "messaging.mqtt.retain";
  public const string AttributeMessagingMqttQoS = "messaging.mqtt.qos";
  public const string AttributeMessagingMqttTopicAlias = "messaging.mqtt.topic.alias";
  public const string AttributeMessagingMqttProtocolVersion = "messaging.mqtt.protocol.version";

  public const string AttributeServerAddress = "server.address";
  public const string AttributeServerPort = "server.port";

  public const string MessagingSystemMqtt = "mqtt";
  public const string MessagingOperationNamePublish = "publish";
  public const string MessagingOperationNameConsume = "consume";
  public const string MessagingOperationTypeSend = "send";
  public const string MessagingOperationTypeReceive = "receive";
}
