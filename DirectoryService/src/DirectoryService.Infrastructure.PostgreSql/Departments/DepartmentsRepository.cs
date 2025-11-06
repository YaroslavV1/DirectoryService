using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Shared;
using DirectoryService.Shared.Departments;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace DirectoryService.Infrastructure.Departments;

public class DepartmentsRepository : IDepartmentsRepository
{
    private readonly DirectoryServiceDbContext _dbContext;
    private readonly ILogger<DepartmentsRepository> _logger;

    public DepartmentsRepository(
        DirectoryServiceDbContext dbContext,
        ILogger<DepartmentsRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<Guid, Error>> Create(
        Department department,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbContext.Departments.AddAsync(department, cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully added to the database with {department}", department.Id.Value);

            return department.Id.Value;
        }
        catch (DbUpdateException e) when (e.InnerException is PostgresException pgEx)
        {
            if (pgEx is { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: not null } &&
                pgEx.ConstraintName.Contains("parent_identifier"))
            {
                _logger.LogError(e, "Database update error while creating a new department"
                                    + " with identifier {identifier} under parent {parentId}",
                    department.Identifier.Value, department.ParentId?.Value);
                return DepartmentsErrors.IdentifierConflict(department.Identifier.Value);
            }

            if (pgEx is { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: not null } &&
                pgEx.ConstraintName.Contains("root_identifier"))
            {
                _logger.LogError(e, "Database update error while creating"
                                    + " a root department with identifier {identifier}",
                    department.Identifier.Value);
                return DepartmentsErrors.RootIdentifierConflict(department.Identifier.Value);
            }

            if (pgEx is { SqlState: PostgresErrorCodes.ForeignKeyViolation, ConstraintName: not null })
            {
                _logger.LogError(e, "Database foreign key violation while creating department with parent {parentId}",
                    department.ParentId?.Value);
                return DepartmentsErrors.ParentNotFound(department.ParentId?.Value);
            }

            _logger.LogError(e, "Error updating the database with {department}", department.Id.Value);
            return DepartmentsErrors.DatabaseError();
        }
        catch (OperationCanceledException oce)
        {
            _logger.LogError(oce, "Operation was cancelled while updating the database with {department}",
                department.Id.Value);
            return DepartmentsErrors.OperationCancelled();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected error while updating the database with {department}",
                department.Id.Value);
            return DepartmentsErrors.DatabaseError();
        }
    }

    public async Task<Result<Department?, Error>> GetByIdAsync(
        DepartmentId departmentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var department =
                await _dbContext.Departments.FirstOrDefaultAsync(
                    d => d.Id == departmentId,
                    cancellationToken);

            if (department is null)
                return GeneralErrors.NotFound("Department");

            return department;
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, "Failed to get department");
            return GeneralErrors.Failure();
        }
    }

    public async Task<Result<bool, Error>> CheckIfAllDepartmentsExistAsync(
        IEnumerable<Guid> departmentIds,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var departmentIdList = departmentIds
                .Select(DepartmentId.Create);

            int checkResult = await _dbContext.Departments
                .Where(d => departmentIdList.Contains(d.Id) && d.IsActive)
                .CountAsync(cancellationToken);

            if (checkResult == departmentIds.Count())
                return true;

            _logger.LogError("Department with ids {departmentIds} does not exist", departmentIds);
            return GeneralErrors.NotFound("DepartmentIds");
        }
        catch (Exception e)
        {
            _logger.LogError(
                e.Message,
                "Failed to check existence of departments by IDs. Requested IDs: {RequestedIds}",
                departmentIds);

            return GeneralErrors.Failure();
        }
    }
}