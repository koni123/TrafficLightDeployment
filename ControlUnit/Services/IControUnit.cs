namespace ControlUnit.Services;

public interface IControlUnit
{
    public Task RunOperation(string? operationMode);
}