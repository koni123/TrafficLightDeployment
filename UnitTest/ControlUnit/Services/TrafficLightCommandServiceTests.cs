using ControlUnit.Services;
using NSubstitute;
using Shared.Contracts;
using Shared.Models;
using Shared.Services;

namespace UnitTest.ControlUnit.Services;

public class TrafficLightCommandServiceTests
{
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();

    [Fact]
    public void GetNormalOperationCommands_ReturnsTransitionSet1Green_WhenBothSetsIdle()
    {
        // arrange
        _dateTimeProvider.NowUtc.Returns(DateTime.UtcNow);
        var trafficLightService = new TrafficLightCommandService(_dateTimeProvider);
        
        var lightSet1 = new TrafficLightSet(
            [
                new TrafficLight(TrafficLightConfig.TrafficLight1Id),
                new TrafficLight(TrafficLightConfig.TrafficLight2Id)
            ]
        )
        {
            Status = TrafficLightStatus.Transition
        };
        var lightSet2 = new TrafficLightSet(
            [
                new TrafficLight(TrafficLightConfig.TrafficLight3Id),
                new TrafficLight(TrafficLightConfig.TrafficLight4Id)
            ]
        )
        {
            Status = TrafficLightStatus.Idle
        };

        // act
        var result = trafficLightService.GetNormalOperationCommands(lightSet1, lightSet2);

        // assert
        Assert.Equal(4, result.Count);
        var set1Light1 = result.Find(r => r.TrafficLightId == TrafficLightConfig.TrafficLight1Id);
        var set1Light2 = result.Find(r => r.TrafficLightId == TrafficLightConfig.TrafficLight2Id);
        Assert.NotNull(set1Light1);
        Assert.NotNull(set1Light2);
        Assert.Equal(TrafficLightColor.Green, set1Light1.Color);
        Assert.Equal(TrafficLightColor.Green, set1Light2.Color);
    }
    
    [Fact]
    public void GetNormalOperationCommands_ReturnsTransitionSet1Yellow_WhenGreenOverThresholdSeconds()
    {
        // arrange
        _dateTimeProvider.NowUtc.Returns(DateTime.UtcNow.AddSeconds(30));
        var trafficLightService = new TrafficLightCommandService(_dateTimeProvider);
        
        var lightSet1 = new TrafficLightSet(
            [
                new TrafficLight(TrafficLightConfig.TrafficLight1Id) {  Color = TrafficLightColor.Green, LastChanged = DateTime.UtcNow },
                new TrafficLight(TrafficLightConfig.TrafficLight2Id) { Color = TrafficLightColor.Green, LastChanged = DateTime.UtcNow }
            ]
        )
        {
            Status = TrafficLightStatus.Transition
        };
        var lightSet2 = new TrafficLightSet(
            [
                new TrafficLight(TrafficLightConfig.TrafficLight3Id),
                new TrafficLight(TrafficLightConfig.TrafficLight4Id)
            ]
        )
        {
            Status = TrafficLightStatus.Idle
        };

        // act
        var result = trafficLightService.GetNormalOperationCommands(lightSet1, lightSet2);

        // assert
        var set1Light1 = result.Find(r => r.TrafficLightId == TrafficLightConfig.TrafficLight1Id);
        var set1Light2 = result.Find(r => r.TrafficLightId == TrafficLightConfig.TrafficLight2Id);
        Assert.NotNull(set1Light1);
        Assert.NotNull(set1Light2);
        Assert.Equal(TrafficLightColor.Yellow, set1Light1.Color);
        Assert.Equal(TrafficLightColor.Yellow, set1Light2.Color);
    }
    
    [Fact]
    public void GetNormalOperationCommands_ReturnsTransitionSet1Red_WhenYellowOverThresholdSeconds()
    {
        // arrange
        _dateTimeProvider.NowUtc.Returns(DateTime.UtcNow.AddSeconds(30));
        var trafficLightService = new TrafficLightCommandService(_dateTimeProvider);
        
        var lightSet1 = new TrafficLightSet(
            [
                new TrafficLight(TrafficLightConfig.TrafficLight1Id) {  Color = TrafficLightColor.Yellow, LastChanged = DateTime.UtcNow },
                new TrafficLight(TrafficLightConfig.TrafficLight2Id) { Color = TrafficLightColor.Yellow, LastChanged = DateTime.UtcNow }
            ]
        )
        {
            Status = TrafficLightStatus.Transition
        };
        var lightSet2 = new TrafficLightSet(
            [
                new TrafficLight(TrafficLightConfig.TrafficLight3Id),
                new TrafficLight(TrafficLightConfig.TrafficLight4Id)
            ]
        )
        {
            Status = TrafficLightStatus.Idle
        };

        // act
        var result = trafficLightService.GetNormalOperationCommands(lightSet1, lightSet2);

        // assert
        var set1Light1 = result.Find(r => r.TrafficLightId == TrafficLightConfig.TrafficLight1Id);
        var set1Light2 = result.Find(r => r.TrafficLightId == TrafficLightConfig.TrafficLight2Id);
        Assert.NotNull(set1Light1);
        Assert.NotNull(set1Light2);
        Assert.Equal(TrafficLightColor.Red, set1Light1.Color);
        Assert.Equal(TrafficLightColor.Red, set1Light2.Color);
    }
}