using CSharpFunctionalExtensions;
using DirectoryService.Domain.Locations;
using DirectoryService.Shared;

namespace DirectoryService.Application.Locations;

public interface ILocationsRepository
{
    public Task<Result<Guid, Errors>> Create(Location location, CancellationToken cancellationToken = default);
}