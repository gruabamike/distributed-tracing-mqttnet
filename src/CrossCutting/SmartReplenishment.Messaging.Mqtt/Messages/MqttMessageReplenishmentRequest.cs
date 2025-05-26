namespace SmartReplenishment.Messaging.Mqtt.Messages;

public record MqttMessageReplenishmentRequest(string ProductId, int StockIncreaseAmount) : IMqttMessage;
