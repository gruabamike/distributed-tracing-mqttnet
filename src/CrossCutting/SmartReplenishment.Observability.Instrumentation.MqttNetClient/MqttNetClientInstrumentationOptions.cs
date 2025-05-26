using System.Diagnostics;

namespace SmartReplenishment.Observability.Instrumentation.MqttNetClient;

public class MqttNetClientInstrumentationOptions
{
  /// <summary>
  /// Gets or sets an action to enrich an Activity.
  /// </summary>
  public Action<Activity, string, object>? Enrich { get; set; }

  /// <summary>
  /// Gets or sets a value indicating whether the exception will be recorded as ActivityEvent or not.
  /// Default value: False.
  /// </summary>
  public bool RecordException { get; set; }
}