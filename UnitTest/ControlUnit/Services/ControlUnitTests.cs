using System.Xml.Serialization;
using ControlUnit.Services;
using NSubstitute;
using Shared.Services;

namespace UnitTest.ControlUnit.Services;

public class ControlUnitTests
{
    private readonly IMessagingService _messagingService = Substitute.For<IMessagingService>();
    private readonly ITrafficLightCommandService _trafficLightCommandService = Substitute.For<ITrafficLightCommandService>();

    [Fact]
    public async Task RunNormalOperation_Throws_WhenAllSetsInTransition()
    {
        
    }
}