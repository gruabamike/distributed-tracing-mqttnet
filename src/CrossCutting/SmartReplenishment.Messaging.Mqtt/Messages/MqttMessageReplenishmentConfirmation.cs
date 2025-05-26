namespace SmartReplenishment.Messaging.Mqtt.Messages;

public record MqttMessageReplenishmentConfirmation(string ProductId, int Quantity) : IMqttMessage;
