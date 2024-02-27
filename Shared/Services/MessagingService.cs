using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Contracts;

namespace Shared.Services;

public class MessagingService(ILogger<MessagingService> logger) : IMessagingService
{
    public async Task<T> SendAndReceiveAsync<T>(T commandMessage, string queueTo, string queueFrom,
        int responseMaxWaitTimeMs = 2000) where T : ITrackedMessage, new()
    {
        await SendCommandAsync(
            queueTo,
            commandMessage,
            TrafficLightConfig.ControlUnitToTrafficLightRoutingKey);
        logger.LogDebug("Traffic light command sent!");

        using var connection = await GetConnection("TrafficLightSendReceiver");
        using var channel = await GetChannelAndQueueAsync(connection, queueFrom, TrafficLightConfig.TrafficLightToControlUnitRoutingKey);

        var consumer = new EventingBasicConsumer(channel);
        var tcs = new TaskCompletionSource<T>();
        var deliveryTag = ulong.MinValue;

        consumer.Received += (_, args) =>
        {
            var body = args.Body.ToArray();
            var message = JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(body));
            deliveryTag = args.DeliveryTag;
            tcs.SetResult(message!);
        };

        await channel.BasicConsumeAsync(queueFrom, false, consumer);

        // Wait for the response with a timeout
        var timeoutTask = Task.Delay(responseMaxWaitTimeMs);
        logger.LogDebug("Start to listen for ack!");

        var completedTask = await Task.WhenAny(tcs.Task, timeoutTask);
        logger.LogDebug("Listening done!");
        var result = completedTask == timeoutTask
            ? new T()
            : await tcs.Task;

        if (deliveryTag > ulong.MinValue)
        {
            await channel.BasicAckAsync(deliveryTag, false);
        }

        await channel.CloseAsync();
        await connection.CloseAsync();
        return result;
    }

    public async Task ReceiveAndReplyAsync<T>(
        string queueFrom,
        string queueTo,
        Func<T, T> eventHandler) where T : ITrackedMessage
    {
        using var connection = await GetConnection("TrafficLightReceiveSender");
        using var channel = await GetChannelAndQueueAsync(connection, queueFrom, TrafficLightConfig.ControlUnitToTrafficLightRoutingKey);
        await channel.BasicQosAsync(0, 1, false);

        var consumer = new EventingBasicConsumer(channel);
        var tcs = new TaskCompletionSource<T>();
        consumer.Received += (_, args) =>
        {
            var body = args.Body.ToArray();
            var message = JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(body));
            tcs.SetResult(message!);
        };
        logger.LogDebug("Start to fetch messages from queue!");
        var message = await channel.BasicGetAsync(queueFrom, true);
        if (message is not null)
        {
            var body = message.Body.ToArray();
            var str = Encoding.UTF8.GetString(body);
            var asObject = JsonSerializer.Deserialize<T>(str);
            var result = eventHandler(asObject!);
            await SendCommandAsync(queueTo, result, TrafficLightConfig.TrafficLightToControlUnitRoutingKey);
        }

        await channel.CloseAsync();
        await connection.CloseAsync();
    }

    private static async Task SendCommandAsync<T>(
        string queueName,
        T commandMessage,
        string routingKey,
        string expirationTimeMs = "2000")
        where T : ITrackedMessage
    {
        using var connection = await GetConnection("TrafficLightSender");
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

    private static async Task<IChannel> GetChannelAndQueueAsync(IConnection connection, string queueName,
        string routingKey)
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