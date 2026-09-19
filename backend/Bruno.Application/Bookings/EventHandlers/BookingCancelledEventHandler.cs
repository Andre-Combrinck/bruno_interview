using Bruno.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bruno.Application.Bookings.EventHandlers;

public sealed class BookingCancelledEventHandler : INotificationHandler<BookingCancelledEvent>
{
    private readonly ILogger<BookingCancelledEventHandler> _logger;

    public BookingCancelledEventHandler(ILogger<BookingCancelledEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(BookingCancelledEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Booking {BookingId} cancelled for vehicle {VehicleId} and customer {CustomerId} at {OccurredOnUtc}",
            notification.BookingId,
            notification.VehicleId,
            notification.CustomerId,
            notification.OccurredOnUtc);

        return Task.CompletedTask;
    }
}
