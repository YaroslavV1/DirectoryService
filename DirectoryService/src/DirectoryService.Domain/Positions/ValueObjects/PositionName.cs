using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;
using SharedService;

namespace DirectoryService.Domain.Positions.ValueObjects;

public record PositionName
{
    private PositionName(string value) => Value = value;

    public string Value { get; }

    public static Result<PositionName, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return GeneralErrors.ValueIsRequired("PositionName");
        }

        if (value.Length < LengthConstants.MIN_POSITION_NAME && value.Length > LengthConstants.MAX_POSITION_NAME)
        {
            return GeneralErrors.ValueIsInvalid("PositionName");
        }

        return new PositionName(value);
    }
}