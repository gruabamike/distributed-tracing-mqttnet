namespace SmartReplenishment.Messaging.Mqtt.Messages;

public record MqttMessageStockLevelChanged(string ArticleName, int StockDecreaseAmount) : IMqttMessage;
