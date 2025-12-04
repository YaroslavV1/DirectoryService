using DirectoryService.Application.Validation;
using DirectoryService.Shared;
using FluentValidation;

namespace DirectoryService.Application.Departments.DeleteDepartment;

public class SoftDeleteDepartmentCommandValidator : AbstractValidator<SoftDeleteDepartmentCommand>
{
    public SoftDeleteDepartmentCommandValidator()
    {
        RuleFor(query => query.DepartmentId)
            .NotEmpty().WithError(GeneralErrors.ValueIsRequired("DepartmentId"));
    }
}