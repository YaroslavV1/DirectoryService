using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.ValueObjects;
using DirectoryService.Shared;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Locations.CreateLocation;

public class CreateLocationHandler : ICommandHandler<Result<Guid, Errors>, CreateLocationCommand>
{
    private readonly ILocationsRepository _locationsRepository;
    private readonly ILogger<CreateLocationHandler> _logger;

    public CreateLocationHandler(
        ILocationsRepository locationsRepository,
        ILogger<CreateLocationHandler> logger)
    {
        _locationsRepository = locationsRepository;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(
        CreateLocationCommand command,
        CancellationToken cancellationToken = default)
    {
        var address = Address.Create(
            command.Request.Address.City,
            command.Request.Address.Street,
            command.Request.Address.PostalCode,
            command.Request.Address.House);

        if (address.IsFailure)
        {
            _logger.LogError(address.Error.Message);
            return address.Error.ToErrors();
        }

        var locationName = LocationName.Create(command.Request.Name);

        if (locationName.IsFailure)
        {
            _logger.LogError(locationName.Error.Message);
            return locationName.Error.ToErrors();
        }

        var timeZone = LocationTimeZone.Create(command.Request.TimeZone);

        if (timeZone.IsFailure)
        {
            _logger.LogError(timeZone.Error.Message);
            return timeZone.Error.ToErrors();
        }

        var location = Location.Create(
            LocationId.NewId(),
            locationName.Value,
            address.Value,
            timeZone.Value);

        var locationId = await _locationsRepository.Create(location, cancellationToken);

        if (locationId.IsFailure)
        {
            _logger.LogError("Failed to create location");
            return locationId.Error;
        }

        return locationId;

    }
}