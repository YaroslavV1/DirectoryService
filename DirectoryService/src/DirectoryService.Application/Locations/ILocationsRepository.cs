using CSharpFunctionalExtensions;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.ValueObjects;
using DirectoryService.Shared;

namespace DirectoryService.Application.Locations;

public interface ILocationsRepository
{
    public Task<Result<Guid, Errors>> Create(Location location, CancellationToken cancellationToken = default);

    public Task<Result<bool, Errors>> ExistsByNameAsync(LocationName name, CancellationToken cancellationToken = default);

    public Task<Result<bool, Errors>> ExistsByAddressAsync(
        Address address,
        CancellationToken cancellationToken = default);
}