using DatabaseService.Database;
using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace DatabaseService.Services;

public class DbService : IDbService
{
    private readonly TrafficLightContext _context;

    public DbService(TrafficLightContext context)
    {
        _context = context;
    }

    public async Task AddOperationModeToDatabaseAsync(OperationModeDto operationMode)
    {
        var model = new OperationModeModel
        {
            OperationMode = operationMode.OperationMode,
            CreatedAt = operationMode.CreatedAt
        };

        _context.Add(model);
        await _context.SaveChangesAsync();
    }

    public async Task<OperationModeModel?> GetCurrentModeAsync()
    {
        return await _context.OperationModeModels
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task AddStatusModelToDatabaseAsync(TrafficLightStatusDto statusModel)
    {
        await _context.TrafficLight
            .Where(tl => tl.ModifiedAt < DateTime.UtcNow.AddMinutes(-10))
            .ExecuteDeleteAsync();

        var model = new TrafficLightStatusModel
        {
            TrafficLightId = statusModel.TrafficLightId,
            Color = statusModel.Color,
            LastChanged = statusModel.LastChanged,
            ModifiedAt = DateTime.UtcNow
        };
        _context.Add(model);
        await _context.SaveChangesAsync();
    }

    public async Task<List<TrafficLightStatusModel>> GetCurrentStatusAsync()
    {
        var ids = await _context.TrafficLight
            .AsNoTracking()
            .Select(tl => tl.TrafficLightId)
            .Distinct()
            .ToListAsync();

        var statuses = new List<TrafficLightStatusModel>();

        foreach (var id in ids)
        {
            var status = await _context.TrafficLight
                .Where(tl => tl.TrafficLightId == id)
                .OrderByDescending(tl => tl.ModifiedAt)
                .FirstOrDefaultAsync();
            if (status is null) continue;
            statuses.Add(status);
        }

        return statuses;
    }

    public async Task<List<TrafficLightStatusModel>> GetAllAsync()
    {
        return await _context.TrafficLight
            .ToListAsync();
    }
}