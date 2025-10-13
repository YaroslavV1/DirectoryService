using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using DirectoryService.Shared;

namespace DirectoryService.Domain.Modules.DepartmentEntity.ValueObjects;

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

    public static Result<DepartmentPath, Error> Create(string value)
    {
        if(!_pathRegex.IsMatch(value))
            return GeneralErrors.ValueIsInvalid("DepartmentPath");
        return new DepartmentPath(value);
    }
}