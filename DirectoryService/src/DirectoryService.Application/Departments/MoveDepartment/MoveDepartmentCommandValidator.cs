using FluentValidation;
using SharedService;
using SharedService.Core.Validation;

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