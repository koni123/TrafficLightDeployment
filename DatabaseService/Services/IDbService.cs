using DatabaseService.Database;
using Shared.Models;

namespace DatabaseService.Services;

public interface IDbService
{
    Task AddOperationModeToDatabaseAsync(OperationModeDto operationMode);
    Task<OperationModeModel?> GetCurrentModeAsync();
    Task AddStatusModelToDatabaseAsync(TrafficLightStatusDto statusModel);
    Task<List<TrafficLightStatusModel>> GetCurrentStatusAsync();
    Task<List<TrafficLightStatusModel>> GetAllAsync();
}