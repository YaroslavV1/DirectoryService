using DirectoryService.Application.Locations.CreateLocation;

namespace DirectoryService.Application.Abstractions;

public interface ICommandHandler<TResponse, in TCommand>
    where TCommand : ICommand
{
    Task<TResponse> Handle(TCommand command, CancellationToken cancellationToken = default);
}

public interface ICommandHandler<in TCommand>
{
    Task Handle(TCommand command, CancellationToken cancellationToken = default);
}