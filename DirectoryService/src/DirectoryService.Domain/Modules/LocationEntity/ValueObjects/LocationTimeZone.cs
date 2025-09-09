using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Modules.LocationEntity.ValueObjects;

public record LocationTimeZone
{

    private static readonly Regex _ianaRegex = new(
        @"^[A-Z][a-zA-Z]*(?:/[A-Z][a-zA-Z_]+)+$",
        RegexOptions.Compiled);

    public string Value { get; }

    private LocationTimeZone(string value) => Value = value;

    public static Result<LocationTimeZone> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<LocationTimeZone>("Timezone is required!");

        if (!_ianaRegex.IsMatch(value))
            return Result.Failure<LocationTimeZone>("Invalid IANA timezone format.");

        return new LocationTimeZone(value);
    }
}