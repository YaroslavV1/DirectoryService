using CSharpFunctionalExtensions;
using DirectoryService.Shared;

namespace DirectoryService.Application.Abstractions.Queries;

public interface IQueryHandler<TResponse, in TQuery>
    where TQuery : IQuery
{
    Task<TResponse> Handle(TQuery query, CancellationToken cancellationToken = default);
}

public interface IQueryHandler<in TQuery>
    where TQuery : IQuery
{
    Task<UnitResult<Errors>> Handle(TQuery query, CancellationToken cancellationToken = default);
}