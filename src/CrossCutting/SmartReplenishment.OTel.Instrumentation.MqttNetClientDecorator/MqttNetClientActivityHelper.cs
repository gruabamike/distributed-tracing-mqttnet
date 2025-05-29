using System.Diagnostics;
using System.Reflection;

namespace SmartReplenishment.OTel.Instrumentation.MqttNetClientDecorator;

internal sealed class MqttNetClientActivityHelper
{
  private static readonly AssemblyName AssemblyName = typeof(MqttNetClientActivityHelper).Assembly.GetName();

  public static readonly string ActivitySourceName = AssemblyName.Name!;
  public static readonly string? ActivitySourceVersion = AssemblyName.Version?.ToString();
  public static readonly ActivitySource ActivitySource = new(ActivitySourceName, ActivitySourceVersion);

  public const string MqttNetClientSystemName = "mqttnet.client";

  public static readonly IEnumerable<KeyValuePair<string, object>> ActivityCreationTags =
    [
        new KeyValuePair<string, object>("TraceSemanticConventions", "hi" ),
    ];
}
