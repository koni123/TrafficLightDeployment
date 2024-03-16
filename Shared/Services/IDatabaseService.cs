using Shared.Models;

namespace Shared.Services;

public interface IDatabaseService
{
    Task AddTrafficLightStatusAsync(TrafficLightStatusDto statusDto);
    Task<List<TrafficLightStatusDto>> GetCurrentStatusAsync();
    Task AddControlUnitOperationModeAsync(OperationModeDto operationMode);
    Task<OperationModeDto?> GetControlUnitOperationModeAsync();
    Task<List<TrafficLightStatusDto>> GetAllAsync();
}