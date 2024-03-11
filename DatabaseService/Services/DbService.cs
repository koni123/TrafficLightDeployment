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

    public async Task AddModelToDatabaseAsync(TrafficLightStatusModel statusModel)
    {
        _context.Add(statusModel);
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
                .OrderByDescending(tl => tl.LastChanged)
                .FirstOrDefaultAsync();
            if (status is null) continue;
            statuses.Add(status);
        }

        return statuses;
    }
}