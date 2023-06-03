// Copyright (C) 2023 James Coliz, Jr. <jcoliz@outlook.com> All rights reserved
// Use of this source code is governed by the MIT license (see LICENSE file)

using MqttSender;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context,services) =>
    {
        services.AddHostedService<Worker>();
        services.Configure<MqttOptions>(context.Configuration.GetSection(MqttOptions.Section));
    })
    .Build();

host.Run();
