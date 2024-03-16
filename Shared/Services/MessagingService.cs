using Microsoft.Extensions.Logging;
using Shared.Contracts;

namespace Shared.Services;

public class MessagingService(ILogger<MessagingService> logger, IRabbitMqService rabbitMqService) : IMessagingService
{
    public async Task<T?> SendAndReceiveAsync<T>(T commandMessage, string queueTo, string queueFrom,
        int responseMaxWaitTimeMs = 3000) where T : ITrackedMessage, new()
    {
        await rabbitMqService.SendCommandAsync(
            queueTo,
            commandMessage,
            TrafficLightConfig.ControlUnitToTrafficLightRoutingKey);
        logger.LogDebug("Traffic light command sent!");

        var result = await rabbitMqService.ReceiveFromQueueAsync<T>(
            queueFrom,
            TrafficLightConfig.TrafficLightToControlUnitRoutingKey,
            responseMaxWaitTimeMs);
        
        logger.LogDebug("Waited for message!");
        if (result is null)
        {
            logger.LogDebug("No response for command after {waitTimeMs} ms!", responseMaxWaitTimeMs);
        }

        return result;
    }

    public async Task ReceiveAndReplyAsync<T>(
        string queueFrom,
        string queueTo,
        Func<T, T> eventHandler) where T : ITrackedMessage
    {
        logger.LogDebug("Start to fetch messages from queue!");
        var result = await rabbitMqService.ReceiveFromQueueAsync<T>(
            queueFrom,
            TrafficLightConfig.ControlUnitToTrafficLightRoutingKey,
            500);

        if (result == null) return;
        var handledResult = eventHandler(result);
        await rabbitMqService.SendCommandAsync(queueTo, handledResult,
            TrafficLightConfig.TrafficLightToControlUnitRoutingKey);
        logger.LogDebug("Command received and handled!");
    }
    
    public async Task<T?> ReceiveAsync<T>(string queueFrom, string routingKey) where T : ITrackedMessage
    {
        logger.LogDebug("Start to fetch messages from queue!");
        return await rabbitMqService.ReceiveFromQueueAsync<T>(
            queueFrom,
            routingKey,
            500);
    }
    
    public async Task SendAsync<T>(string queueTo, string routingKey, T message) where T : ITrackedMessage
    {
        await rabbitMqService.SendCommandAsync(queueTo, message, routingKey, "10000");
    }
}