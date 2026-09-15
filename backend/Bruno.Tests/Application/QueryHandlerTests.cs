using Bruno.Application.Common.Interfaces;
using Bruno.Application.Customers.Queries;
using Bruno.Application.Vehicles.Queries;
using Bruno.Domain.Entities;
using FluentAssertions;
using Moq;

namespace Bruno.Tests.Application;

public class QueryHandlerTests
{
    [Fact]
    public async Task GetVehiclesQuery_maps_paged_results()
    {
        var vehicle = Vehicle.Create("CA121212", "BMW", "320i", 2020, 900m);
        var vehicles = new Mock<IVehicleRepository>();
        vehicles.Setup(v => v.GetPagedAsync("bmw", false, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Vehicle> { vehicle } as IReadOnlyList<Vehicle>, 1));

        var handler = new GetVehiclesQueryHandler(vehicles.Object);
        var result = await handler.Handle(new GetVehiclesQuery("bmw", false, 1, 20), CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle(v => v.Make == "BMW");
    }

    [Fact]
    public async Task GetCustomerByIdQuery_returns_dto()
    {
        var customer = Customer.Create("Carl", "Coetzee", "carl@example.com", "+27111111111");
        var customers = new Mock<ICustomerRepository>();
        customers.Setup(c => c.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(customer);

        var handler = new GetCustomerByIdQueryHandler(customers.Object);
        var result = await handler.Handle(new GetCustomerByIdQuery(customer.Id), CancellationToken.None);

        result.Email.Should().Be("carl@example.com");
        result.FirstName.Should().Be("Carl");
    }
}
