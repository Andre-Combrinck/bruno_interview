using Bruno.Domain.Common;

namespace Bruno.Domain.Entities;

public sealed class Customer : Entity
{
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string PhoneNumber { get; private set; } = null!;
    public DateTime CreatedDate { get; private set; }

    private Customer()
    {
    }

    public static Customer Create(string firstName, string lastName, string email, string phoneNumber)
    {
        Validate(firstName, lastName, email, phoneNumber);

        return new Customer
        {
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            PhoneNumber = phoneNumber.Trim(),
            CreatedDate = DateTime.UtcNow
        };
    }

    public void Update(string firstName, string lastName, string email, string phoneNumber)
    {
        Validate(firstName, lastName, email, phoneNumber);

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Email = email.Trim().ToLowerInvariant();
        PhoneNumber = phoneNumber.Trim();
    }

    public void EnsureCanBeDeleted(bool hasBookings)
    {
        if (hasBookings)
        {
            throw new DomainException("Cannot delete a customer with existing bookings.");
        }
    }

    private static void Validate(string firstName, string lastName, string email, string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            throw new DomainException("First name is required.");
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            throw new DomainException("Last name is required.");
        }

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
        {
            throw new DomainException("A valid email is required.");
        }

        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            throw new DomainException("Phone number is required.");
        }
    }
}
