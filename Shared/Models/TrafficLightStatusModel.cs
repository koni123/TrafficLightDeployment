using System.ComponentModel.DataAnnotations;

namespace Shared.Models;

public class TrafficLightStatusModel
{
    [Key] public int Id { get; set; }
    public required string TrafficLightId { get; set; }
    public required string Color { get; set; }
    public required DateTime LastChanged { get; set; }
}