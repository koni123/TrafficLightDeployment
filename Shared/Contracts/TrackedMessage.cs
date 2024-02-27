namespace Shared.Contracts;

public interface ITrackedMessage
{ 
    public string CorrelationId { get; }
}