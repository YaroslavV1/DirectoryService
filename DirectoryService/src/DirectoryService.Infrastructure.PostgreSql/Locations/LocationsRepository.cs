using CSharpFunctionalExtensions;
using DirectoryService.Application.Locations;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.ValueObjects;
using DirectoryService.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Infrastructure.Locations;

public class LocationsRepository : ILocationsRepository
{
    private readonly DirectoryServiceDbContext _dbContext;
    private readonly ILogger<LocationsRepository> _logger;

    public LocationsRepository(
        DirectoryServiceDbContext dbContext,
        ILogger<LocationsRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Create(Location location, CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbContext.Locations.AddAsync(location, cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully added to the database with {location}", location.Id.Value);

            return location.Id.Value;
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, "Fail to create location");
            return GeneralErrors.Failure().ToErrors();
        }
    }

    public async Task<Result<bool, Errors>> ExistsByNameAsync(
        LocationName locationName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            bool result = await _dbContext.Locations.AnyAsync(
                location => location.Name.Value == locationName.Value,
                cancellationToken);

            return result;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to check if location exists by name: {LocationName}", locationName.Value);
            return GeneralErrors.Failure().ToErrors();
        }
    }

    public async Task<Result<bool, Errors>> ExistsByAddressAsync(
        Address address,
        CancellationToken cancellationToken = default)
    {
        try
        {
            bool result = await _dbContext.Locations.AnyAsync(
                l =>
                    l.Address.City == address.City &&
                    l.Address.Street == address.Street &&
                    l.Address.House == address.House &&
                    l.Address.PostalCode == address.PostalCode,
                cancellationToken);

            return result;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to check if location exists by address: {Address}", address);
            return GeneralErrors.Failure().ToErrors();
        }
    }
}