using System.Diagnostics;

namespace SmartReplenishment.OTel.Instrumentation.MqttNetClientListener;

internal class DiagnosticSourceObserver : IObserver<KeyValuePair<string, object>>
{
  private readonly DiagnosticSourceListenerBase _handler;

  public DiagnosticSourceObserver(DiagnosticSourceListenerBase handler)
  {
    _handler = handler ?? throw new ArgumentNullException(nameof(handler));
  }

  public void OnCompleted() { }

  public void OnError(Exception error) { }

  public void OnNext(KeyValuePair<string, object> value)
  {
    if (!_handler.SupportsNullActivity && Activity.Current == null)
      return;

    try
    {
      string key = value.Key;
      var activity = Activity.Current;

      switch (key)
      {
        case string k when k.EndsWith("Start", StringComparison.Ordinal):
          _handler.OnStartActivity(k, activity, value.Value);
          break;
        case string k when k.EndsWith("Stop", StringComparison.Ordinal):
          _handler.OnStopActivity(k, activity, value.Value);
          break;
        case string k when k.EndsWith("Exception", StringComparison.Ordinal):
          _handler.OnException(k, activity, value.Value);
          break;
        default:
          _handler.OnCustom(key, activity, value.Value);
          break;
      }
    }
    catch (Exception ex)
    {
      InstrumentationEventSource.Log.UnknownErrorProcessingEvent(_handler.SourceName, value.Key, ex);
    }
  }
}
