using System.Diagnostics;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using Shared.Contracts;

namespace Shared.Services;

public class RabbitMqService : IRabbitMqService
{ 
    public async Task SendCommandAsync<T>(string queueName, T commandMessage, string routingKey,
        string expirationTimeMs = "3000")
    {
        using var connection = await GetConnection("TrafficLightOperation");
        using var channel = await GetChannelAndQueueAsync(connection, queueName, routingKey);
        await channel.QueuePurgeAsync(queueName);
        var messageProperties = new BasicProperties
        {
            Expiration = expirationTimeMs
        };

        var asString = JsonSerializer.Serialize(commandMessage);
        var body = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(asString));
        await channel.BasicPublishAsync(TrafficLightConfig.TrafficLightExchangeName, routingKey, messageProperties,
            body);

        await channel.CloseAsync();
        await connection.CloseAsync();
    }

    public async Task<T?> ReceiveFromQueueAsync<T>(string queueName, string routingKey, int tryForMs)
    {
        using var connection = await GetConnection("TrafficLightReceiveSender");
        using var channel = await GetChannelAndQueueAsync(connection, queueName, routingKey);
        await channel.BasicQosAsync(0, 1, false);

        var watch = new Stopwatch();
        watch.Start();

        T? result = default;
        while (watch.Elapsed < TimeSpan.FromMilliseconds(tryForMs) && result is null)
        {
            var message = await channel.BasicGetAsync(queueName, true);
            if (message is null)
            {
                await Task.Delay(200);
                continue;
            }
            
            var body = message.Body.ToArray();
            var str = Encoding.UTF8.GetString(body);
            result = JsonSerializer.Deserialize<T>(str);
        }

        watch.Stop();
        await channel.CloseAsync();
        await connection.CloseAsync();
        return result;
    }
    
    private static async Task<IConnection> GetConnection(string clientName)
    {
        var rabbitHost = Environment.GetEnvironmentVariable("RABBIT_HOSTNAME");
        var factory = new ConnectionFactory
        {
            HostName = string.IsNullOrWhiteSpace(rabbitHost) ? "localhost" : rabbitHost,
            UserName = "guest",
            Password = "guest",
            ClientProvidedName = clientName
        };

        return await factory.CreateConnectionAsync();
    }

    private static async Task<IChannel> GetChannelAndQueueAsync(IConnection connection, string queueName, string routingKey)
    {
        var channel = await connection.CreateChannelAsync();
        await channel.ExchangeDeclareAsync(TrafficLightConfig.TrafficLightExchangeName, ExchangeType.Direct);
        await channel.QueueDeclareAsync(queue: queueName,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);
        await channel.QueueBindAsync(
            queueName,
            TrafficLightConfig.TrafficLightExchangeName,
            routingKey);
        return channel;
    }
}