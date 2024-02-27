using Shared.Models;

namespace UnitTest.Shared.Models;

public class TrafficLightTests
{
    [Fact]
    public void GetNextColor_GreenToYellow_AfterTwentySeconds()
    {
        // arrange
        var trafficLight = new TrafficLight("1")
        {
            Color = TrafficLightColor.Green,
            LastChanged = DateTime.UtcNow.AddSeconds(-20)
        };

        // act
        var nextColor = trafficLight.GetNextColor(DateTime.UtcNow);

        // assert
        Assert.Equal(TrafficLightColor.Yellow, nextColor);
    }

    [Fact]
    public void GetNextColor_YellowToRed_AfterTenSeconds()
    {
        // arrange
        var trafficLight = new TrafficLight("1")
        {
            Color = TrafficLightColor.Yellow,
            LastChanged = DateTime.UtcNow.AddSeconds(-10)
        };

        // act
        var nextColor = trafficLight.GetNextColor(DateTime.UtcNow);

        // assert
        Assert.Equal(TrafficLightColor.Red, nextColor);
    }

    [Fact]
    public void GetNextColor_RedToGreen_Immediately()
    {
        // arrange
        var now = DateTime.UtcNow;
        var trafficLight = new TrafficLight("1")
        {
            Color = TrafficLightColor.Red,
            LastChanged = now
        };

        // act
        var nextColor = trafficLight.GetNextColor(now);

        // assert
        Assert.Equal(TrafficLightColor.Green, nextColor);
    }
}