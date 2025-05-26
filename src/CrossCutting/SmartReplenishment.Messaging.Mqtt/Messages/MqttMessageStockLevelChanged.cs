namespace SmartReplenishment.Messaging.Mqtt.Messages;

public record MqttMessageStockLevelChanged(string ProductName, int StockDecreaseAmount) : IMqttMessage;
