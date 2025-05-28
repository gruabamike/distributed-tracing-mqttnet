namespace SmartReplenishment.Messaging.Mqtt.Messages;

public record MqttMessageStockLow(Guid ArticleId, string ArticleName, int CurrentStockAmount) : IMqttMessage;
