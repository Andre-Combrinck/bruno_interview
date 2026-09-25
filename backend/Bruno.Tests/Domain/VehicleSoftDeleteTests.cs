using Bruno.Domain.Common;
using Bruno.Domain.Entities;
using FluentAssertions;

namespace Bruno.Tests.Domain;

public class VehicleSoftDeleteTests
{
    [Fact]
    public void Restore_clears_deleted_flag()
    {
        var vehicle = Vehicle.Create("CA111222", "Toyota", "Corolla", 2022, 100m);
        vehicle.SoftDelete();

        vehicle.Restore();

        vehicle.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Restore_rejects_active_vehicle()
    {
        var vehicle = Vehicle.Create("CA111222", "Toyota", "Corolla", 2022, 100m);

        var act = () => vehicle.Restore();

        act.Should().Throw<DomainException>().WithMessage("Vehicle is not deleted.");
    }

    [Fact]
    public void SoftDelete_then_Restore_allows_booking()
    {
        var vehicle = Vehicle.Create("CA111222", "Toyota", "Corolla", 2022, 100m);
        vehicle.SoftDelete();

        vehicle.Restore();

        var act = () => vehicle.EnsureBookable();

        act.Should().NotThrow();
    }
}
