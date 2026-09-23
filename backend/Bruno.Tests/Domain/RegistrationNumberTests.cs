using Bruno.Domain.Common;
using Bruno.Domain.Entities;
using Bruno.Domain.ValueObjects;
using FluentAssertions;

namespace Bruno.Tests.Domain;

public class RegistrationNumberTests
{
    [Theory]
    [InlineData("CA 123-456", "CA123456")]
    [InlineData("  cy-12345 ", "CY12345")]
    [InlineData("ab12cd gp", "AB12CDGP")]
    [InlineData("cool1-gp", "COOL1GP")]
    public void Normalize_strips_spaces_dashes_and_uppercases(string input, string expected)
    {
        RegistrationNumber.Normalize(input).Should().Be(expected);
    }

    [Theory]
    [InlineData("CA123456")]
    [InlineData("CA 123-456")]
    [InlineData("CY12345")]
    [InlineData("AB12CDGP")]
    [InlineData("AB 12 CD GP")]
    [InlineData("BND123GP")]
    [InlineData("COOL1GP")]
    [InlineData("DADWC")]
    [InlineData("ABC123KZN")]
    public void IsValid_accepts_broad_south_african_plates(string input)
    {
        RegistrationNumber.IsValid(input).Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("12")]
    [InlineData("123456")]
    [InlineData("ABCDEFGHIJKLM")]
    [InlineData("!!!")]
    [InlineData("123GP")]
    [InlineData("GP")]
    [InlineData("CA*123")]
    public void IsValid_rejects_invalid_plates(string input)
    {
        RegistrationNumber.IsValid(input).Should().BeFalse();
    }

    [Fact]
    public void ValidateOrThrow_raises_domain_exception()
    {
        var act = () => RegistrationNumber.ValidateOrThrow("NOT A PLATE!!!");

        act.Should().Throw<DomainException>().WithMessage(RegistrationNumber.InvalidMessage);
    }

    [Fact]
    public void Vehicle_Create_stores_normalized_registration()
    {
        var vehicle = Vehicle.Create("ca 999-888", "Toyota", "Hilux", 2024, 800m);

        vehicle.RegistrationNumber.Should().Be("CA999888");
    }

    [Fact]
    public void Vehicle_Create_rejects_invalid_registration()
    {
        var act = () => Vehicle.Create("INVALID!!", "Toyota", "Hilux", 2024, 800m);

        act.Should().Throw<DomainException>().WithMessage(RegistrationNumber.InvalidMessage);
    }
}
