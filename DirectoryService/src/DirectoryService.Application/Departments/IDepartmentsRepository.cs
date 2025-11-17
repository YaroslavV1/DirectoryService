using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Shared;

namespace DirectoryService.Application.Departments;

public interface IDepartmentsRepository
{
    Task<Result<Guid, Error>> Create(Department department, CancellationToken cancellationToken = default);

    Task<Result<Department, Error>> GetByIdAsync(
        DepartmentId departmentId,
        CancellationToken cancellationToken = default);

    Task<Result<Department, Error>> GetByIdWithLockAsync(
        DepartmentId departmentId,
        CancellationToken cancellationToken = default);

    Task<Result<bool, Error>> CheckIfAllDepartmentsExistAsync(
        IEnumerable<Guid> departmentIds,
        CancellationToken cancellationToken = default);

    Task<Result<bool, Error>> IsDescendant(
        DepartmentId potentialDescendantId,
        DepartmentId ancestorId,
        CancellationToken cancellationToken = default);

    Task<UnitResult<Error>> LockDescendant(
        Department department,
        CancellationToken cancellationToken = default);

    Task<UnitResult<Error>> UpdateDescendantsPathAndDepthAsync(
        DepartmentId departmentId,
        DepartmentPath oldPath,
        DepartmentPath newPath,
        int depthDifference,
        CancellationToken cancellationToken = default);
}