using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Domain.Modules.DepartmentEntity.ValueObjects;

public record DepartmentName
{
    public string Value { get; private set; }

    private DepartmentName(string value)
    {
        Value = value;
    }

    public static Result<DepartmentName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<DepartmentName>("Name is required!");
        if (value.Length <= LengthConstants.MIN_DEPARTMENT_NAME && value.Length >= LengthConstants.MAX_DEPARTMENT_NAME)
        {
            return Result.Failure<DepartmentName>(
                $"Name must be between {LengthConstants.MIN_DEPARTMENT_NAME} and {LengthConstants.MAX_DEPARTMENT_NAME} length!");
        }

        return new DepartmentName(value);
    }
}