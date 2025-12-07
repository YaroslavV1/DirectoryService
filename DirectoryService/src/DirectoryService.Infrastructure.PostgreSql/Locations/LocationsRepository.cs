using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Locations;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.ValueObjects;
using DirectoryService.Shared;
using DirectoryService.Shared.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace DirectoryService.Infrastructure.Locations;

public class LocationsRepository : ILocationsRepository
{
    private readonly DirectoryServiceDbContext _dbContext;
    private readonly ILogger<LocationsRepository> _logger;

    public LocationsRepository(
        DirectoryServiceDbContext dbContext,
        ILogger<LocationsRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Create(Location location, CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbContext.Locations.AddAsync(location, cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully added to the database with {location}", location.Id.Value);

            return location.Id.Value;
        }
        catch (DbUpdateException e) when (e.InnerException is PostgresException pgEx)
        {
            if (pgEx is { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: not null } &&
                pgEx.ConstraintName.Contains("name"))
            {
                _logger.LogError(e, "Database update error while creating a new location with {name}",
                    location.Name.Value);
                return LocationsErrors.NameConflict(location.Name.Value).ToErrors();
            }

            if (pgEx is { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: not null } &&
                pgEx.ConstraintName.Contains("address"))
            {
                _logger.LogError(e, "Database update error while creating a new location with {address}",
                    location.Address);
                return LocationsErrors.AddressConflict().ToErrors();
            }

            _logger.LogError(e, "Error updating the database with {location}", location);
            return LocationsErrors.DatabaseError().ToErrors();
        }
        catch (OperationCanceledException oce)
        {
            _logger.LogError(oce, "Operation was cancelled while updating the database with {location}", location);
            return LocationsErrors.OperationCancelled().ToErrors();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected error while updating the database with {location}", location);
            return LocationsErrors.DatabaseError().ToErrors();
        }
    }

    public async Task<Result<bool, Errors>> CheckAllLocationsExistByIds(
        IEnumerable<LocationId> requestedIds,
        CancellationToken cancellationToken = default)
    {
        try
        {
            bool isAllLocationsExist = await _dbContext.Locations
                .Where(l => requestedIds.Contains(l.Id) && l.IsActive)
                .CountAsync(cancellationToken) == requestedIds.Count();

            return isAllLocationsExist;
        }
        catch (Exception e)
        {
            _logger.LogError(
                "Failed to check existence of locations by IDs. Requested IDs: {RequestedIds}",
                requestedIds.Select(x => x.Value));

            return GeneralErrors.Failure().ToErrors();
        }
    }

    public async Task<UnitResult<Errors>> DeactivateUnusedLocationsByDepartmentIdAsync(
        DepartmentId departmentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var connection = _dbContext.Database.GetDbConnection();

            const string sql = """
                               WITH related_locations AS (SELECT l."Id"
                                                          from locations l
                                                                   JOIN department_locations dl ON l."Id" = dl.location_id
                                                              AND dl.department_id = @departmentId),

                                    location_usage AS (SELECT rl."Id",
                                                              count(distinct case
                                                                                 when d.is_active = true AND d.id != @departmentId
                                                                                     then d.id
                                                                  end) as active_dept_count
                                                       FROM related_locations rl
                                                                JOIN department_locations dl ON rl."Id" = dl.location_id
                                                                JOIN departments d ON d.id = dl.department_id
                                                       GROUP BY rl."Id")

                               UPDATE locations
                               SET is_active  = false,
                                   updated_at = NOW()
                               WHERE "Id" IN (SELECT "Id"
                                              FROM location_usage
                                              WHERE active_dept_count = 0)

                               """;

            await connection.ExecuteAsync(sql, new { departmentId = departmentId.Value });

            return UnitResult.Success<Errors>();
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to deactivate unused location by {departmentId}", departmentId);
            return Error.Failure(
                "location.soft-delete.unused",
                $"Failed to soft delete unused locations: {e.Message}").ToErrors();
        }
    }

    public async Task<UnitResult<Errors>> DeleteInactiveAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbContext.Database.ExecuteSqlRawAsync(
                """
                DELETE FROM locations l
                WHERE l.is_active = false
                AND not exists(
                    SELECT 1
                    from department_locations dl
                    WHERE dl.location_id = l."Id"
                )
                """,
                cancellationToken);

            return UnitResult.Success<Errors>();
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to delete inactive location with error: {e}", e.Message);
            return UnitResult.Failure<Errors>(GeneralErrors.Failure());
        }
    }
}