using Bruno.Domain.Common;

namespace Bruno.Domain.Events;

public sealed class BookingCancelledEvent : IDomainEvent
{
    public Guid BookingId { get; }
    public Guid VehicleId { get; }
    public Guid CustomerId { get; }
    public DateTime OccurredOnUtc { get; }

    public BookingCancelledEvent(Guid bookingId, Guid vehicleId, Guid customerId)
    {
        BookingId = bookingId;
        VehicleId = vehicleId;
        CustomerId = customerId;
        OccurredOnUtc = DateTime.UtcNow;
    }
}
