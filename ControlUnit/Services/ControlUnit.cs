﻿using Microsoft.Extensions.Logging;
using Shared.Contracts;
using Shared.Models;
using Shared.Services;

namespace ControlUnit.Services;

public class ControlUnit : IControlUnit
{
    private TrafficLightSet _trafficLightSet1 = new([]);
    private TrafficLightSet _trafficLightSet2 = new([]);

    private readonly IMessagingService _messagingService;
    private readonly ITrafficLightService _trafficLightService;
    private readonly ILogger<ControlUnit> _logger;

    public ControlUnit(
        IMessagingService messagingService,
        ITrafficLightService trafficLightService,
        ILogger<ControlUnit> logger)
    {
        InitializeTrafficLightSets();
        _messagingService = messagingService;
        _trafficLightService = trafficLightService;
        _logger = logger;
    }

    public async Task RunNormalOperation()
    {
        var correlationId = Guid.NewGuid().ToString();
        (_trafficLightSet1.Status, _trafficLightSet2.Status) =
            _trafficLightService.GetSetStatuses(_trafficLightSet1, _trafficLightSet2);

        if (_trafficLightSet1.Status == TrafficLightStatus.Transition &&
            _trafficLightSet2.Status == TrafficLightStatus.Transition)
        {
            InitializeTrafficLightSets();
            throw new ApplicationException(
                "Fatal error in application - all lights in transition! Resetting rotation.");
        }

        var commands = _trafficLightService.GetNormalOperationCommands(_trafficLightSet1, _trafficLightSet2);
        if (commands.Count != _trafficLightSet1.TrafficLights.Count + _trafficLightSet2.TrafficLights.Count)
        {
            InitializeTrafficLightSets();
            throw new ApplicationException("Wrong amount of commands received from logic! Resetting rotation.");
        }

        await HandleLightCommands(commands, correlationId);
    }

    private async Task HandleLightCommands(List<TrafficLightCommand> commands, string correlationId)
    {
        var response = await _messagingService.SendAndReceiveAsync(
            new TrafficLightCommandMessage { CorrelationId = correlationId, Commands = commands },
            TrafficLightConfig.ControlUnitToTrafficLightQueueName,
            TrafficLightConfig.TrafficLightToControlUnitQueueName);

        var matching = commands.Intersect(response?.Commands ?? []).ToList();
        if (matching.Count != commands.Count || response is null)
        {
            throw new ApplicationException("Error in messaging! Trying again..");
        }

        response.Commands.ForEach(ChangeColorForLight);

        var light1Status = string.Join(",", _trafficLightSet1.TrafficLights.Select(c => c.Color.ToString()));
        var light2Status = string.Join(",", _trafficLightSet2.TrafficLights.Select(c => c.Color.ToString()));
        _logger.LogInformation(
            "Light set 1 '{set1status}': ({light1Status}) set 2 '{set1status}': ({light2Status})",
            _trafficLightSet1.Status.ToString(),
            light1Status,
            _trafficLightSet2.Status.ToString(),
            light2Status);
    }

    private void ChangeColorForLight(TrafficLightCommand command)
    {
        var existingFromSet1 = _trafficLightSet1.TrafficLights.Find(tl => tl.TrafficLightId == command.TrafficLightId);
        if (existingFromSet1 is not null)
        {
            if (existingFromSet1.Color == command.Color) return;
            existingFromSet1.Color = command.Color;
            existingFromSet1.LastChanged = DateTime.UtcNow;
            _trafficLightSet1.SetStatus();
            return;
        }

        var existingFromSet2 = _trafficLightSet2.TrafficLights.Find(tl => tl.TrafficLightId == command.TrafficLightId);
        if (existingFromSet2 is null) return;
        if (existingFromSet2.Color == command.Color) return;

        existingFromSet2.Color = command.Color;
        existingFromSet2.LastChanged = DateTime.UtcNow;
        _trafficLightSet2.SetStatus();
    }

    private void InitializeTrafficLightSets()
    {
        _trafficLightSet1 = new TrafficLightSet(
            [
                new TrafficLight(TrafficLightConfig.Set1Light1Id),
                new TrafficLight(TrafficLightConfig.Set1Light2Id)
            ]
        );
        _trafficLightSet2 = new TrafficLightSet(
            [
                new TrafficLight(TrafficLightConfig.Set2Light1Id),
                new TrafficLight(TrafficLightConfig.Set2Light2Id)
            ]
        );
    }
}