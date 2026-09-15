using Bruno.Domain.Common;
using Bruno.Domain.Entities;
using Bruno.Domain.Enums;
using Bruno.Domain.ValueObjects;
using FluentAssertions;

namespace Bruno.Tests.Domain;

public class DateRangeAndBookingRulesTests
{
    [Fact]
    public void DateRange_uses_inclusive_day_count()
    {
        var range = DateRange.Create(new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 3));

        range.DayCount.Should().Be(3);
    }

    [Fact]
    public void DateRange_rejects_end_not_after_start()
    {
        var act = () => DateRange.Create(new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 1));

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Booking_rejects_same_day_handoff_overlap()
    {
        var vehicle = Vehicle.Create("CA111222", "Toyota", "Corolla", 2022, 100m);
        var existing = Booking.Create(
            vehicle,
            Guid.NewGuid(),
            new DateOnly(2026, 9, 1),
            new DateOnly(2026, 9, 5),
            Array.Empty<Booking>());

        var act = () => Booking.Create(
            vehicle,
            Guid.NewGuid(),
            new DateOnly(2026, 9, 5),
            new DateOnly(2026, 9, 8),
            new[] { existing });

        act.Should().Throw<DomainException>()
            .WithMessage("*overlap*");
    }

    [Fact]
    public void Booking_total_price_is_inclusive_days_times_rate()
    {
        var vehicle = Vehicle.Create("CA333444", "Ford", "Ranger", 2023, 200m);

        var booking = Booking.Create(
            vehicle,
            Guid.NewGuid(),
            new DateOnly(2026, 10, 1),
            new DateOnly(2026, 10, 3),
            Array.Empty<Booking>());

        booking.TotalPrice.Should().Be(600m);
        booking.Status.Should().Be(BookingStatus.Active);
    }

    [Fact]
    public void Booking_cannot_delete_when_start_is_today_or_past()
    {
        var vehicle = Vehicle.Create("CA555666", "VW", "Polo", 2021, 150m);
        var booking = Booking.Create(
            vehicle,
            Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.UtcNow.Date),
            DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(2)),
            Array.Empty<Booking>());

        var act = () => booking.EnsureCanBeDeleted(DateOnly.FromDateTime(DateTime.UtcNow.Date));

        act.Should().Throw<DomainException>();
    }
}
