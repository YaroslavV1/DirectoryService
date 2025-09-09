using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Domain.Modules.PositionEntity.ValueObjects;

public record PositionName
{
    public string Value { get; }

    private PositionName(string value)
    {
        Value = value;
    }

    public static Result<PositionName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<PositionName>("Position name cannot be empty!");
        if(value.Length < Constants.MIN_POSITION_NAME_LENGTH && value.Length > Constants.MAX_POSITION_NAME_LENGTH)
        {
            return Result.Failure<PositionName>(
                $"Position name must be between {Constants.MIN_POSITION_NAME_LENGTH} and {Constants.MAX_POSITION_NAME_LENGTH} length!");
        }

        return new PositionName(value);
    }
}