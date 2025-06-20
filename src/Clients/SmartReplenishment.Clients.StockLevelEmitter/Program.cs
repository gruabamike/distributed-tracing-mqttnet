using SmartReplenishment.Clients.StockLevelEmitter.BackgroundServices;
using SmartReplenishment.Clients.StockLevelEmitter.Settings;
using SmartReplenishment.Messaging.Mqtt;

var builder = Host.CreateApplicationBuilder(args);

// Mqtt Settings
IMqttSettings? mqttSettings = builder.Configuration
  .GetRequiredSection(IMqttSettings.MqttSettingsKey)
  .Get<MqttSettings>();

ArgumentNullException.ThrowIfNull(mqttSettings);

// Stock Level Emitter Settings
IStockLevelEmitterSettings? stockLevelEmitterSettings = builder.Configuration
  .GetRequiredSection(IStockLevelEmitterSettings.StockLevelEmitterSettingsKey)
  .Get<StockLevelEmitterSettings>();

ArgumentNullException.ThrowIfNull(stockLevelEmitterSettings);

// Register Services
builder.Services.AddSingleton<IMqttSettings>(mqttSettings);
builder.Services.AddSingleton<IStockLevelEmitterSettings>(stockLevelEmitterSettings);
builder.AddServiceDefaults();

// Background Services
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
