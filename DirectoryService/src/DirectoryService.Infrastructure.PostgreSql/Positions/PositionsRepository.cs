using CSharpFunctionalExtensions;
using DirectoryService.Application.Positions;
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
}