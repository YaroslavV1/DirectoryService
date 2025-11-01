﻿using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.ValueObjects;
using DirectoryService.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Locations.CreateLocation;

public class CreateLocationHandler : ICommandHandler<Result<Guid, Errors>, CreateLocationCommand>
{
    private readonly ILocationsRepository _locationsRepository;
    private readonly IValidator<CreateLocationCommand> _validator;
    private readonly ILogger<CreateLocationHandler> _logger;

    public CreateLocationHandler(
        ILocationsRepository locationsRepository,
        IValidator<CreateLocationCommand> validator,
        ILogger<CreateLocationHandler> logger)
    {
        _locationsRepository = locationsRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(
        CreateLocationCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            return validationResult.ToErrorList();
        }

        var address = Address.Create(
            command.Request.Address.City,
            command.Request.Address.Street,
            command.Request.Address.PostalCode,
            command.Request.Address.House).Value;

        var locationName = LocationName.Create(command.Request.Name).Value;

        var timeZone = LocationTimeZone.Create(command.Request.TimeZone).Value;

        var location = Location.Create(
            LocationId.NewId(),
            locationName,
            address,
            timeZone);

        var locationNameExistsResult = await _locationsRepository.ExistsByNameAsync(locationName, cancellationToken);

        if (!locationNameExistsResult.Value)
            return GeneralErrors.AlreadyExists("Location").ToErrors();

        var locationAddressExistsResult = await _locationsRepository.ExistsByAddressAsync(address, cancellationToken);

        if (!locationAddressExistsResult.Value)
            return GeneralErrors.AlreadyExists("Location").ToErrors();

        var locationId = await _locationsRepository.Create(location, cancellationToken);

        _logger.LogInformation("Successfully created location {location}", locationId.Value);

        return locationId;
    }
}