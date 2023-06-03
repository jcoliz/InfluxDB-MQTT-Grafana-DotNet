namespace MqttSender.Messages;

public class AirQualtityMetrics
{
    public static readonly string measurement = "air";
    public double aqi { get; init; }
    public double co { get; init; }
    public double no2 { get; init; }
}
