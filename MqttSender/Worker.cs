using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MqttSender.Messages;
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
                await SendMessages(stoppingToken);
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

    private async Task SendMessages(CancellationToken stoppingToken)
    {
        try
        {
            var time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var th = new TempHumidityMetrics()
            {
                temp = Math.Round((time / 100.0) % 100.0,2),
                humidity = Math.Round((time / 1000.0) % 100.0,2)
            };
            await SendMeasurement(TempHumidityMetrics.measurement, th, stoppingToken);

            var aqi = new AirQualtityMetrics()
            {
                aqi = Math.Round((time / 100.0) % 100.0,2),
                co = Math.Round((time / 1500.0) % 100.0,2),
                no2 = Math.Round((time / 2500.0) % 100.0,2)
            };
            await SendMeasurement(AirQualtityMetrics.measurement, aqi, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,"Message: Failed");
        }
    }

    private async Task SendMeasurement(string measurement, object metrics, CancellationToken stoppingToken)
    {
        var topic = $"{_mqttoptions.Value.Topic}{measurement}/{deviceid}";
        var time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var payload = new MessagePayload()
        {
            time = time,
            metrics = metrics
        };
        var json = JsonSerializer.Serialize(payload);
        
        var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(json)
            .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
            .WithRetainFlag()
            .Build();

        await mqttClient!.InternalClient.PublishAsync(message, stoppingToken);

        _logger.LogInformation("Message: Sent {topic} {message}", topic, json);
    }
}
