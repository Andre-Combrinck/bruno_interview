using Bruno.Domain.Common;

namespace Bruno.Domain.ValueObjects;

public sealed class DateRange : IEquatable<DateRange>
{
    public DateOnly StartDate { get; }
    public DateOnly EndDate { get; }

    public int DayCount => EndDate.DayNumber - StartDate.DayNumber + 1;

    private DateRange(DateOnly startDate, DateOnly endDate)
    {
        StartDate = startDate;
        EndDate = endDate;
    }

    public static DateRange Create(DateOnly startDate, DateOnly endDate)
    {
        if (endDate <= startDate)
        {
            throw new DomainException("EndDate must be greater than StartDate.");
        }

        return new DateRange(startDate, endDate);
    }

    /// <summary>
    /// Closed-interval overlap (no same-day handoff): ranges conflict when they share any day
    /// or touch on a boundary day.
    /// </summary>
    public bool OverlapsInclusive(DateRange other) =>
        StartDate <= other.EndDate && other.StartDate <= EndDate;

    public bool Equals(DateRange? other) =>
        other is not null && StartDate == other.StartDate && EndDate == other.EndDate;

    public override bool Equals(object? obj) => obj is DateRange other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(StartDate, EndDate);
}
