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
        if(value.Length < LengthConstants.MIN_POSITION_NAME && value.Length > LengthConstants.MAX_POSITION_NAME)
        {
            return Result.Failure<PositionName>(
                $"Position name must be between {LengthConstants.MIN_POSITION_NAME} and {LengthConstants.MAX_POSITION_NAME} length!");
        }

        return new PositionName(value);
    }
}