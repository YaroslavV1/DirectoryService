using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Positions;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Positions;
using DirectoryService.Shared;
using DirectoryService.Shared.Positions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace DirectoryService.Infrastructure.Positions;

public class PositionsRepository : IPositionRepository
{
    private readonly DirectoryServiceDbContext _dbContext;
    private readonly ILogger<PositionsRepository> _logger;

    public PositionsRepository(
        DirectoryServiceDbContext dbContext,
        ILogger<PositionsRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<Guid, Error>> Create(Position position, CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbContext.Positions.AddAsync(position, cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully added to the database with {positionId}", position.Id.Value);

            return position.Id.Value;
        }
        catch (DbUpdateException e) when (e.InnerException is PostgresException pgEx)
        {
            if (pgEx is { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: not null } &&
                pgEx.ConstraintName.Contains("name"))
            {
                _logger.LogError(e, "Database update error while creating a new position with name {name}",
                    position.Name.Value);
                return PositionsErrors.NameConflict(position.Name.Value);
            }

            _logger.LogError(e, "Error updating the database with {position}", position.Id.Value);
            return PositionsErrors.DatabaseError();
        }
        catch (OperationCanceledException oce)
        {
            _logger.LogError(oce, "Operation was cancelled while updating the database with {position}",
                position.Id.Value);
            return PositionsErrors.OperationCancelled();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected error while updating the database with {position}", position.Id.Value);
            return PositionsErrors.DatabaseError();
        }
    }

    public async Task<UnitResult<Errors>> DeactivateUnusedPositionsByDepartmentIdAsync(
        DepartmentId departmentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var connection = _dbContext.Database.GetDbConnection();

            const string sql = """
                               WITH related_positions AS (SELECT p."Id"
                                                          from positions p
                                                                   JOIN department_positions dp ON p."Id" = dp.position_id
                                                              AND dp.department_id = @departmentId),
                               
                                    position_usage AS (SELECT rp."Id",
                                                              count(distinct case
                                                                                 when d.is_active = true AND d.id != @departmentId
                                                                                     then d.id
                                                                  end) as active_dept_count
                                                       FROM related_positions rp
                                                                JOIN department_positions dp ON rp."Id" = dp.position_id
                                                                JOIN departments d ON d.id = dp.department_id
                                                       GROUP BY rp."Id")
                               
                               UPDATE positions
                               SET is_active  = false,
                                   updated_at = NOW()
                               WHERE "Id" IN (SELECT "Id"
                                              FROM position_usage
                                              WHERE active_dept_count = 0)
                               

                               """;

            await connection.ExecuteAsync(sql, new { departmentId = departmentId.Value });

            return UnitResult.Success<Errors>();
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to deactivate unused positions by {departmentId}", departmentId);
            return Error.Failure(
                "positions.soft-delete.unused",
                $"Failed to soft delete unused locations: {e.Message}").ToErrors();
        }
    }

    public async Task<UnitResult<Errors>> DeleteInactiveAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbContext.Database.ExecuteSqlRawAsync(
                """
                DELETE FROM positions p
                WHERE p.is_active = false
                AND not exists(
                    SELECT 1
                    from department_positions dp
                    WHERE dp.position_id = p."Id"
                )
                """,
                cancellationToken);

            return UnitResult.Success<Errors>();
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to delete inactive positions with error: {e}", e.Message);
            return UnitResult.Failure<Errors>(GeneralErrors.Failure());
        }
    }
}