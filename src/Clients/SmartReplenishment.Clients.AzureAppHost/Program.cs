using Projects;

var builder = DistributedApplication.CreateBuilder(args);

//var mqttBroker = builder.AddContainer("mqtt-broker", "eclipse-mosquitto", "2.0.20")
//    .WithEndpoint(port: 1883, targetPort: 1883)
//    .WithBindMount("../mosquitto-config", "/mosquitto/config")
//    .WithVolume("mosquitto-data", "/mosquitto/data")
//    .WithVolume("mosquitto-log", "/mosquitto/log");

//var postgres = builder.AddPostgres("postgres")
//    .WithPgAdmin(pgAdmin => pgAdmin.WithHostPort(5050));
//var inventoryDb = postgres.AddDatabase("inventorydb");

//var simulation01 = builder.AddProject<SmartReplenishment_Clients_StockLevelEmitter>("simulation01")
//    .WaitFor(mqttBroker);
//.WithEnvironment("SIMULATION_ID", "1");
//.WithEnvironment("CONFIG_PATH", "appsettings.sim1.json");

//var inventoryService = builder.AddProject<SmartReplenishment_Services_InventoryService>("inventorysvc")
//    .WithReference(inventoryDb)
//    .WaitFor(mqttBroker);

//var fullfillmentService = builder.AddProject<SmartReplenishment_Services_FullfillmentService>("fullfillmentsvc")
//    .WaitFor(mqttBroker);

builder.Build().Run();
