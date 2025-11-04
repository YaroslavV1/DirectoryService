using CSharpFunctionalExtensions;
using DirectoryService.Application.Positions;
using DirectoryService.Domain.Positions;
using DirectoryService.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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
        catch (Exception e)
        {
            _logger.LogError(e.Message, "Fail to create position");
            return GeneralErrors.Failure();
        }
    }

    public async Task<Result<bool, Error>> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        try
        {
            bool result = await _dbContext.Positions.AnyAsync(
                p => p.Name.Value == name && p.IsActive,
                cancellationToken);

            return result;
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, "Failed to check if position exists by name: {name}", name);
            return GeneralErrors.Failure();
        }
    }
}