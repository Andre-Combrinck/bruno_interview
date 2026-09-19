using Bruno.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Bruno.Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BrunoDbContext>();
        await db.Database.MigrateAsync();

        if (await db.Vehicles.IgnoreQueryFilters().AnyAsync())
        {
            return;
        }

        var vehicles = new[]
        {
            Vehicle.Create("CA123456", "Toyota", "Corolla", 2022, 450m),
            Vehicle.Create("GP987654", "Volkswagen", "Polo", 2021, 380m),
            Vehicle.Create("KZN555111", "Ford", "Ranger", 2023, 750m)
        };

        var customers = new[]
        {
            Customer.Create("Alice", "Nkosi", "alice.nkosi@example.com", "+27821234567"),
            Customer.Create("Ben", "Botha", "ben.botha@example.com", "+27829876543")
        };

        db.Vehicles.AddRange(vehicles);
        db.Customers.AddRange(customers);
        await db.SaveChangesAsync();

        var booking = Booking.Create(
            vehicles[0],
            customers[0].Id,
            DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(10)),
            DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(14)),
            Array.Empty<Booking>());

        db.Bookings.Add(booking);
        await db.SaveChangesAsync();
    }
}
