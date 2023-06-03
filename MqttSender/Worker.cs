using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using System.Text.Json;

namespace MqttSender;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IOptions<MqttOptions> _mqttoptions;
    private readonly IHostApplicationLifetime _lifetime;

    private string? deviceid;
    private IManagedMqttClient? mqttClient;

    public Worker(ILogger<Worker> logger, IHostApplicationLifetime lifetime, IOptions<MqttOptions> mqttoptions)
    {
        _logger = logger;
        _lifetime = lifetime;
        _mqttoptions = mqttoptions;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            Provision();

            await Connect();

            while (!stoppingToken.IsCancellationRequested)
            {
                await SendMessage(stoppingToken);
                await Task.Delay(1000, stoppingToken);
            }

            if (mqttClient is not null)
            {
                mqttClient.Dispose();
            }
        }
        catch (TaskCanceledException)
        {
            _logger.LogInformation("Worker: Stopped");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex,"Worker: Failed");
        }

        await Task.Delay(500);
        _lifetime.StopApplication();
    }

    private void Provision()
    {
        deviceid = System.Net.Dns.GetHostName();
    }

    private async Task Connect()
    {
        if (_mqttoptions.Value?.Server is null)
            throw new ApplicationException("Missing MQTT Server. Please set MQTT__SERVER to hostname of MQTT broker");

        var server = _mqttoptions.Value.Server;
        var port = _mqttoptions.Value.Port;

        MqttClientOptionsBuilder builder = new MqttClientOptionsBuilder()
                                    .WithClientId(deviceid)
                                    .WithTcpServer(server, Convert.ToInt32(port));

        ManagedMqttClientOptions options = new ManagedMqttClientOptionsBuilder()
                                .WithAutoReconnectDelay(TimeSpan.FromSeconds(10))
                                .WithClientOptions(builder.Build())
                                .Build();

        mqttClient = new MqttFactory().CreateManagedMqttClient();

        mqttClient.ConnectedAsync += (e) => 
        {
            _logger.LogInformation("Connection: Connected OK");
            return Task.CompletedTask;
        };
        mqttClient.ConnectingFailedAsync += (e) => 
        {
            _logger.LogError(e.Exception,"Connection: Failed");
            return Task.CompletedTask;
        };
        mqttClient.DisconnectedAsync += (e) => 
        {
            _logger.LogInformation("Connection: Disconnected {reason}",e.ReasonString);
            return Task.CompletedTask;
        };

        await mqttClient.StartAsync(options);

        _logger.LogDebug("Connection: Connecting on {server}:{port}",server,port);
    }

    private async Task SendMessage(CancellationToken stoppingToken)
    {
        var topic = $"{_mqttoptions.Value.Topic}th/{deviceid}";

        var time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var payload = new TempHumidityMessage()
        {
            time = time,
            temp = (time / 100.0) % 100.0,
            humidity = (time / 1000.0) % 100.0
        };
        var json = JsonSerializer.Serialize(payload);
        
        var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(json)
            .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
            .WithRetainFlag()
            .Build();

        await mqttClient!.InternalClient.PublishAsync(message, stoppingToken);

        _logger.LogInformation("Sent: {topic} {message}", topic, json);
    }
}
