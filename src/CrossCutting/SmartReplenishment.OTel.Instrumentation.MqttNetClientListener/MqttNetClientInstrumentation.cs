namespace SmartReplenishment.OTel.Instrumentation.MqttNetClientListener;

internal class MqttNetClientInstrumentation : IDisposable
{
  private readonly DiagnosticSourceSubscriber _diagnosticSourceSubscriber;

  public MqttNetClientInstrumentation(MqttNetClientInstrumentationOptions? options = null)
  {
    _diagnosticSourceSubscriber = new DiagnosticSourceSubscriber(
      name => new DiagnosticSourceListenerMqttNetClient(name, options),
      listener => listener.Name == DiagnosticSourceListenerMqttNetClient.DiagnosticSourceName,
      null);
    _diagnosticSourceSubscriber.Subscribe();
  }

  public void Dispose()
  {
    _diagnosticSourceSubscriber?.Dispose();
  }
}
