using SmartReplenishment.Messaging.Mqtt;
using SmartReplenishment.Services.NotificationMail.BackgroundServices;

var builder = Host.CreateApplicationBuilder(args);

// Mqtt Settings
IMqttSettings? mqttSettings = builder.Configuration
  .GetRequiredSection(IMqttSettings.MqttSettingsKey)
  .Get<MqttSettings>();

ArgumentNullException.ThrowIfNull(mqttSettings);

// Register Services
builder.Services.AddSingleton<IMqttSettings>(mqttSettings);
builder.AddServiceDefaults();

// Register additional service metric sources
builder.Services.AddOpenTelemetry()
  .WithMetrics(metrics =>
  {
    metrics.AddMeter();
  });

// Background Services
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
