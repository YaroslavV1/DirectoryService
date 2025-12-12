using FluentValidation;
using SharedService;
using SharedService.Core.Validation;

namespace DirectoryService.Application.Departments.DeleteDepartment;

public class SoftDeleteDepartmentCommandValidator : AbstractValidator<SoftDeleteDepartmentCommand>
{
    public SoftDeleteDepartmentCommandValidator()
    {
        RuleFor(query => query.DepartmentId)
            .NotEmpty().WithError(GeneralErrors.ValueIsRequired("DepartmentId"));
    }
}