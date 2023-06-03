// Copyright (C) 2023 James Coliz, Jr. <jcoliz@outlook.com> All rights reserved
// Use of this source code is governed by the MIT license (see LICENSE file)

namespace MqttSender;

public class MqttOptions
{
    public const string Section = "Mqtt";

    public string Topic { get; set; } = string.Empty;
    public string? Server { get; set; }
    public int Port { get; set; } = 1883;
}