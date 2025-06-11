using System.Diagnostics;
using System.Reflection;

namespace SmartReplenishment.OTel.Instrumentation.MqttNetClientListener;

internal static class MqttClientActivitySourceProvider
{
  private static readonly AssemblyName AssemblyName = typeof(MqttClientActivitySourceProvider).Assembly.GetName();

  public static readonly string ActivitySourceName = AssemblyName.Name!;
  public static readonly string? ActivitySourceVersion = AssemblyName.Version?.ToString();
  public static readonly ActivitySource ActivitySource = new(ActivitySourceName, ActivitySourceVersion);
}
