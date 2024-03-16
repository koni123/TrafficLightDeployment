namespace Shared.Services;

public interface IRabbitMqService
{
    public Task SendCommandAsync<T>(
        string queueName,
        T commandMessage,
        string routingKey,
        string expirationTimeMs = "3000");

    public Task<T?> ReceiveFromQueueAsync<T>(
        string queueName,
        string routingKey,
        int tryForMs);
}