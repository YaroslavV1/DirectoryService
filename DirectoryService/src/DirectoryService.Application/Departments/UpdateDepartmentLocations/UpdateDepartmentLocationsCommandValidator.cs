using DirectoryService.Application.Validation;
using DirectoryService.Shared;
using FluentValidation;

namespace DirectoryService.Application.Departments.UpdateDepartmentLocations;

public class UpdateDepartmentLocationsCommandValidator : AbstractValidator<UpdateDepartmentLocationsCommand>
{
    public UpdateDepartmentLocationsCommandValidator()
    {
        RuleFor(command => command.DepartmentId)
            .NotEmpty().WithError(GeneralErrors.ValueIsRequired("DepartmentId"));

        RuleFor(c => c.Request.LocationIds)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired("LocationIds"))
            .Must(ids => ids.Distinct().Count() == ids.Count())
            .WithError(GeneralErrors.ContainsDuplicates("LocationIds"));
    }
}