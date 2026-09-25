using Bruno.Application.Common.Interfaces;
using Bruno.Application.Customers.Commands;
using Bruno.Application.Vehicles.Commands;
using Bruno.Domain.Common;
using Bruno.Domain.Entities;
using FluentAssertions;
using Moq;

namespace Bruno.Tests.Application;

public class CommandHandlerTests
{
    [Fact]
    public async Task CreateVehicleCommand_persists_unique_vehicle()
    {
        var vehicles = new Mock<IVehicleRepository>();
        var uow = new Mock<IUnitOfWork>();
        vehicles.Setup(v => v.RegistrationExistsAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        vehicles.Setup(v => v.AddAsync(It.IsAny<Vehicle>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new CreateVehicleCommandHandler(vehicles.Object, uow.Object);
        var result = await handler.Handle(
            new CreateVehicleCommand("ca999888", "Toyota", "Hilux", 2024, 800m),
            CancellationToken.None);

        result.RegistrationNumber.Should().Be("CA999888");
        result.DailyRate.Should().Be(800m);
        vehicles.Verify(v => v.AddAsync(It.IsAny<Vehicle>(), It.IsAny<CancellationToken>()), Times.Once);
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateVehicleCommand_rejects_duplicate_registration()
    {
        var vehicles = new Mock<IVehicleRepository>();
        var uow = new Mock<IUnitOfWork>();
        vehicles.Setup(v => v.RegistrationExistsAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new CreateVehicleCommandHandler(vehicles.Object, uow.Object);

        var act = async () => await handler.Handle(
            new CreateVehicleCommand("CA999888", "Toyota", "Hilux", 2024, 800m),
            CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*unique*");
    }

    [Fact]
    public async Task DeleteCustomerCommand_blocks_when_any_bookings_exist()
    {
        var customer = Customer.Create("Amy", "Adams", "amy@example.com", "+27000000000");
        var customers = new Mock<ICustomerRepository>();
        var bookings = new Mock<IBookingRepository>();
        var uow = new Mock<IUnitOfWork>();

        customers.Setup(c => c.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(customer);
        bookings.Setup(b => b.CustomerHasAnyBookingsAsync(customer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var handler = new DeleteCustomerCommandHandler(customers.Object, bookings.Object, uow.Object);

        var act = async () => await handler.Handle(new DeleteCustomerCommand(customer.Id), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*existing bookings*");
    }

    [Fact]
    public async Task RestoreVehicleCommand_restores_soft_deleted_vehicle()
    {
        var vehicle = Vehicle.Create("CA111222", "Toyota", "Corolla", 2022, 100m);
        vehicle.SoftDelete();

        var vehicles = new Mock<IVehicleRepository>();
        var uow = new Mock<IUnitOfWork>();
        vehicles.Setup(v => v.GetByIdIncludingDeletedAsync(vehicle.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);
        uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new RestoreVehicleCommandHandler(vehicles.Object, uow.Object);
        await handler.Handle(new RestoreVehicleCommand(vehicle.Id), CancellationToken.None);

        vehicle.IsDeleted.Should().BeFalse();
        vehicles.Verify(v => v.Update(vehicle), Times.Once);
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RestoreVehicleCommand_rejects_missing_vehicle()
    {
        var vehicles = new Mock<IVehicleRepository>();
        var uow = new Mock<IUnitOfWork>();
        vehicles.Setup(v => v.GetByIdIncludingDeletedAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vehicle?)null);

        var handler = new RestoreVehicleCommandHandler(vehicles.Object, uow.Object);

        var act = async () => await handler.Handle(new RestoreVehicleCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>().WithMessage("Vehicle not found.");
    }

    [Fact]
    public async Task RestoreVehicleCommand_rejects_active_vehicle()
    {
        var vehicle = Vehicle.Create("CA111222", "Toyota", "Corolla", 2022, 100m);
        var vehicles = new Mock<IVehicleRepository>();
        var uow = new Mock<IUnitOfWork>();
        vehicles.Setup(v => v.GetByIdIncludingDeletedAsync(vehicle.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);

        var handler = new RestoreVehicleCommandHandler(vehicles.Object, uow.Object);

        var act = async () => await handler.Handle(new RestoreVehicleCommand(vehicle.Id), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>().WithMessage("Vehicle is not deleted.");
    }
}
