using Shared.Services;

namespace UI.Services;

public class StatusSyncService(NotifyService notifyService, IDatabaseService databaseService) : IDisposable
{
    private static readonly TimeSpan HeartbeatTickRate = TimeSpan.FromSeconds(1);
    private PeriodicTimer? _timer;

    public async Task Start()
    {
        if (_timer is null)
        {
            _timer = new PeriodicTimer(HeartbeatTickRate);

            using (_timer)
            {
                while (await _timer.WaitForNextTickAsync())
                {
                    var operationMode = await databaseService.GetControlUnitOperationModeAsync();
                    var statuses = await databaseService.GetCurrentStatusAsync();
                    var ordered = statuses
                        .OrderBy(tl => tl.TrafficLightId)
                        .ToList();

                    await notifyService.Update(ordered, operationMode);
                }
            }
        }
    }

    public void Dispose()
    {
        _timer?.Dispose();
        GC.SuppressFinalize(this);
    }
}