using MQTTnet;
using System.Net;

namespace SmartReplenishment.OTel.Instrumentation.MqttNetClientListener;

internal static class MqttClientChannelOptionsExtensions
{
  public static string? GetHost(this IMqttClientChannelOptions options) =>
    ((options as MqttClientTcpOptions)?.RemoteEndpoint) switch
    {
      DnsEndPoint dns => dns.Host,
      IPEndPoint ip => ip.Address.ToString(),
      _ => null
    };

  public static int? GetPort(this IMqttClientChannelOptions options) =>
    ((options as MqttClientTcpOptions)?.RemoteEndpoint) switch
    {
      DnsEndPoint dns => dns.Port != 0 ? dns.Port : options.GetDefaultPort(),
      IPEndPoint ip => ip.Port != 0 ? ip.Port : options.GetDefaultPort(),
      _ => null
    };

  private static int GetDefaultPort(this IMqttClientChannelOptions options)
    => options.TlsOptions.UseTls ? 8883 : 1883;
}
