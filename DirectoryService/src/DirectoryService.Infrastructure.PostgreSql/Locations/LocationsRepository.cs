using CSharpFunctionalExtensions;
using DirectoryService.Application.Locations;
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
}