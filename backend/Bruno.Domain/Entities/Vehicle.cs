using Bruno.Domain.Common;

namespace Bruno.Domain.Entities;

public sealed class Vehicle : Entity
{
    public string RegistrationNumber { get; private set; } = null!;
    public string Make { get; private set; } = null!;
    public string Model { get; private set; } = null!;
    public int Year { get; private set; }
    public decimal DailyRate { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime CreatedDate { get; private set; }

    private Vehicle()
    {
    }

    public static Vehicle Create(
        string registrationNumber,
        string make,
        string model,
        int year,
        decimal dailyRate)
    {
        Validate(registrationNumber, make, model, year, dailyRate);

        return new Vehicle
        {
            RegistrationNumber = ValueObjects.RegistrationNumber.Normalize(registrationNumber),
            Make = make.Trim(),
            Model = model.Trim(),
            Year = year,
            DailyRate = dailyRate,
            IsDeleted = false,
            CreatedDate = DateTime.UtcNow
        };
    }

    public void Update(string registrationNumber, string make, string model, int year, decimal dailyRate)
    {
        if (IsDeleted)
        {
            throw new DomainException("Cannot update a soft-deleted vehicle.");
        }

        Validate(registrationNumber, make, model, year, dailyRate);

        RegistrationNumber = ValueObjects.RegistrationNumber.Normalize(registrationNumber);
        Make = make.Trim();
        Model = model.Trim();
        Year = year;
        DailyRate = dailyRate;
    }

    public void SoftDelete()
    {
        if (IsDeleted)
        {
            throw new DomainException("Vehicle is already deleted.");
        }

        IsDeleted = true;
    }

    public void EnsureBookable()
    {
        if (IsDeleted)
        {
            throw new DomainException("Cannot book a soft-deleted vehicle.");
        }
    }

    private static void Validate(string registrationNumber, string make, string model, int year, decimal dailyRate)
    {
        ValueObjects.RegistrationNumber.ValidateOrThrow(registrationNumber);

        if (string.IsNullOrWhiteSpace(make))
        {
            throw new DomainException("Make is required.");
        }

        if (string.IsNullOrWhiteSpace(model))
        {
            throw new DomainException("Model is required.");
        }

        if (year < 1980 || year > DateTime.UtcNow.Year + 1)
        {
            throw new DomainException("Year is out of valid range.");
        }

        if (dailyRate <= 0)
        {
            throw new DomainException("Daily rate must be greater than zero.");
        }
    }
}
