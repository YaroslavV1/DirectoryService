using CSharpFunctionalExtensions;
using DirectoryService.Domain.Modules.LocationEntity;
using DirectoryService.Shared;

namespace DirectoryService.Application.Locations;

public interface ILocationsRepository
{
    public Task<Result<Guid, Errors>> Create(Location location, CancellationToken cancellationToken = default);
}