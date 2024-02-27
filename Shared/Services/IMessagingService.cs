using RabbitMQ.Client.Events;
using Shared.Contracts;

namespace Shared.Services;

public interface IMessagingService
{
    // public Task SendCommandAsync<T>(T commandMessage) where T : ITrackedMessage;
    // public Task ReceiveCommandAsync<T>(EventHandler<BasicDeliverEventArgs> messageHandler) where T : ITrackedMessage;
    public Task<T> SendAndReceiveAsync<T>(T commandMessage, string queueTo, string queueFrom, int responseMaxWaitTimeMs = 2000) where T : ITrackedMessage, new();
    public Task ReceiveAndReplyAsync<T>(string queueFrom, string queueTo, Func<T, T> eventHandler) where T : ITrackedMessage;
}