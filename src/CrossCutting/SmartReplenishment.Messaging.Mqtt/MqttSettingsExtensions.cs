namespace SmartReplenishment.Messaging.Mqtt;

public static class MqttSettingsExtensions
{
  public static string GetRequiredTopicNamePublish(this IMqttSettings mqttSettings) =>
    mqttSettings.TopicNamePublish ?? throw new InvalidOperationException($"{nameof(IMqttSettings.TopicNamePublish)} must be set in the configuration");

  public static string GetRequiredTopicNameSubscribe(this IMqttSettings mqttSettings) =>
  mqttSettings.TopicNameSubscribe ?? throw new InvalidOperationException($"{nameof(IMqttSettings.TopicNameSubscribe)} must be set in the configuration");
}
