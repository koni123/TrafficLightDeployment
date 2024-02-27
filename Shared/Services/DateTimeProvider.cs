namespace Shared.Services;

public interface IDateTimeProvider
{
    public DateTime NowUtc { get; }
}

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime NowUtc => DateTime.UtcNow;
}