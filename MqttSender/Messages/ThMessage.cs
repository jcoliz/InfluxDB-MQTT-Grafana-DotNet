// Copyright (C) 2023 James Coliz, Jr. <jcoliz@outlook.com> All rights reserved
// Use of this source code is governed by the MIT license (see LICENSE file)

namespace MqttSender.Messages;

public record MessagePayload
{
    public long time { get; init; }
    public object? metrics { get; init; }
}
