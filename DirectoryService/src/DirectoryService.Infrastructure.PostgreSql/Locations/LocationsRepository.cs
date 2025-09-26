using DirectoryService.Application.Locations;
using DirectoryService.Domain.Modules.LocationEntity;

namespace DirectoryService.Infrastructure.Locations;

public class LocationsRepository: ILocationsRepository
{
    private readonly DirectoryServiceDbContext _dbContext;

    public LocationsRepository(DirectoryServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }


    public async Task<Guid> Create(Location location, CancellationToken cancellationToken = default)
    {
        await _dbContext.Locations.AddAsync(location, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return location.Id.Value;
    }
}