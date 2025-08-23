using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.DepartmentEntity.ValueObjects;

public record Identifier
{

    private const short MIN_LENGTH = 3;
    private const short MAX_LENGTH = 150;

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
        if (value.Length < MIN_LENGTH || value.Length > MAX_LENGTH)
            return Result.Failure<Identifier>($"Name must be between {MIN_LENGTH} and {MAX_LENGTH} length!");
        if (!_latinRegex.IsMatch(value))
            return Result.Failure<Identifier>($"Identifier must contain only latin characters!");

        return new Identifier(value);
    }
}