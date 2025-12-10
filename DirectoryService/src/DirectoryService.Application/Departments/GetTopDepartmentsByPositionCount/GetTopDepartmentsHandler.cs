using System.Text.Json;
using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions.Queries;
using DirectoryService.Application.Caching;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Departments.GetTopDepartments;
using DirectoryService.Shared;
using Microsoft.Extensions.Caching.Distributed;

namespace DirectoryService.Application.Departments.GetTopDepartmentsByPositionCount;

public class GetTopDepartmentsHandler :
    IQueryHandler<
        Result<GetTopDepartmentsResponse, Errors>,
        GetTopDepartmentsQuery>
{
    private const short TIME_TO_LIVE_MINUTES = 5;
    private readonly IDbConnectionFactory _dbConnection;
    private readonly ICacheService _cacheService;

    public GetTopDepartmentsHandler(
        IDbConnectionFactory dbConnection,
        ICacheService cacheService)
    {
        _dbConnection = dbConnection;
        _cacheService = cacheService;
    }

    public async Task<Result<GetTopDepartmentsResponse, Errors>> Handle(
        GetTopDepartmentsQuery query,
        CancellationToken cancellationToken = default)
    {
        string key = "departments_" + JsonSerializer.Serialize(query);

        var options =
            new DistributedCacheEntryOptions() { SlidingExpiration = TimeSpan.FromMinutes(TIME_TO_LIVE_MINUTES) };

        var topDepartmentsByPositionsCount = await _cacheService.GetOrSetAsync(
            key,
            options,
            async () => await GetTopDepartmentsAsync(query, cancellationToken),
            cancellationToken);

        if (topDepartmentsByPositionsCount is null)
        {
            return Error.NotFound(
                "departments.not_found.top_list",
                "Top departments was not found").ToErrors();
        }

        return topDepartmentsByPositionsCount;
    }

    private async Task<GetTopDepartmentsResponse> GetTopDepartmentsAsync(
        GetTopDepartmentsQuery query,
        CancellationToken cancellationToken)
    {
        var connection = await _dbConnection.CreateConnectionAsync(cancellationToken);

        int topLimit = query.Request.Top is > 0 ? query.Request.Top.Value : 5;

        var topDepartmentsByPositionsCount = await connection.QueryAsync<GetTopDepartmentsByPositionsCountDto>(
            """
            SELECT root.id,
                   root.name,
                   root.created_at,
                   root.updated_at,
                   count(distinct dp.position_id) as total_positions
            FROM departments d
                     JOIN departments root ON root.path = subpath(d.path, 0, 1)
                     LEFT JOIN department_positions dp ON dp.department_id = d.id
            GROUP BY root.id, root.name
            ORDER BY total_positions DESC
            LIMIT @TopLimit
            """,
            new { TopLimit = topLimit });

        return new GetTopDepartmentsResponse(
            topDepartmentsByPositionsCount.ToList(),
            topDepartmentsByPositionsCount.Count());
    }
}