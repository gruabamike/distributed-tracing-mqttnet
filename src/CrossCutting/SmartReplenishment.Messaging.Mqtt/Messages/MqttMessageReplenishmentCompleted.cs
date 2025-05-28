namespace SmartReplenishment.Messaging.Mqtt.Messages;

public record MqttMessageReplenishmentCompleted(Guid ArticleId, string ArticleName, int StockIncreaseAmount) : IMqttMessage;
