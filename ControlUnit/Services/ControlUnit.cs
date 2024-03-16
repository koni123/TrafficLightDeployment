using Microsoft.Extensions.Logging;
using Shared.Contracts;
using Shared.Models;
using Shared.Services;

namespace ControlUnit.Services;

public class ControlUnit : IControlUnit
{
    private TrafficLightSet _trafficLightSet1 = new([]);
    private TrafficLightSet _trafficLightSet2 = new([]);
    private string _currentOperationMode = TrafficLightOperationMode.Stop;

    private readonly IMessagingService _messagingService;
    private readonly IDatabaseService _databaseService;
    private readonly ITrafficLightService _trafficLightService;
    private readonly ILogger<ControlUnit> _logger;

    public ControlUnit(
        IMessagingService messagingService,
        ITrafficLightService trafficLightService,
        ILogger<ControlUnit> logger,
        IDatabaseService databaseService)
    {
        InitializeTrafficLightSets();
        _messagingService = messagingService;
        _trafficLightService = trafficLightService;
        _logger = logger;
        _databaseService = databaseService;
    }

    public async Task RunOperation(string? operationMode)
    {
        if (!string.IsNullOrWhiteSpace(operationMode))
        {
            _currentOperationMode = operationMode;
        }

        await _databaseService.AddControlUnitOperationModeAsync(
            new OperationModeDto(_currentOperationMode, DateTime.UtcNow));
        
        switch (_currentOperationMode)
        {
            case TrafficLightOperationMode.Normal:
                await RunNormalOperation();
                break;
            case TrafficLightOperationMode.Stop:
                await RunStopOperation();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private async Task RunNormalOperation()
    {
        var correlationId = Guid.NewGuid().ToString();
        (_trafficLightSet1.Status, _trafficLightSet2.Status) =
            _trafficLightService.GetSetNormalStatuses(_trafficLightSet1, _trafficLightSet2);

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

    private async Task RunStopOperation()
    {
        var correlationId = Guid.NewGuid().ToString();
        (_trafficLightSet1.Status, _trafficLightSet2.Status) =
            _trafficLightService.GetSetStopStatuses(_trafficLightSet1, _trafficLightSet2);

        if (_trafficLightSet1.Status == TrafficLightStatus.Transition &&
            _trafficLightSet2.Status == TrafficLightStatus.Transition)
        {
            InitializeTrafficLightSets();
            throw new ApplicationException(
                "Fatal error in application - all lights in transition! Resetting rotation.");
        }

        var commands = _trafficLightService.GetStopOperationCommands(_trafficLightSet1, _trafficLightSet2);
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

        await SendStatusToDatabase();
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

    private async Task SendStatusToDatabase()
    {
        foreach (var model in _trafficLightSet1.TrafficLights.Select(trafficLight => new TrafficLightStatusDto
                 {
                     TrafficLightId = trafficLight.TrafficLightId,
                     Color = trafficLight.Color.ToString(),
                     LastChanged = trafficLight.LastChanged
                 }))
        {
            await _databaseService.AddTrafficLightStatusAsync(model);
        }

        foreach (var model in _trafficLightSet2.TrafficLights.Select(trafficLight => new TrafficLightStatusDto
                 {
                     TrafficLightId = trafficLight.TrafficLightId,
                     Color = trafficLight.Color.ToString(),
                     LastChanged = trafficLight.LastChanged
                 }))
        {
            await _databaseService.AddTrafficLightStatusAsync(model);
        }
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