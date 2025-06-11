namespace SmartReplenishment.OTel.Instrumentation.MqttNetClientListener;

internal sealed class MqttClientInstrumentation : IDisposable
{
  private readonly DiagnosticSourceSubscriber _diagnosticSourceSubscriber;

  public MqttClientInstrumentation()
  {
    _diagnosticSourceSubscriber = new DiagnosticSourceSubscriber(
      name => new DiagnosticSourceListenerMqttClient(name),
      listener => listener.Name == DiagnosticSourceListenerMqttClient.DiagnosticSourceName,
      null);
    _diagnosticSourceSubscriber.Subscribe();
  }

  public void Dispose() => _diagnosticSourceSubscriber?.Dispose();
}
