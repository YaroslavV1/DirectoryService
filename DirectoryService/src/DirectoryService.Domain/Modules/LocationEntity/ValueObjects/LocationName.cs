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
        if (value.Length < Constants.MIN_LOCATION_NAME_LENGTH && value.Length > Constants.MAX_LOCATION_NAME_LENGTH)
        {
            return Result.Failure<LocationName>(
                $"Location name must be between {Constants.MIN_LOCATION_NAME_LENGTH} and {Constants.MAX_LOCATION_NAME_LENGTH} characters");
        }

        return new LocationName(value);
    }

}