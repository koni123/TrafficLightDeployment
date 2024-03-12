using ControlUnit.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shared.Models;
using Shared.Services;

namespace UnitTest.ControlUnit.Services;

public class ControlUnitTests
{
    private readonly IMessagingService _messagingService = Substitute.For<IMessagingService>();
    private readonly ITrafficLightService _trafficLightService = Substitute.For<ITrafficLightService>();
    private readonly IDatabaseService _databaseService = Substitute.For<IDatabaseService>();

    private readonly ILogger<global::ControlUnit.Services.ControlUnit> _logger =
        Substitute.For<ILogger<global::ControlUnit.Services.ControlUnit>>();

    private readonly global::ControlUnit.Services.ControlUnit _sut;

    public ControlUnitTests()
    {
        _sut = new global::ControlUnit.Services.ControlUnit(_messagingService, _trafficLightService, _logger, _databaseService);
    }

    [Fact]
    public async Task RunNormalOperation_Throws_WhenAllSetsInTransition()
    {
        // arrange
        var status1 = TrafficLightStatus.Transition;
        var status2 = TrafficLightStatus.Transition;
        _trafficLightService.GetSetStatuses(Arg.Any<TrafficLightSet>(), Arg.Any<TrafficLightSet>())
            .Returns((status1, status2));

        // act
        var result = await Record.ExceptionAsync(() => _sut.RunNormalOperation());

        // assert
        Assert.NotNull(result);
        Assert.IsType<ApplicationException>(result);
    }
}