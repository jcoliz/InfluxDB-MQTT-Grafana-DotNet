namespace MqttSender.Messages;

public class TempHumidityMetrics
{
    public static readonly string measurement = "th";
    public double temp { get; init; }
    public double humidity { get; init; }
}
