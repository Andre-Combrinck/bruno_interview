using Bruno.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bruno.Infrastructure.Persistence;

public sealed class BrunoDbContext : DbContext
{
    public BrunoDbContext(DbContextOptions<BrunoDbContext> options) : base(options)
    {
    }

    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BrunoDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
