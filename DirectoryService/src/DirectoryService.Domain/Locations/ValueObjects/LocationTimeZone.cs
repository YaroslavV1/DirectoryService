using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using DirectoryService.Shared;

namespace DirectoryService.Domain.Locations.ValueObjects;

public record LocationTimeZone
{
    private static readonly Regex _ianaRegex = new(
        @"^[A-Z][a-zA-Z]*(?:/[A-Z][a-zA-Z_]+)+$",
        RegexOptions.Compiled);

    public string Value { get; }

    private LocationTimeZone(string value) => Value = value;

    public static Result<LocationTimeZone, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return GeneralErrors.ValueIsRequired("LocationTimeZone");
        if (!_ianaRegex.IsMatch(value))
            return GeneralErrors.ValueIsInvalid("LocationTimeZone");
        return new LocationTimeZone(value);
    }
}