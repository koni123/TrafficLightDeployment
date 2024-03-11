using System.ComponentModel.DataAnnotations;

namespace DatabaseService.Database;

public class TrafficLightStatusModel
{
    [Key] public int Id { get; set; }
    public required string TrafficLightId { get; set; }
    public required string Color { get; set; }
    public required DateTime LastChanged { get; set; }
    public required DateTime ModifiedAt { get; set; }
}