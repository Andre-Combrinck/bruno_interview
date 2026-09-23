using System.Text.RegularExpressions;
using Bruno.Domain.Common;

namespace Bruno.Domain.ValueObjects;

/// <summary>
/// South African vehicle registration helpers: normalize and validate plate formats
/// (legacy provincial, newer provincial with suffix, and personalised).
/// </summary>
public static class RegistrationNumber
{
    public const int MinLength = 3;
    public const int MaxLength = 12;

    public const string InvalidMessage = "Registration number must be a valid South African plate.";

    private static readonly Regex LegacyPattern = new(
        @"^[A-Z]{1,3}\d{1,6}$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>
    /// Longest-first so KZN is preferred over a trailing two-letter match.
    /// </summary>
    private static readonly string[] ProvinceSuffixes =
    [
        "KZN",
        "EC", "FS", "GP", "LP", "MP", "NC", "NW", "WC", "WP"
    ];

    public static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return value
            .Trim()
            .Replace(" ", string.Empty, StringComparison.Ordinal)
            .Replace("-", string.Empty, StringComparison.Ordinal)
            .ToUpperInvariant();
    }

    public static bool IsValid(string? value)
    {
        var normalized = Normalize(value);

        if (normalized.Length is < MinLength or > MaxLength)
        {
            return false;
        }

        if (!normalized.All(static c => c is >= 'A' and <= 'Z' or >= '0' and <= '9'))
        {
            return false;
        }

        if (LegacyPattern.IsMatch(normalized))
        {
            return true;
        }

        if (!TrySplitProvince(normalized, out var body))
        {
            return false;
        }

        if (body.Length is < 1 or > 8 || !body.Any(char.IsLetter))
        {
            return false;
        }

        // Newer provincial: 2–8 alphanumeric with at least one digit.
        if (body.Length is >= 2 and <= 8 && body.Any(char.IsDigit))
        {
            return true;
        }

        // Personalised: 1–7 alphanumeric with at least one letter (digit optional).
        return body.Length <= 7;
    }

    public static void ValidateOrThrow(string? value)
    {
        if (!IsValid(value))
        {
            throw new DomainException(InvalidMessage);
        }
    }

    private static bool TrySplitProvince(string normalized, out string body)
    {
        foreach (var suffix in ProvinceSuffixes)
        {
            if (normalized.Length > suffix.Length &&
                normalized.EndsWith(suffix, StringComparison.Ordinal))
            {
                body = normalized[..^suffix.Length];
                return true;
            }
        }

        body = string.Empty;
        return false;
    }
}
