using Bruno.Application.Common.Dtos;
using Bruno.Domain.Entities;

namespace Bruno.Application.Common.Mappings;

public static class Mapper
{
    public static VehicleDto ToDto(this Vehicle vehicle) =>
        new(vehicle.Id, vehicle.RegistrationNumber, vehicle.Make, vehicle.Model, vehicle.Year, vehicle.DailyRate, vehicle.IsDeleted, vehicle.CreatedDate);

    public static CustomerDto ToDto(this Customer customer) =>
        new(customer.Id, customer.FirstName, customer.LastName, customer.Email, customer.PhoneNumber, customer.CreatedDate);

    public static BookingDto ToDto(this Booking booking) =>
        new(booking.Id, booking.VehicleId, booking.CustomerId, booking.StartDate, booking.EndDate, booking.TotalPrice, booking.Status, booking.CreatedDate);
}
