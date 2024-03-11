using Shared.Models;

namespace DatabaseService.Services;

public interface IDbService
{
    Task AddModelToDatabaseAsync(TrafficLightStatusModel statusModel);
    Task<List<TrafficLightStatusModel>> GetCurrentStatusAsync();
}