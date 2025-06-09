using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace SmartReplenishment.OTel.Instrumentation.MqttNetClientListener;

internal static class AnonymousObjectPropertyFetcher
{
  public static bool TryGetProperty<T>(object? payload, string propertyName, [NotNullWhen(true)] out T? value)
  {
    value = default;

    if (payload == null)
      return false;

    var type = payload.GetType();
    var property = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);

    if (property == null || !typeof(T).IsAssignableFrom(property.PropertyType))
      return false;

    var rawValue = property.GetValue(payload);
    if (rawValue is T typedValue)
    {
      value = typedValue;
      return true;
    }

    return false;
  }
}
