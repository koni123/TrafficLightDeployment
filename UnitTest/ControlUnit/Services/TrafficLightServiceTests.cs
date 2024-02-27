using ControlUnit.Services;
using NSubstitute;
using Shared.Contracts;
using Shared.Models;
using Shared.Services;

namespace UnitTest.ControlUnit.Services;

public class TrafficLightServiceTests
{
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    private readonly ITrafficLightService _sut;

    public TrafficLightServiceTests()
    {
        _sut = new TrafficLightService(_dateTimeProvider);
    }

    [Fact]
    public void GetNormalOperationCommands_ReturnsTransitionSet1Green_WhenBothSetsIdle()
    {
        // arrange
        _dateTimeProvider.NowUtc.Returns(DateTime.UtcNow);

        var lightSet1 = new TrafficLightSet(
            [
                new TrafficLight(TrafficLightConfig.Set1Light1Id),
                new TrafficLight(TrafficLightConfig.Set1Light2Id)
            ]
        )
        {
            Status = TrafficLightStatus.Transition
        };
        var lightSet2 = new TrafficLightSet(
            [
                new TrafficLight(TrafficLightConfig.Set2Light1Id),
                new TrafficLight(TrafficLightConfig.Set2Light2Id)
            ]
        )
        {
            Status = TrafficLightStatus.Stop
        };

        // act
        var result = _sut.GetNormalOperationCommands(lightSet1, lightSet2);

        // assert
        Assert.Equal(4, result.Count);
        var set1Light1 = result.Find(r => r.TrafficLightId == TrafficLightConfig.Set1Light1Id);
        var set1Light2 = result.Find(r => r.TrafficLightId == TrafficLightConfig.Set1Light2Id);
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

        var lightSet1 = new TrafficLightSet(
            [
                new TrafficLight(TrafficLightConfig.Set1Light1Id)
                    { Color = TrafficLightColor.Green, LastChanged = DateTime.UtcNow },
                new TrafficLight(TrafficLightConfig.Set1Light2Id)
                    { Color = TrafficLightColor.Green, LastChanged = DateTime.UtcNow }
            ]
        )
        {
            Status = TrafficLightStatus.Transition
        };
        var lightSet2 = new TrafficLightSet(
            [
                new TrafficLight(TrafficLightConfig.Set2Light1Id),
                new TrafficLight(TrafficLightConfig.Set2Light2Id)
            ]
        )
        {
            Status = TrafficLightStatus.Stop
        };

        // act
        var result = _sut.GetNormalOperationCommands(lightSet1, lightSet2);

        // assert
        var set1Light1 = result.Find(r => r.TrafficLightId == TrafficLightConfig.Set1Light1Id);
        var set1Light2 = result.Find(r => r.TrafficLightId == TrafficLightConfig.Set1Light2Id);
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
        var lightSet1 = new TrafficLightSet(
            [
                new TrafficLight(TrafficLightConfig.Set1Light1Id)
                    { Color = TrafficLightColor.Yellow, LastChanged = DateTime.UtcNow },
                new TrafficLight(TrafficLightConfig.Set1Light2Id)
                    { Color = TrafficLightColor.Yellow, LastChanged = DateTime.UtcNow }
            ]
        )
        {
            Status = TrafficLightStatus.Transition
        };
        var lightSet2 = new TrafficLightSet(
            [
                new TrafficLight(TrafficLightConfig.Set2Light1Id),
                new TrafficLight(TrafficLightConfig.Set2Light2Id)
            ]
        )
        {
            Status = TrafficLightStatus.Stop
        };

        // act
        var result = _sut.GetNormalOperationCommands(lightSet1, lightSet2);

        // assert
        var set1Light1 = result.Find(r => r.TrafficLightId == TrafficLightConfig.Set1Light1Id);
        var set1Light2 = result.Find(r => r.TrafficLightId == TrafficLightConfig.Set1Light2Id);
        Assert.NotNull(set1Light1);
        Assert.NotNull(set1Light2);
        Assert.Equal(TrafficLightColor.Red, set1Light1.Color);
        Assert.Equal(TrafficLightColor.Red, set1Light2.Color);
    }

    [Fact]
    public void GetStatus_BothStop_ReturnsTransitionForSet1()
    {
        // arrange
        var trafficLightSet1 = new TrafficLightSet([new TrafficLight("someId")]);
        var trafficLightSet2 = new TrafficLightSet([new TrafficLight("someId")]);

        // act
        var (status1, status2) = _sut.GetSetStatuses(trafficLightSet1, trafficLightSet2);

        // assert
        Assert.Equal(TrafficLightStatus.Transition, status1);
        Assert.Equal(TrafficLightStatus.Stop, status2);
    }
}