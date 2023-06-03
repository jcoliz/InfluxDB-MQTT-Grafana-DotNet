// Copyright (C) 2023 James Coliz, Jr. <jcoliz@outlook.com> All rights reserved
// Use of this source code is governed by the MIT license (see LICENSE file)

namespace MqttSender.Messages;

public class AirQualtityMetrics
{
    public static readonly string measurement = "air";
    public double aqi { get; init; }
    public double co { get; init; }
    public double no2 { get; init; }
}
