using System.ComponentModel.DataAnnotations;

namespace DatabaseService.Database;

public class OperationModeModel
{
    [Key] public int Id { get; set; }
    public required string OperationMode { get; set; }
    public required DateTime CreatedAt { get; set; }
}