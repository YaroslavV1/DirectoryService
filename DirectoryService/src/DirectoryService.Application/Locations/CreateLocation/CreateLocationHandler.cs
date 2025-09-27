using DirectoryService.Application.Abstractions;
using DirectoryService.Domain.Modules.LocationEntity;
using DirectoryService.Domain.Modules.LocationEntity.ValueObjects;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Locations.CreateLocation;

public class CreateLocationHandler : ICommandHandler<Guid, CreateLocationCommand>
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

    public async Task<Guid> Handle(
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
            _logger.LogError(address.Error);
            throw new ArgumentException(address.Error);
        }

        var locationName = LocationName.Create(command.Request.Name);

        if (locationName.IsFailure)
        {
            _logger.LogError(locationName.Error);
            throw new ArgumentException(locationName.Error);
        }

        var timeZone = LocationTimeZone.Create(command.Request.TimeZone);

        if (timeZone.IsFailure)
        {
            _logger.LogError(timeZone.Error);
            throw new ArgumentException(timeZone.Error);
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
            throw new ArgumentException("Failed to create location");
        }

        return locationId.Value;

    }
}