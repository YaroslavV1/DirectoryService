using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;
using DirectoryService.Shared;

namespace DirectoryService.Domain.Departments.ValueObjects;

public record Identifier
{
    private static readonly Regex _latinRegex = new(@"^[A-Za-z]+$", RegexOptions.Compiled);

    public string Value { get; }

    private Identifier(string value)
    {
        Value = value;
    }

    public static Result<Identifier, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return GeneralErrors.ValueIsRequired("DepartmentIdentifier");

        if (value.Length < LengthConstants.MIN_DEPARTMENT_ID || value.Length > LengthConstants.MAX_DEPARTMENT_ID)
        {
            return GeneralErrors.ValueIsInvalid("DepartmentIdentifier");
        }

        if (!_latinRegex.IsMatch(value))
            return GeneralErrors.ValueIsInvalid("DepartmentIdentifier");

        return new Identifier(value);
    }
}