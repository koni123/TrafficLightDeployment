using DatabaseService.Database;
using Shared.Models;

namespace DatabaseService.Services;

public interface IDbService
{
    Task AddModelToDatabaseAsync(TrafficLightStatusDto statusModel);
    Task<List<TrafficLightStatusModel>> GetCurrentStatusAsync();
    Task<List<TrafficLightStatusModel>> GetAllAsync();
}