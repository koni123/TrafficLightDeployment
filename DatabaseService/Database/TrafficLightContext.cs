using Microsoft.EntityFrameworkCore;

namespace DatabaseService.Database;

public class TrafficLightContext: DbContext
{
    public DbSet<TrafficLightStatusModel> TrafficLight { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql("Host=database;Database=traffic_light;Username=postgres;Password=postgres");
}
