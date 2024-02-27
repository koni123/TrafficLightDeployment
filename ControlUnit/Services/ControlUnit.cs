using Microsoft.Extensions.Logging;
using Shared.Contracts;
using Shared.Models;
using Shared.Services;

namespace ControlUnit.Services;

public class ControlUnit : IControlUnit
{
    private readonly TrafficLightSet _trafficLightSet1;
    private readonly TrafficLightSet _trafficLightSet2;

    private readonly IMessagingService _messagingService;
    private readonly ITrafficLightCommandService _trafficLightCommandService;
    private readonly ILogger<ControlUnit> _logger;

    public ControlUnit(IMessagingService messagingService, ITrafficLightCommandService trafficLightCommandService, ILogger<ControlUnit> logger)
    {
        _trafficLightSet1 = new TrafficLightSet(
            [
                new TrafficLight(TrafficLightConfig.TrafficLight1Id),
                new TrafficLight(TrafficLightConfig.TrafficLight2Id)
            ]
        );
        _trafficLightSet2 = new TrafficLightSet(
            [
                new TrafficLight(TrafficLightConfig.TrafficLight3Id),
                new TrafficLight(TrafficLightConfig.TrafficLight4Id)
            ]
        );
        _messagingService = messagingService;
        _trafficLightCommandService = trafficLightCommandService;
        _logger = logger;
    }

    public async Task RunNormalOperation()
    {
        var correlationId = Guid.NewGuid().ToString();
        (_trafficLightSet1.Status, _trafficLightSet2.Status) =
            TrafficLightStatusUtil.GetStatus(_trafficLightSet1, _trafficLightSet2);

        if (_trafficLightSet1.Status == TrafficLightStatus.Transition &&
            _trafficLightSet2.Status == TrafficLightStatus.Transition)
        {
            throw new ApplicationException("Fatal error in application! Shutting down..");
        }

        // no need to send messages if finished and/or idle
        if (_trafficLightSet1.Status != TrafficLightStatus.Transition &&
            _trafficLightSet2.Status != TrafficLightStatus.Transition)
        {
            return;
        }

        var commands = _trafficLightCommandService.GetNormalOperationCommands(_trafficLightSet1, _trafficLightSet2);
        if (commands.Count == _trafficLightSet1.TrafficLights.Count + _trafficLightSet2.TrafficLights.Count)
        {
            var response = await _messagingService.SendAndReceiveAsync(
                new TrafficLightCommandMessage { CorrelationId = correlationId, Commands = commands },
                TrafficLightConfig.ControlUnitToTrafficLightQueueName,
                TrafficLightConfig.TrafficLightToControlUnitQueueName);

            var matching = commands.Intersect(response.Commands).ToList();
            if (matching.Count != commands.Count)
            {
                throw new ApplicationException("Fatal error in application! Shutting down..");
            }

            response.Commands.ForEach(c =>
            {
                var existing = FindWithId(c.TrafficLightId);
                if (existing.Color != c.Color) existing.LastChanged = DateTime.UtcNow;
                existing.Color = c.Color;
            });

            var light1Status = string.Join(",", _trafficLightSet1.TrafficLights.Select(c => c.Color.ToString()));
            var light2Status = string.Join(",", _trafficLightSet2.TrafficLights.Select(c => c.Color.ToString()));
            _logger.LogInformation($"Light status: 1:({light1Status}) 2:({light2Status})");
            return;
        }
        
        Console.WriteLine("Wrong amount of commands received from logic!");
    }

    private TrafficLight FindWithId(string trafficLightId)
    {
        var existing = _trafficLightSet1.TrafficLights.Find(tl => tl.TrafficLightId == trafficLightId);
        if (existing is not null) return existing;
        return _trafficLightSet2.TrafficLights.Find(tl => tl.TrafficLightId == trafficLightId)!;
    }
}