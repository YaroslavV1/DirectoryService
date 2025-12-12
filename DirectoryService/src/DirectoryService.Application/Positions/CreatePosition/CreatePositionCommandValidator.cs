using DirectoryService.Domain.Positions.ValueObjects;
using DirectoryService.Domain.Shared;
using FluentValidation;
using SharedService;
using SharedService.Core.Validation;

namespace DirectoryService.Application.Positions.CreatePosition;

public class CreatePositionCommandValidator : AbstractValidator<CreatePositionCommand>
{
    public CreatePositionCommandValidator()
    {
        RuleFor(c => c.Request.Name).MustBeValueObject(PositionName.Create);

        RuleFor(c => c.Request.Description)
            .MaximumLength(LengthConstants.MAX_POSITION_DESCRIPTION)
            .WithError(GeneralErrors.ValueIsInvalid("Description"));

        RuleFor(c => c.Request.DepartmentsIds)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired("DepartmentIds"))
            .Must(ids => ids.Distinct().Count() == ids.Count())
            .WithError(GeneralErrors.ContainsDuplicates("DepartmentIds"));
    }
}