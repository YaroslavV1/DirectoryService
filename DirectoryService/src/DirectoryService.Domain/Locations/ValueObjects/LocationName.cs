using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;
using DirectoryService.Shared;

namespace DirectoryService.Domain.Locations.ValueObjects;

public record LocationName
{
    public string Value { get; }

    private LocationName(string value)
    {
        Value = value;
    }

    public static Result<LocationName, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return GeneralErrors.ValueIsRequired("LocationName");
        if (value.Length < LengthConstants.MIN_LOCATION_NAME && value.Length > LengthConstants.MAX_LOCATION_NAME)
        {
            return GeneralErrors.ValueIsInvalid("LocationName");
        }

        return new LocationName(value);
    }

}