using Bruno.Application.Common.Interfaces;
using Bruno.Domain.Entities;
using Bruno.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Bruno.Infrastructure.Persistence.Repositories;

public sealed class BookingRepository : IBookingRepository
{
    private readonly BrunoDbContext _db;

    public BookingRepository(BrunoDbContext db)
    {
        _db = db;
    }

    public async Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _db.Bookings.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Booking>> GetActiveByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default) =>
        await _db.Bookings
            .Where(b => b.VehicleId == vehicleId && b.Status == BookingStatus.Active)
            .ToListAsync(cancellationToken);

    public async Task<bool> CustomerHasAnyBookingsAsync(Guid customerId, CancellationToken cancellationToken = default) =>
        await _db.Bookings.AnyAsync(b => b.CustomerId == customerId, cancellationToken);

    public async Task<(IReadOnlyList<Booking> Items, int TotalCount)> GetPagedAsync(
        Guid? vehicleId,
        Guid? customerId,
        BookingStatus? status,
        DateOnly? from,
        DateOnly? to,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Bookings.AsQueryable();

        if (vehicleId.HasValue)
        {
            query = query.Where(b => b.VehicleId == vehicleId.Value);
        }

        if (customerId.HasValue)
        {
            query = query.Where(b => b.CustomerId == customerId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(b => b.Status == status.Value);
        }

        if (from.HasValue)
        {
            query = query.Where(b => b.EndDate >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(b => b.StartDate <= to.Value);
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(b => b.StartDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task AddAsync(Booking booking, CancellationToken cancellationToken = default) =>
        await _db.Bookings.AddAsync(booking, cancellationToken);

    public void Update(Booking booking) => _db.Bookings.Update(booking);

    public void Remove(Booking booking) => _db.Bookings.Remove(booking);
}
