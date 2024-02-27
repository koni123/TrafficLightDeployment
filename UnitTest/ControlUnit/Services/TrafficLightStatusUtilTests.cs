using ControlUnit.Services;
using Shared.Models;

namespace UnitTest.ControlUnit.Services;

public class TrafficLightStatusUtilTests
{
    [Fact]
    public void GetStatus_BothIdle_ReturnsTransitionForSet1()
    {
        // arrange
        var trafficLightSet1 = new TrafficLightSet([]);
        var trafficLightSet2 = new TrafficLightSet([]);

        // act
        var (status1, status2) = TrafficLightStatusUtil.GetStatus(trafficLightSet1, trafficLightSet2);

        // assert
        Assert.Equal(TrafficLightStatus.Transition, status1);
        Assert.Equal(TrafficLightStatus.Idle, status2);
    }

    [Fact]
    public void GetStatus_Set1TransitionReturnsFinishedForSet1_WhenAllRedInSet1()
    {
        // arrange
        var trafficLights1 = new List<TrafficLight>
        {
            new("1") { Color = TrafficLightColor.Red }
        };
        var trafficLights2 = new List<TrafficLight>
        {
            new("2") { Color = TrafficLightColor.Green }
        };
        var trafficLightSet1 = new TrafficLightSet(trafficLights1) { Status = TrafficLightStatus.Transition };
        var trafficLightSet2 = new TrafficLightSet(trafficLights2);

        // act
        var (status1, status2) = TrafficLightStatusUtil.GetStatus(trafficLightSet1, trafficLightSet2);

        // assert
        Assert.Equal(TrafficLightStatus.Finished, status1);
        Assert.Equal(TrafficLightStatus.Idle, status2);
    }
    
    [Fact]
    public void GetStatus_ThrowsException_WhenBothInTransition()
    {
        // arrange
        var trafficLights1 = new List<TrafficLight>
        {
            new("1") { Color = TrafficLightColor.Red }
        };
        var trafficLights2 = new List<TrafficLight>
        {
            new("2") { Color = TrafficLightColor.Green }
        };
        var trafficLightSet1 = new TrafficLightSet(trafficLights1) { Status = TrafficLightStatus.Transition };
        var trafficLightSet2 = new TrafficLightSet(trafficLights2) { Status = TrafficLightStatus.Transition };

        // act
        var result = Record.Exception(() => TrafficLightStatusUtil.GetStatus(trafficLightSet1, trafficLightSet2));

        // assert
        Assert.NotNull(result);
        Assert.IsType<ApplicationException>(result);
    }

}