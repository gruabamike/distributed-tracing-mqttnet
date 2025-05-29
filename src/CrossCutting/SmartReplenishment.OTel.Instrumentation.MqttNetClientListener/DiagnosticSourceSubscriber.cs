using System.Diagnostics;

namespace SmartReplenishment.OTel.Instrumentation.MqttNetClientListener;

internal class DiagnosticSourceSubscriber : IDisposable, IObserver<DiagnosticListener>
{
  private readonly Func<string, DiagnosticSourceListenerBase> _handlerFactory;
  private readonly Func<DiagnosticListener, bool> _diagnosticSourceFilter;
  private readonly Func<string, object?, object?, bool>? _isEnabledFilter;
  
  private readonly List<IDisposable> _listenerSubscriptions;
  private IDisposable? _allSourcesSubscription;
  private long _disposed;

  public DiagnosticSourceSubscriber(
    DiagnosticSourceListenerBase handler,
    Func<string, object?, object?, bool>? isEnabledFilter)
    : this(_ => handler, value => handler.SourceName == value.Name, isEnabledFilter)
  {
  }

  public DiagnosticSourceSubscriber(
    Func<string, DiagnosticSourceListenerBase> handlerFactory,
    Func<DiagnosticListener, bool> diagnosticSourceFilter,
    Func<string, object?, object?, bool>? isEnabledFilter)
  {
    ArgumentNullException.ThrowIfNull(handlerFactory);

    _listenerSubscriptions = new List<IDisposable>();
    _handlerFactory = handlerFactory;
    _diagnosticSourceFilter = diagnosticSourceFilter;
    _isEnabledFilter = isEnabledFilter;
  }

  public void Subscribe()
  {
    if (_allSourcesSubscription == null)
    {
      _allSourcesSubscription = DiagnosticListener.AllListeners.Subscribe(this);
    }
  }

  public void OnNext(DiagnosticListener value)
  {
    if ((Interlocked.Read(ref _disposed) == 0) && _diagnosticSourceFilter(value))
    {
      var handler = _handlerFactory(value.Name);
      var listener = new DiagnosticSourceObserver(handler);
      var subscription = _isEnabledFilter == null ?
          value.Subscribe(listener!) :
          value.Subscribe(listener!, _isEnabledFilter);

      lock (_listenerSubscriptions)
      {
        _listenerSubscriptions.Add(subscription);
      }
    }
  }

  public void OnCompleted()
  {
    throw new NotImplementedException("No further implementation required.");
  }

  public void OnError(Exception error)
  {
    throw new NotImplementedException("No further implementation required.");
  }

  public void Dispose()
  {
    this.Dispose(true);
    GC.SuppressFinalize(this);
  }

  protected virtual void Dispose(bool disposing)
  {
    if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 1)
    {
      return;
    }

    lock (_listenerSubscriptions)
    {
      foreach (var listenerSubscription in _listenerSubscriptions)
      {
        listenerSubscription?.Dispose();
      }

      _listenerSubscriptions.Clear();
    }

    _allSourcesSubscription?.Dispose();
    _allSourcesSubscription = null;
  }
}
