using Bruno.Application.Common.Interfaces;
using Bruno.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bruno.Infrastructure.Persistence.Repositories;

public sealed class VehicleRepository : IVehicleRepository
{
    private readonly BrunoDbContext _db;

    public VehicleRepository(BrunoDbContext db)
    {
        _db = db;
    }

    public async Task<Vehicle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _db.Vehicles.FirstOrDefaultAsync(v => v.Id == id, cancellationToken);

    public async Task<Vehicle?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _db.Vehicles.IgnoreQueryFilters().FirstOrDefaultAsync(v => v.Id == id, cancellationToken);

    public async Task<bool> RegistrationExistsAsync(
        string registrationNumber,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var normalized = registrationNumber.Trim().ToUpperInvariant();
        return await _db.Vehicles
            .IgnoreQueryFilters()
            .AnyAsync(
                v => v.RegistrationNumber == normalized && (!excludeId.HasValue || v.Id != excludeId.Value),
                cancellationToken);
    }

    public async Task<(IReadOnlyList<Vehicle> Items, int TotalCount)> GetPagedAsync(
        string? search,
        bool includeDeleted,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Vehicle> query = includeDeleted
            ? _db.Vehicles.IgnoreQueryFilters()
            : _db.Vehicles;

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(v =>
                v.RegistrationNumber.ToLower().Contains(term) ||
                v.Make.ToLower().Contains(term) ||
                v.Model.ToLower().Contains(term));
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(v => v.RegistrationNumber)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task AddAsync(Vehicle vehicle, CancellationToken cancellationToken = default) =>
        await _db.Vehicles.AddAsync(vehicle, cancellationToken);

    public void Update(Vehicle vehicle) => _db.Vehicles.Update(vehicle);
}
