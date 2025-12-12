using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.ValueObjects;
using SharedService;

namespace DirectoryService.Application.Locations;

public interface ILocationsRepository
{
    Task<Result<Guid, Errors>> Create(Location location, CancellationToken cancellationToken = default);

    Task<Result<bool, Errors>> CheckAllLocationsExistByIds(
        IEnumerable<LocationId> requestedIds,
        CancellationToken cancellationToken = default);

    Task<UnitResult<Errors>> DeactivateUnusedLocationsByDepartmentIdAsync(
        DepartmentId departmentId,
        CancellationToken cancellationToken = default);

    Task<UnitResult<Errors>> DeleteInactiveAsync(CancellationToken cancellationToken = default);
}