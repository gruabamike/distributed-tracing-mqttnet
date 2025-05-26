using MQTTnet.Packets;
using OpenTelemetry.Context.Propagation;

namespace SmartReplenishment.Observability.Instrumentation.MqttNetClient;

internal static class MqttNetClientContextPropagation
{
  internal static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;

  internal static void Inject(IList<MqttUserProperty>? userProperties, string key, string value)
  {
    userProperties?.Add(new MqttUserProperty(key, value));
  }

  internal static IEnumerable<string> Extract(IList<MqttUserProperty>? userProperties, string key)
  {
    if (userProperties is null)
      return Enumerable.Empty<string>();

    return userProperties
      .Where(prop => prop.Name.Equals(key, StringComparison.Ordinal))
      .Select(prop => prop.Value);
  }
}
