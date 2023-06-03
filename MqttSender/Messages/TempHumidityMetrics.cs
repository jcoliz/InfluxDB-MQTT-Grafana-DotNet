// Copyright (C) 2023 James Coliz, Jr. <jcoliz@outlook.com> All rights reserved
// Use of this source code is governed by the MIT license (see LICENSE file)

namespace MqttSender.Messages;

public class TempHumidityMetrics
{
    public static readonly string measurement = "th";
    public double temp { get; init; }
    public double humidity { get; init; }
}
