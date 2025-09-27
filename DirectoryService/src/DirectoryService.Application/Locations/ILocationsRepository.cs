using CSharpFunctionalExtensions;
using DirectoryService.Domain.Modules.LocationEntity;

namespace DirectoryService.Application.Locations;

public interface ILocationsRepository
{
    public Task<Result<Guid>> Create(Location location, CancellationToken cancellationToken = default);
}