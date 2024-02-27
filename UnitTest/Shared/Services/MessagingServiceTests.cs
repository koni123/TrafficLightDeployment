using Microsoft.Extensions.Logging;
using NSubstitute;
using Shared.Contracts;
using Shared.Services;

namespace UnitTest.Shared.Services;

public class MessagingServiceTests
{
    private readonly ILogger<MessagingService> _logger = Substitute.For<ILogger<MessagingService>>();
    private readonly IRabbitMqService _rabbitMqService = Substitute.For<IRabbitMqService>();

    [Fact]
    public async Task SendAndReceiveAsync_ReturnsResponse_WhenSuccess()
    {
        // arrange
        var sut = new MessagingService(_logger, _rabbitMqService);
        var id = Guid.NewGuid().ToString();
        var message = new TrafficLightCommandMessage
        {
            CorrelationId = id
        };
        _rabbitMqService.SendCommandAsync(Arg.Any<string>(), Arg.Any<TrafficLightCommandMessage>(), Arg.Any<string>(),
                Arg.Any<string>())
            .Returns(Task.CompletedTask);
        _rabbitMqService
            .ReceiveFromQueueAsync<TrafficLightCommandMessage>(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>())
            .Returns(message);

        // act
        var response = await sut.SendAndReceiveAsync(message, "someQueue", "someQueue");

        // assert
        Assert.NotNull(response);
        Assert.Equal(id, response.CorrelationId);
    }

    [Fact]
    public async Task ReceiveAndReplyAsync_HandlesMessage_WhenFetchFromQueue()
    {
        // arrange
        var sut = new MessagingService(_logger, _rabbitMqService);
        var id = Guid.NewGuid().ToString();
        Func<TrafficLightCommandMessage, TrafficLightCommandMessage> testHandler = message =>
        {
            message.CorrelationId = id;
            return message;
        };
        var message = new TrafficLightCommandMessage
        {
            CorrelationId = "some_totally_different_id"
        };
        _rabbitMqService.SendCommandAsync(Arg.Any<string>(), Arg.Any<TrafficLightCommandMessage>(), Arg.Any<string>(),
                Arg.Any<string>())
            .Returns(Task.CompletedTask);
        _rabbitMqService
            .ReceiveFromQueueAsync<TrafficLightCommandMessage>(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>())
            .Returns(message);

        // act
        await sut.ReceiveAndReplyAsync("someQueue", "someQueue", testHandler);

        // assert
        await _rabbitMqService.Received(1)
            .SendCommandAsync(
                Arg.Any<string>(),
                Arg.Is<TrafficLightCommandMessage>(m => m.CorrelationId == id),
                Arg.Any<string>(),
                Arg.Any<string>());
    }
}