using MQTTnet;
using MQTTnet.Diagnostics.PacketInspection;

namespace SmartReplenishment.OTel.Instrumentation.MqttNetClientDecorator;

public abstract class MqttClientDecorator : IMqttClient
{
  private bool _disposedValue;
  protected readonly IMqttClient _mqttClient;

  protected MqttClientDecorator(IMqttClient mqttClient)
  {
    _mqttClient = mqttClient;

    // delegate events from the inner mqttClient to the methods that can be extended
    _mqttClient.ApplicationMessageReceivedAsync += async e => await OnApplicationMessageReceivedAsync(e);
    _mqttClient.ConnectedAsync += async e => await OnConnectedAsync(e);
    _mqttClient.ConnectingAsync += async e => await OnConnectingAsync(e);
    _mqttClient.DisconnectedAsync += async e => await OnDisconnectedAsync(e);
    _mqttClient.InspectPacketAsync += async e => await OnInspectPacketAsync(e);
  }

  public bool IsConnected => _mqttClient.IsConnected;
  public MqttClientOptions Options => _mqttClient.Options;

  public event Func<MqttApplicationMessageReceivedEventArgs, Task> ApplicationMessageReceivedAsync;
  public event Func<MqttClientConnectedEventArgs, Task> ConnectedAsync;
  public event Func<MqttClientConnectingEventArgs, Task> ConnectingAsync;
  public event Func<MqttClientDisconnectedEventArgs, Task> DisconnectedAsync;
  public event Func<InspectMqttPacketEventArgs, Task> InspectPacketAsync;

  public virtual Task<MqttClientConnectResult> ConnectAsync(MqttClientOptions options, CancellationToken cancellationToken = default)
    => _mqttClient.ConnectAsync(options, cancellationToken);

  public virtual Task DisconnectAsync(MqttClientDisconnectOptions options, CancellationToken cancellationToken = default)
    => _mqttClient.DisconnectAsync(options, cancellationToken);

  public virtual Task PingAsync(CancellationToken cancellationToken = default)
    => _mqttClient.PingAsync(cancellationToken);

  public virtual Task<MqttClientPublishResult> PublishAsync(MqttApplicationMessage applicationMessage, CancellationToken cancellationToken = default)
    => _mqttClient.PublishAsync(applicationMessage, cancellationToken);

  public virtual Task SendEnhancedAuthenticationExchangeDataAsync(MqttEnhancedAuthenticationExchangeData data, CancellationToken cancellationToken = default)
    => _mqttClient.SendEnhancedAuthenticationExchangeDataAsync(data, cancellationToken);

  public virtual Task<MqttClientSubscribeResult> SubscribeAsync(MqttClientSubscribeOptions options, CancellationToken cancellationToken = default)
    => _mqttClient.SubscribeAsync(options, cancellationToken);

  public virtual Task<MqttClientUnsubscribeResult> UnsubscribeAsync(MqttClientUnsubscribeOptions options, CancellationToken cancellationToken = default)
    => _mqttClient.UnsubscribeAsync(options, cancellationToken);

  protected virtual async Task OnApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
  {
    if (ApplicationMessageReceivedAsync != null)
      await ApplicationMessageReceivedAsync.Invoke(e);
  }

  protected virtual async Task OnConnectedAsync(MqttClientConnectedEventArgs e)
  {
    if (ConnectedAsync != null)
      await ConnectedAsync.Invoke(e);
  }

  protected virtual async Task OnConnectingAsync(MqttClientConnectingEventArgs e)
  {
    if (ConnectingAsync != null)
      await ConnectingAsync.Invoke(e);
  }

  protected virtual async Task OnDisconnectedAsync(MqttClientDisconnectedEventArgs e)
  {
    if (DisconnectedAsync != null)
      await DisconnectedAsync.Invoke(e);
  }

  protected virtual async Task OnInspectPacketAsync(InspectMqttPacketEventArgs e)
  {
    if (InspectPacketAsync != null)
      await InspectPacketAsync.Invoke(e);
  }

  protected virtual void Dispose(bool disposing)
  {
    if (!_disposedValue)
    {
      if (disposing)
      {
        // dispose managed objects here
        _mqttClient.Dispose();
      }

      // dispose unmanaged objects here
      _disposedValue = true;
    }
  }

  // Optional: Only implement if there are unmanaged resources
  // ~MqttClientDecorator()
  // {
  //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
  //     Dispose(disposing: false);
  // }

  public void Dispose()
  {
    // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    Dispose(disposing: true);
    GC.SuppressFinalize(this);
  }
}
