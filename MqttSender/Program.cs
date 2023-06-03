using MqttSender;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context,services) =>
    {
        services.AddHostedService<Worker>();
        services.Configure<MqttOptions>(context.Configuration.GetSection(MqttOptions.Section));
    })
    .Build();

host.Run();
