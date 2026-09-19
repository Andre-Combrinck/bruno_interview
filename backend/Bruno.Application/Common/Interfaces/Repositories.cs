using Bruno.Domain.Entities;
using Bruno.Domain.Enums;

namespace Bruno.Application.Common.Interfaces;

public interface IVehicleRepository
{
    Task<Vehicle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Vehicle?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> RegistrationExistsAsync(string registrationNumber, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Vehicle> Items, int TotalCount)> GetPagedAsync(
        string? search,
        bool includeDeleted,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task AddAsync(Vehicle vehicle, CancellationToken cancellationToken = default);
    void Update(Vehicle vehicle);
}

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Customer> Items, int TotalCount)> GetPagedAsync(
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task AddAsync(Customer customer, CancellationToken cancellationToken = default);
    void Update(Customer customer);
    void Remove(Customer customer);
}

public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Booking>> GetActiveByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default);
    Task<bool> CustomerHasAnyBookingsAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Booking> Items, int TotalCount)> GetPagedAsync(
        Guid? vehicleId,
        Guid? customerId,
        BookingStatus? status,
        DateOnly? from,
        DateOnly? to,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task AddAsync(Booking booking, CancellationToken cancellationToken = default);
    void Update(Booking booking);
    void Remove(Booking booking);
}

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
