using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.PositionEntity.ValueObjects;

public record PositionName
{
    private const short MIN_LENGTH = 3;
    private const short MAX_LENGTH = 100;

    public string Value { get; }

    private PositionName(string value)
    {
        Value = value;
    }

    public static Result<PositionName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<PositionName>("Position name cannot be empty!");
        if(value.Length < MIN_LENGTH && value.Length > MAX_LENGTH)
            return Result.Failure<PositionName>($"Position name must be between {MIN_LENGTH} and {MAX_LENGTH} length!");

        return new PositionName(value);
    }
}