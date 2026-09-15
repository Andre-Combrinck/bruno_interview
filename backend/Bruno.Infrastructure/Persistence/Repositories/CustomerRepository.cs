using Bruno.Application.Common.Interfaces;
using Bruno.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bruno.Infrastructure.Persistence.Repositories;

public sealed class CustomerRepository : ICustomerRepository
{
    private readonly BrunoDbContext _db;

    public CustomerRepository(BrunoDbContext db)
    {
        _db = db;
    }

    public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _db.Customers.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<bool> EmailExistsAsync(string email, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return await _db.Customers.AnyAsync(
            c => c.Email == normalized && (!excludeId.HasValue || c.Id != excludeId.Value),
            cancellationToken);
    }

    public async Task<(IReadOnlyList<Customer> Items, int TotalCount)> GetPagedAsync(
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Customers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(c =>
                c.FirstName.ToLower().Contains(term) ||
                c.LastName.ToLower().Contains(term) ||
                c.Email.ToLower().Contains(term) ||
                c.PhoneNumber.Contains(term));
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task AddAsync(Customer customer, CancellationToken cancellationToken = default) =>
        await _db.Customers.AddAsync(customer, cancellationToken);

    public void Update(Customer customer) => _db.Customers.Update(customer);

    public void Remove(Customer customer) => _db.Customers.Remove(customer);
}
