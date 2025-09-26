using DirectoryService.Domain.Modules.LocationEntity;

namespace DirectoryService.Application.Locations;

public interface ILocationsRepository
{
    public Task<Guid> Create(Location location, CancellationToken cancellationToken = default);
}