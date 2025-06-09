using MQTTnet.Packets;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using System.Diagnostics;

namespace SmartReplenishment.OTel.Instrumentation.MqttNetClientDecorator;

internal static class MqttNetClientContextPropagationHandler
{
  private static readonly TextMapPropagator s_propagator = Propagators.DefaultTextMapPropagator;

  internal static void Inject(ActivityContext context, IList<MqttUserProperty> carrier)
    => s_propagator.Inject(new PropagationContext(context, Baggage.Current), carrier, InjectInternal);

  internal static PropagationContext Extract(IList<MqttUserProperty>? carrier)
  {
    var parentContext = s_propagator.Extract(default, carrier, ExtractInternal);
    Baggage.Current = parentContext.Baggage;
    return parentContext;
  }

  private static void InjectInternal(IList<MqttUserProperty> userProperties, string key, string value)
    => userProperties.Add(new MqttUserProperty(key, value));

  private static IEnumerable<string> ExtractInternal(IList<MqttUserProperty>? userProperties, string key)
    => userProperties?
      .Where(property => property.Name.Equals(key, StringComparison.Ordinal))
      .Select(property => property.Value) ?? [];
}
