namespace SmartReplenishment.Messaging.Mqtt;

public sealed class MqttSettings : IMqttSettings
{
  public required string Host { get; set; }

  public int? Port { get; set; }

  public required string Username { get; set; }

  public required string Password { get; set; }

  public required string ClientId { get; set; }

  public string? TopicNamePublish { get; set; }

  public string? TopicNameSubscribe { get; set; }
}
