namespace SmartReplenishment.Messaging.Mqtt;

public interface IMqttSettings
{
  public const string MqttSettingsKey = "MqttSettings";

  string Host { get; set; }
  int? Port { get; set; }
  string Username { get; set; }
  string Password { get; set; }
  string ClientId { get; set; }
  string? TopicNamePublish { get; set; }
  string? TopicNameSubscribe { get; set; }
}
