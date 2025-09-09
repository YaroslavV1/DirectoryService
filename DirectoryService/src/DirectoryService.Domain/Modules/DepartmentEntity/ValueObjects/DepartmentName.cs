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
        if (value.Length <= Constants.MIN_DEPARTMENT_NAME_LENGTH && value.Length >= Constants.MAX_DEPARTMENT_NAME_LENGTH)
        {
            return Result.Failure<DepartmentName>(
                $"Name must be between {Constants.MIN_DEPARTMENT_NAME_LENGTH} and {Constants.MAX_DEPARTMENT_NAME_LENGTH} length!");
        }

        return new DepartmentName(value);
    }
}