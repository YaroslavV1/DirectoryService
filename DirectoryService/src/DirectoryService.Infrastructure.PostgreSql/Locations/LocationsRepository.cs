using CSharpFunctionalExtensions;
using DirectoryService.Application.Locations;
using DirectoryService.Domain.Modules.LocationEntity;
using DirectoryService.Shared;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Infrastructure.Locations;

public class LocationsRepository: ILocationsRepository
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

            return location.Id.Value;
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, "Fail to create location");
            return GeneralErrors.Failure().ToErrors();
        }
    }
}