using SmartReplenishment.Messaging.Mqtt;
using SmartReplenishment.Services.Replenishment.BackgroundServices;

var builder = Host.CreateApplicationBuilder(args);

// Mqtt Settings
IMqttSettings? mqttSettings = builder.Configuration
  .GetRequiredSection(IMqttSettings.MqttSettingsKey)
  .Get<MqttSettings>();

ArgumentNullException.ThrowIfNull(mqttSettings);

// Register Services
builder.Services.AddSingleton<IMqttSettings>(mqttSettings);
builder.AddServiceDefaults();

// Background Services
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
