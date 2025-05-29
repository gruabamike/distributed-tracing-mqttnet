using MQTTnet.Packets;
using OpenTelemetry.Context.Propagation;

namespace SmartReplenishment.OTel.Instrumentation.MqttNetClientDecorator;

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
      yield break;

    int userPropertiesCount = userProperties.Count;
    for (int i = 0; i < userPropertiesCount; i++)
    {
      var userProperty = userProperties[i];
      if (userProperty.Name.Equals(key, StringComparison.Ordinal))
        yield return userProperty.Value;
    }
  }
}
