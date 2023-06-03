# How to: Use InfluxDB with MQTT and Grafana in containers

[![Build Status](https://dev.azure.com/jcoliz/InfluxDB-MQTT-Grafana-DotNet/_apis/build/status%2FMqttSender%20CI?repoName=jcoliz%2FInfluxDB-MQTT-Grafana-DotNet&branchName=main)](https://dev.azure.com/jcoliz/InfluxDB-MQTT-Grafana-DotNet/_build/latest?definitionId=44&repoName=jcoliz%2FInfluxDB-MQTT-Grafana-DotNet&branchName=main)

This repository demonstrates a baseline MTIG (MQTT -> Telegraf -> InfluxDB -> Grafana) stack
using Docker images. I've included an example with a .NET MQTT client sending sample data, although this will
work great with any MQTT client.

![Example Grafana Dashboard](/docs/images/dashboard-x3.png)

## Starting

It's simple to get started! Just bring up the docker composition with the usual `docker compose up`. If this is your first time running, I suggest you also bring up the sample data senders so you have data to watch.

```
docker compose -f .\docker-compose.yml -f .\docker-senders.yml up
```

This brings up a replica set of three sender clients, as well as the
full stack. Have a look at the logs, to see the senders at work:

```
influxdb-mqtt-grafana-dotnet-sender-1  | <6> [ 03/06/2023 21:19:22 ] MqttSender.Worker[0] Message: Sent example/th/fa1ee4653730 {"time":1685827162457,"metrics":{"temp":15.745,"humidity":56.42}}
influxdb-mqtt-grafana-dotnet-sender-1  | <6> [ 03/06/2023 21:19:22 ] MqttSender.Worker[0] Message: Sent example/air/fa1ee4653730 {"time":1685827162458,"metrics":{"aqi":77.368,"co":103.247,"no2":152.255}}
influxdb-mqtt-grafana-dotnet-sender-2  | <6> [ 03/06/2023 21:19:22 ] MqttSender.Worker[0] Message: Sent example/th/c0d1f62b98ea {"time":1685827162716,"metrics":{"temp":24.102,"humidity":58.113}}
influxdb-mqtt-grafana-dotnet-sender-2  | <6> [ 03/06/2023 21:19:22 ] MqttSender.Worker[0] Message: Sent example/air/c0d1f62b98ea {"time":1685827162716,"metrics":{"aqi":96.144,"co":1.579,"no2":44.629}}
influxdb-mqtt-grafana-dotnet-sender-3  | <6> [ 03/06/2023 21:19:22 ] MqttSender.Worker[0] Message: Sent example/th/08441f09cf0a {"time":1685827162738,"metrics":{"temp":1.632,"humidity":53.869}}
influxdb-mqtt-grafana-dotnet-sender-3  | <6> [ 03/06/2023 21:19:22 ] MqttSender.Worker[0] Message: Sent example/air/08441f09cf0a {"time":1685827162738,"metrics":{"aqi":101.929,"co":19.834,"no2":124.979}}
```

Occasionally you'll see telegraf dump out what it's sending to InfluxDB

```
telegraf | th,device=fa1ee4653730 temp=16.734,humidity=55.847 1685827226512000000
telegraf | air,device=fa1ee4653730 aqi=88.753,co=115.777,no2=136.818 1685827226513000000
telegraf | th,device=c0d1f62b98ea temp=25.227,humidity=57.704 1685827226757000000
telegraf | air,device=c0d1f62b98ea aqi=94.905,co=-2.368,no2=45.506 1685827226757000000
telegraf | th,device=08441f09cf0a temp=0.856,humidity=54.578 1685827226769000000
telegraf | air,device=08441f09cf0a aqi=101.763,co=32.627,no2=147.711 1685827226769000000
```

Then you can jump over to Grafana (listening locally on localhost:3000) and log in with the example credentials (admin:adminadmin). 
There you'll see an example dashboard pre-provisioned on the Grafana instance. That's the screen shot up above!

## MQTT Schema

As an example, we're using a simple MQTT schema:

### Topic

```
example/{measurement}/{device}
```

In InfluxDB terms, the `measurement` describes what we're measuring, as well as the schema we're sending. The example
client sends two measurements: `th` (temperature and humidity), and `air` (air quality).

The `device` describes the machine from which the measurements come. We'll store that in InfluxDB as a tag.
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
