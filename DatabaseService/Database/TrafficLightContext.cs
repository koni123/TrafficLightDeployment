using Microsoft.EntityFrameworkCore;

namespace DatabaseService.Database;

public class TrafficLightContext: DbContext
{
    public DbSet<TrafficLightStatusModel> TrafficLight { get; set; } = null!;

    public TrafficLightContext(DbContextOptions options) : base(options)
    {
    }
}
