using Bruno.Domain.Common;
using Bruno.Domain.Enums;
using Bruno.Domain.Events;
using Bruno.Domain.ValueObjects;

namespace Bruno.Domain.Entities;

public sealed class Booking : Entity
{
    public Guid VehicleId { get; private set; }
    public Guid CustomerId { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public decimal TotalPrice { get; private set; }
    public BookingStatus Status { get; private set; }
    public DateTime CreatedDate { get; private set; }

    public DateRange DateRange => DateRange.Create(StartDate, EndDate);

    private Booking()
    {
    }

    public static Booking Create(
        Vehicle vehicle,
        Guid customerId,
        DateOnly startDate,
        DateOnly endDate,
        IEnumerable<Booking> existingActiveBookingsForVehicle)
    {
        vehicle.EnsureBookable();

        var range = DateRange.Create(startDate, endDate);

        foreach (var existing in existingActiveBookingsForVehicle.Where(b => b.Status == BookingStatus.Active))
        {
            if (range.OverlapsInclusive(existing.DateRange))
            {
                throw new DomainException("Booking dates overlap an existing active booking for this vehicle.");
            }
        }

        var totalPrice = vehicle.DailyRate * range.DayCount;

        return new Booking
        {
            VehicleId = vehicle.Id,
            CustomerId = customerId,
            StartDate = range.StartDate,
            EndDate = range.EndDate,
            TotalPrice = totalPrice,
            Status = BookingStatus.Active,
            CreatedDate = DateTime.UtcNow
        };
    }

    public void ChangeStatus(BookingStatus newStatus)
    {
        if (Status == newStatus)
        {
            return;
        }

        Status = newStatus switch
        {
            BookingStatus.Completed when Status == BookingStatus.Active => BookingStatus.Completed,
            BookingStatus.Cancelled when Status == BookingStatus.Active => BookingStatus.Cancelled,
            BookingStatus.Active => throw new DomainException("Cannot revert a booking to Active."),
            BookingStatus.Completed => throw new DomainException("Only active bookings can be completed."),
            BookingStatus.Cancelled => throw new DomainException("Only active bookings can be cancelled."),
            _ => throw new DomainException($"Unsupported booking status transition to {newStatus}.")
        };

        if (Status == BookingStatus.Cancelled)
        {
            RaiseDomainEvent(new BookingCancelledEvent(Id, VehicleId, CustomerId));
        }
    }

    public void EnsureCanBeDeleted(DateOnly todayUtc)
    {
        if (StartDate <= todayUtc)
        {
            throw new DomainException("Can only delete bookings with a StartDate in the future.");
        }
    }
}
