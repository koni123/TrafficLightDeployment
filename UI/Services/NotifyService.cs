using Shared.Models;

namespace UI.Services;

public class NotifyService
{
    public async Task Update(List<TrafficLightStatusDto> statuses, OperationModeDto? operationMode)
    {
        if (Notify != null)
        {
            await Notify.Invoke(statuses, operationMode);
        }
    }

    public event Func<List<TrafficLightStatusDto>, OperationModeDto?, Task>? Notify;
}