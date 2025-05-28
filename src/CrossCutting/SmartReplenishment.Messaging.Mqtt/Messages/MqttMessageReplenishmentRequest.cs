namespace SmartReplenishment.Messaging.Mqtt.Messages;

public record MqttMessageReplenishmentRequest(Guid ArticleId, string ArticleName, int StockIncreaseAmount) : IMqttMessage;
