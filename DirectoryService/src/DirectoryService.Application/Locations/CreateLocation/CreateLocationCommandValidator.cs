using DirectoryService.Application.Validation;
using DirectoryService.Domain.Locations.ValueObjects;
using DirectoryService.Shared;
using FluentValidation;

namespace DirectoryService.Application.Locations.CreateLocation;

public class CreateLocationCommandValidator : AbstractValidator<CreateLocationCommand>
{
    public CreateLocationCommandValidator()
    {
        RuleFor(c => c.Request)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired("Request"));

        RuleFor(c => c.Request.Address)
            .MustBeValueObject(a =>
                Address.Create(
                    a.City,
                    a.Street,
                    a.PostalCode,
                    a.House));

        RuleFor(c => c.Request.Name).MustBeValueObject(LocationName.Create);

        RuleFor(c => c.Request.TimeZone).MustBeValueObject(LocationTimeZone.Create);
    }
}