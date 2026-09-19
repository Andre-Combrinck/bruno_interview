using Bruno.Domain.Enums;

namespace Bruno.Application.Common.Dtos;

public sealed record VehicleDto(
    Guid Id,
    string RegistrationNumber,
    string Make,
    string Model,
    int Year,
    decimal DailyRate,
    bool IsDeleted,
    DateTime CreatedDate);

public sealed record CustomerDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    DateTime CreatedDate);

public sealed record BookingDto(
    Guid Id,
    Guid VehicleId,
    Guid CustomerId,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal TotalPrice,
    BookingStatus Status,
    DateTime CreatedDate);
