using System.Diagnostics;

namespace SmartReplenishment.OTel.Instrumentation.MqttNetClientListener;

internal sealed class DiagnosticSourceListenerMqttNetClient : DiagnosticSourceListenerBase
{
  internal static readonly string ActivitySourceName = typeof(DiagnosticSourceListenerMqttNetClient).Assembly.GetName().Name!;

  internal const string DiagnosticSourceName = "MQTTnet.MqttClient";

  internal const string EventNamePrefix = "MQTTnet.MqttClient";

  public const string EventNameMessagePublishBefore = $"{EventNamePrefix}.WriteMessagePublishBefore";
  public const string EventNameMessagePublishAfter = $"{EventNamePrefix}.WriteMessagePublishAfter";
  public const string EventNameMessageReceiveBefore = $"{EventNamePrefix}.WriteMessageReceiveBefore";
  public const string EventNameMessageReceiveAfter = $"{EventNamePrefix}.WriteMessageReceiveAfter";

  public const string EventNameMessagePublishError = $"{EventNamePrefix}.WriteMessagePublishError";
  public const string EventNameMessageReceiveError = $"{EventNamePrefix}.WriteMessageReceiveError";

  public override bool SupportsNullActivity => true;

  private readonly MqttNetClientInstrumentationOptions _options;

  public DiagnosticSourceListenerMqttNetClient(string sourceName, MqttNetClientInstrumentationOptions? options) : base(sourceName)
  {
    _options = options ?? new();
  }

  public override void OnCustom(string name, Activity? activity, object? payload)
  {
    
  }
}
