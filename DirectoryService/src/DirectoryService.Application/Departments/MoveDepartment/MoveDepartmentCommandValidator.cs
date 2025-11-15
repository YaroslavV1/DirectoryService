using DirectoryService.Application.Validation;
using DirectoryService.Shared;
using FluentValidation;

namespace DirectoryService.Application.Departments.MoveDepartment;

public class MoveDepartmentCommandValidator: AbstractValidator<MoveDepartmentCommand>
{
    public MoveDepartmentCommandValidator()
    {
        RuleFor(m => m.DepartmentId)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired("DepartmentId"));
    }
}