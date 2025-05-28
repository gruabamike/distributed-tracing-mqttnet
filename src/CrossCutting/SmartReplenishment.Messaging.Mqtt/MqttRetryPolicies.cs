using Microsoft.Extensions.Logging;
using MQTTnet;
using Polly;
using Polly.Retry;

namespace SmartReplenishment.Messaging.Mqtt;

public static class MqttRetryPolicies
{
  public static AsyncRetryPolicy<MqttClientConnectResult> GetMqttConnectRetryPolicy(ILogger logger, int retryCount = 5)
  {
    return Policy
        .HandleResult<MqttClientConnectResult>(res => res.ResultCode != MqttClientConnectResultCode.Success)
        .WaitAndRetryAsync(
            retryCount,
            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            (result, timeSpan, currentRetryCount, context) =>
            {
              logger.LogWarning("Retry {RetryCount} to connect to MQTT broker failed. Waiting {Delay}s.",
                      currentRetryCount, timeSpan.TotalSeconds);
            });
  }
}
