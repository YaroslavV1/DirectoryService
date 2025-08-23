using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.DepartmentEntity.ValueObjects;

public record DepartmentName
{
    public string Value { get; private set; }

    private const short MIN_LENGTH = 3;
    private const short MAX_LENGTH = 150;

    private DepartmentName(string value)
    {
        Value = value;
    }

    public static Result<DepartmentName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<DepartmentName>("Name is required!");
        if (value.Length <= MIN_LENGTH && value.Length >= MAX_LENGTH)
            return Result.Failure<DepartmentName>($"Name must be between {MIN_LENGTH} and {MAX_LENGTH} length!");

        return new DepartmentName(value);
    }
}