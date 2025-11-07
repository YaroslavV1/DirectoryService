using DirectoryService.Application.Validation;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Shared;
using FluentValidation;

namespace DirectoryService.Application.Departments.CreateDepartment;

public class CreateDepartmentCommandValidator : AbstractValidator<CreateDepartmentCommand>
{
    public CreateDepartmentCommandValidator()
    {
        RuleFor(c => c.Request.Name)
            .MustBeValueObject(DepartmentName.Create);

        RuleFor(c => c.Request.Identifier)
            .MustBeValueObject(Identifier.Create);

        RuleFor(c => c.Request.LocationIds)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired("LocationIds"))
            .Must(ids => ids.Distinct().Count() == ids.Count())
            .WithError(GeneralErrors.ContainsDuplicates("LocationIds"));
    }
}