using DatabaseService.Database;
using Shared.Models;

namespace DatabaseService;

public static class TrafficLightStatusModelExtensions
{
    public static TrafficLightStatusDto ToDto(this TrafficLightStatusModel model)
    {
        return new TrafficLightStatusDto
        {
            TrafficLightId = model.TrafficLightId,
            Color = model.Color,
            LastChanged = model.LastChanged
        };
    }
}