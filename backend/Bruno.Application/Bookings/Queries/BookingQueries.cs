using Bruno.Application.Common.Dtos;
using Bruno.Application.Common.Interfaces;
using Bruno.Application.Common.Mappings;
using Bruno.Application.Common.Models;
using Bruno.Domain.Common;
using Bruno.Domain.Enums;
using MediatR;

namespace Bruno.Application.Bookings.Queries;

public sealed record GetBookingByIdQuery(Guid Id) : IRequest<BookingDto>;

public sealed class GetBookingByIdQueryHandler : IRequestHandler<GetBookingByIdQuery, BookingDto>
{
    private readonly IBookingRepository _bookings;

    public GetBookingByIdQueryHandler(IBookingRepository bookings)
    {
        _bookings = bookings;
    }

    public async Task<BookingDto> Handle(GetBookingByIdQuery request, CancellationToken cancellationToken)
    {
        var booking = await _bookings.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new DomainException("Booking not found.");

        return booking.ToDto();
    }
}

public sealed record GetBookingsQuery(
    Guid? VehicleId,
    Guid? CustomerId,
    BookingStatus? Status,
    DateOnly? From,
    DateOnly? To,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<BookingDto>>;

public sealed class GetBookingsQueryHandler : IRequestHandler<GetBookingsQuery, PagedResult<BookingDto>>
{
    private readonly IBookingRepository _bookings;

    public GetBookingsQueryHandler(IBookingRepository bookings)
    {
        _bookings = bookings;
    }

    public async Task<PagedResult<BookingDto>> Handle(GetBookingsQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;

        var (items, total) = await _bookings.GetPagedAsync(
            request.VehicleId,
            request.CustomerId,
            request.Status,
            request.From,
            request.To,
            page,
            pageSize,
            cancellationToken);

        return new PagedResult<BookingDto>
        {
            Items = items.Select(b => b.ToDto()).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }
}
