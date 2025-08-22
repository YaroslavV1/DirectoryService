using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.LocationEntity.ValueObjects;

public record LocationName
{
    private const int MIN_LENGTH = 1;
    private const int MAX_LENGTH = 120;

    public string Value { get; }

    private LocationName(string value)
    {
        Value = value;
    }

    public static Result<LocationName> Create(string value)
    {
        if (string.IsNullOrEmpty(value))
            return Result.Failure<LocationName>($"Location name cannot be null or empty");
        if (value.Length < MIN_LENGTH && value.Length > MAX_LENGTH)
            return Result.Failure<LocationName>($"Location name must be between {MIN_LENGTH} and {MAX_LENGTH} characters");

        return new LocationName(value);
    }

}