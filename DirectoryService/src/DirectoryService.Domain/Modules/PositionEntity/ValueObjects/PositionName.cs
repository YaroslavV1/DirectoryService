using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;
using DirectoryService.Shared;

namespace DirectoryService.Domain.Modules.PositionEntity.ValueObjects;

public record PositionName
{
    public string Value { get; }

    private PositionName(string value)
    {
        Value = value;
    }

    public static Result<PositionName, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return GeneralErrors.ValueIsRequired("PositionName");
        if(value.Length < LengthConstants.MIN_POSITION_NAME && value.Length > LengthConstants.MAX_POSITION_NAME)
        {
            return GeneralErrors.ValueIsInvalid("PositionName");
        }

        return new PositionName(value);
    }
}