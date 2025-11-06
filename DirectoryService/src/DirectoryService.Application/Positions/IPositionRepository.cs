using CSharpFunctionalExtensions;
using DirectoryService.Domain.Positions;
using DirectoryService.Shared;

namespace DirectoryService.Application.Positions;

public interface IPositionRepository
{
    Task<Result<Guid, Error>> Create(Position position, CancellationToken cancellationToken = default);
}