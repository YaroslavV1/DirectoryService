using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;
using DirectoryService.Shared;

namespace DirectoryService.Domain.Modules.DepartmentEntity.ValueObjects;

public record DepartmentName
{
    public string Value { get; private set; }

    private DepartmentName(string value)
    {
        Value = value;
    }

    public static Result<DepartmentName, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return GeneralErrors.ValueIsRequired("DepartmentName");
        if (value.Length <= LengthConstants.MIN_DEPARTMENT_NAME && value.Length >= LengthConstants.MAX_DEPARTMENT_NAME)
        {
            return GeneralErrors.ValueIsInvalid("DepartmentName");
        }

        return new DepartmentName(value);
    }
}