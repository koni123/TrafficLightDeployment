using Shared.Contracts;

namespace Shared.Services;

public interface IMessagingService
{
    public Task<T?> SendAndReceiveAsync<T>(T commandMessage, string queueTo, string queueFrom, int responseMaxWaitTimeMs = 2000) where T : ITrackedMessage, new();
    public Task ReceiveAndReplyAsync<T>(string queueFrom, string queueTo, Func<T, T> eventHandler) where T : ITrackedMessage;
    public Task<T?> ReceiveAsync<T>(string queueFrom, string routingKey) where T : ITrackedMessage;
    public Task SendAsync<T>(string queueTo, string routingKey, T message) where T : ITrackedMessage;
}