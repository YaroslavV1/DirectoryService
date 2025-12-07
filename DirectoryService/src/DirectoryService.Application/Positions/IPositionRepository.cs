using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Positions;
using DirectoryService.Shared;

namespace DirectoryService.Application.Positions;

public interface IPositionRepository
{
    Task<Result<Guid, Error>> Create(Position position, CancellationToken cancellationToken = default);

    public Task<UnitResult<Errors>> DeactivateUnusedPositionsByDepartmentIdAsync(
        DepartmentId departmentId,
        CancellationToken cancellationToken = default);

    Task<UnitResult<Errors>> DeleteInactiveAsync(CancellationToken cancellationToken = default);
}