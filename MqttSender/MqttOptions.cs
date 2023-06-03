namespace MqttSender;

public class MqttOptions
{
    public const string Section = "Mqtt";

    public string Topic { get; set; } = string.Empty;
    public string? Server { get; set; }
    public int Port { get; set; } = 1883;
}