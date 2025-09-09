using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Domain.Modules.DepartmentEntity.ValueObjects;

public record Identifier
{

    private static readonly Regex _latinRegex = new(@"^[A-Za-z]+$", RegexOptions.Compiled);

    public string Value { get; }

    private Identifier(string value)
    {
        Value = value;
    }

    public static Result<Identifier> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<Identifier>("Identifier is required!");
        if (value.Length < Constants.MIN_DEPARTMENT_ID_LENGTH || value.Length > Constants.MAX_DEPARTMENT_ID_LENGTH)
        {
            return Result.Failure<Identifier>(
                $"Name must be between {Constants.MIN_DEPARTMENT_ID_LENGTH} and {Constants.MAX_DEPARTMENT_ID_LENGTH} length!");
        }

        if (!_latinRegex.IsMatch(value))
            return Result.Failure<Identifier>($"Identifier must contain only latin characters!");

        return new Identifier(value);
    }
}