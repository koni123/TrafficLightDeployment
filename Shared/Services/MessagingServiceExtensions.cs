using Microsoft.Extensions.DependencyInjection;

namespace Shared.Services;

public static class MessagingServiceExtensions
{
    public static void AddMessagingService(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IMessagingService, MessagingService>();
        serviceCollection.AddSingleton<IRabbitMqService, RabbitMqService>();
    }
}