namespace SmartReplenishment.Messaging.Mqtt.Messages;

public record MqttMessageStockLow(string ProductName, int LowStockAmount) : IMqttMessage;
