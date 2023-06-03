namespace MqttSender;

public record TempHumidityMessage
{
    public long time { get; init; }
    public double temp { get; init; }
    public double humidity { get; init; }
}