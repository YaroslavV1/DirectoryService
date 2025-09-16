using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Domain.Modules.LocationEntity.ValueObjects;

public record LocationName
{
    public string Value { get; }

    private LocationName(string value)
    {
        Value = value;
    }

    public static Result<LocationName> Create(string value)
    {
        if (string.IsNullOrEmpty(value))
            return Result.Failure<LocationName>($"Location name cannot be null or empty");
        if (value.Length < LengthConstants.MIN_LOCATION_NAME && value.Length > LengthConstants.MAX_LOCATION_NAME)
        {
            return Result.Failure<LocationName>(
                $"Location name must be between {LengthConstants.MIN_LOCATION_NAME} and {LengthConstants.MAX_LOCATION_NAME} characters");
        }

        return new LocationName(value);
    }

}