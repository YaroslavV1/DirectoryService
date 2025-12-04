using System.Text;
using CSharpFunctionalExtensions;
using Dapper;
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

    public async Task<Result<Department, Error>> GetByIdAsync(
        DepartmentId departmentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var department =
                await _dbContext.Departments
                    .Include(d => d.Locations)
                    .FirstOrDefaultAsync(
                        d => d.Id == departmentId && d.IsActive,
                        cancellationToken);

            if (department is null)
                return GeneralErrors.NotFound("Department");

            return department;
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, "Failed to get department by {id}", departmentId.Value);
            return GeneralErrors.Failure();
        }
    }

    public async Task<Result<Department, Error>> GetByIdWithLockAsync(
        DepartmentId departmentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var department = await _dbContext.Departments
                .FromSql(
                    $"SELECT * FROM departments WHERE id = {departmentId.Value} FOR UPDATE")
                .FirstOrDefaultAsync(cancellationToken);

            if (department is null)
                return GeneralErrors.NotFound("Department");

            return department;
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, "Failed to get department by {id}", departmentId.Value);
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

    public async Task<Result<bool, Error>> IsDescendant(
        DepartmentId potentialDescendantId,
        DepartmentId ancestorId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var dbConn = _dbContext.Database.GetDbConnection();

            const string sql = @"
            SELECT EXISTS (
                SELECT 1 
                FROM departments descendant
                JOIN departments ancestor ON ancestor.id = @ancestorId
                WHERE descendant.id = @potentialDescendantId
                  AND descendant.path <@ ancestor.path
                  AND descendant.id != ancestor.id
            )";

            bool isDescendant = await dbConn.QuerySingleAsync<bool>(
                sql,
                new { potentialDescendantId = potentialDescendantId.Value, ancestorId = ancestorId.Value, });

            return isDescendant;
        }
        catch (Exception e)
        {
            _logger.LogError(
                e,
                "Failed to check if department {descendantId} is descendant of {ancestorId}",
                potentialDescendantId, ancestorId);
            return DepartmentsErrors.DatabaseError();
        }
    }

    public async Task<UnitResult<Error>> LockDescendant(
        Department department,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var parentPath = department.Path.Value;

            await _dbContext.Database.ExecuteSqlAsync(
                $"""
                 SELECT id 
                 FROM departments 
                 WHERE path <@ {parentPath}::ltree 
                   AND path != {parentPath}::ltree 
                 FOR UPDATE
                 """,
                cancellationToken);

            return UnitResult.Success<Error>();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to lock descendants for department {id}", department.Id.Value);
            return GeneralErrors.Failure();
        }
    }

    public async Task<UnitResult<Error>> UpdateDescendantsPathAndDepthAsync(
        DepartmentId departmentId,
        DepartmentPath oldPath,
        DepartmentPath newPath,
        int? depthDifference = null,
        bool? onlyActive = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var depthUpdate = depthDifference.HasValue
                ? ", depth = depth + @depthDifference"
                : string.Empty;

            string activeFilter = onlyActive == true
                ? " AND is_active = true"
                : string.Empty;

            var sql = new StringBuilder().Append("""
                                                 UPDATE departments
                                                 SET 
                                                     updated_at = NOW(),
                                                     path = @newPath::ltree || subpath(path, nlevel(@oldPath::ltree))
                                                 """)
                .Append(depthUpdate)
                .Append("""

                        WHERE 
                            path <@ @oldPath::ltree 
                            AND id != @departmentId
                        """)
                .Append(activeFilter)
                .ToString();

            var parameters = new List<NpgsqlParameter>
            {
                new("@newPath", newPath.Value),
                new("@oldPath", oldPath.Value),
                new("@departmentId", departmentId.Value),
            };

            if (depthDifference.HasValue)
            {
                parameters.Add(new NpgsqlParameter("@depthDifference", depthDifference.Value));
            }

            await _dbContext.Database.ExecuteSqlRawAsync(
                sql,
                parameters.ToArray(),
                cancellationToken);

            return UnitResult.Success<Error>();
        }
        catch (Exception ex)
        {
            return Error.Failure(
                "department.update.descendants",
                $"Failed to update descendants: {ex.Message}");
        }
    }
}