using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.DepartmentEntity.ValueObjects;

public record DepartmentPath
{

    private static readonly Regex _pathRegex = new(
        @"^[A-Za-z0-9]+(-[A-Za-z0-9]+)*(\.[A-Za-z0-9]+(-[A-Za-z0-9]+)*)*$",
        RegexOptions.Compiled);

    public string Value { get; }

    private DepartmentPath(string value)
    {
        Value = value;
    }

    public static Result<DepartmentPath> Create(string value)
    {
        if(!_pathRegex.IsMatch(value))
            return Result.Failure<DepartmentPath>("Invalid path format!");
        return new DepartmentPath(value);
    }
}