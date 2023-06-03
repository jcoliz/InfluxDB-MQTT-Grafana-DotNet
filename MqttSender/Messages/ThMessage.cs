namespace MqttSender.Messages;

public record MessagePayload
{
    public long time { get; init; }
    public object? metrics { get; init; }
}
