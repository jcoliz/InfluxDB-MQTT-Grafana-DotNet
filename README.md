# How to: Use InfluxDB with MQTT and Grafana

This repository demonstrates a baseline MTIG (MQTT -> Telegraf -> InfluxDB -> Grafana) stack
using Docker images. I've included an example with a .NET MQTT client, although this will
work great with any MQTT client stack.

## Starting

## MQTT Schema

As an example, we're using a simple MQTT schema:

### Topic

```
example/{measurement}/{device}
```

In InfluxDB terms, the 'measurement' describes what you're measuring, and the schema you're sending. The example
client will send two measurements: `th` (temperature and humidity), and `air` (air quality).

The 'device' describes the machine from which the measurements come. We'll store that in InfluxDB as a tag.
The example client will use the DNS hostname.

### Payload

The example payload here is JSON. The `time` field will be used for the InfluxDB timestamp. The fields in the
`metrics` object will will be passed straight through as fields.

The `th` measurement will show:

```json
{
    "time": 1685760473677,
    "metrics": {
        "temperature": 28.1,
        "humidity": 44.5
    }
}
```

while the `air` measurement gives

```json
{
    "time": 1685760473677,
    "metrics": {
        "aqi": 107.6,
        "co": 53.6,
        "no2": 94.1
    }
}
```
