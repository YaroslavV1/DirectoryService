using System.Data;
using System.Text.Json;
using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions.Queries;
using DirectoryService.Application.Caching;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Departments.GetRootDepartmentsTree;
using DirectoryService.Shared;
using Microsoft.Extensions.Caching.Distributed;

namespace DirectoryService.Application.Departments.GetRootDepartmentsTree;

public class GetRootDepartmentsTreeHandler :
    IQueryHandler<
        Result<RootDepartmentTreeResponse, Errors>,
        GetRootDepartmentsTreeQuery>
{
    private const short TIME_TO_LIVE_MINUTES = 5;
    private readonly IDbConnectionFactory _dbConnection;
    private readonly ICacheService _cacheService;

    public GetRootDepartmentsTreeHandler(
        IDbConnectionFactory dbConnection,
        ICacheService cacheService)
    {
        _dbConnection = dbConnection;
        _cacheService = cacheService;
    }

    public async Task<Result<RootDepartmentTreeResponse, Errors>> Handle(
        GetRootDepartmentsTreeQuery query,
        CancellationToken cancellationToken = default)
    {
        string key = "departments_" + JsonSerializer.Serialize(query);

        var options =
            new DistributedCacheEntryOptions() { SlidingExpiration = TimeSpan.FromMinutes(TIME_TO_LIVE_MINUTES) };

        var rootDepartments = await _cacheService.GetOrSetAsync(
            key,
            options,
            async () => await GetRootDepartmentTreeAsync(query, cancellationToken),
            cancellationToken);

        if (rootDepartments is null)
        {
            return Error.NotFound(
                "departments.not_found.root_tree",
                "Root departments tree was not found").ToErrors();
        }

        return rootDepartments;
    }

    private async Task<RootDepartmentTreeResponse> GetRootDepartmentTreeAsync(
        GetRootDepartmentsTreeQuery query,
        CancellationToken cancellationToken)
    {
        var connection = await _dbConnection.CreateConnectionAsync(cancellationToken);

        int page = query.Request.Page <= 0 ? 1 : query.Request.Page!.Value;
        int size = query.Request.PageSize <= 0 ? 20 : query.Request.PageSize!.Value;
        int prefetch = query.Request.Prefetch <= 0 ? 3 : query.Request.Prefetch!.Value;

        var parameters = new DynamicParameters();

        parameters.Add("pageSize", size, DbType.Int32);
        parameters.Add("page", (page - 1) * size, DbType.Int32);
        parameters.Add("prefetch", prefetch, DbType.Int32);

        var departmentsRaw = await connection.QueryAsync<RootDepartmentTreeDto>(
            """
            WITH roots AS (SELECT id,
                                  parent_id,
                                  name,
                                  path,
                                  depth,
                                  is_active,
                                  created_at,
                                  updated_at,
                                  identifier
                           FROM departments d
                           WHERE parent_id is null
                           ORDER BY created_at DESC
                           LIMIT @pageSize offset @page)

            SELECT *, (EXISTS(SELECT 1 from departments where parent_id = r.id OFFSET @prefetch LIMIT 1)) as has_more_children
            from roots r

            UNION ALL

            SELECT c.*,  (EXISTS(SELECT 1 from departments where parent_id = c.id)) as has_more_children
            from roots r
                     CROSS JOIN LATERAL (SELECT id,
                                                parent_id,
                                                name,
                                                path,
                                                depth,
                                                is_active,
                                                created_at,
                                                updated_at,
                                                identifier
                                         from departments d
                                         where d.parent_id = r.id
                                         ORDER BY created_at
                                         LIMIT @prefetch ) c

            """,
            parameters);

        var departmentsDictionary = departmentsRaw.ToDictionary(d => d.Id);
        var departmentsRoot = new List<RootDepartmentTreeDto>();

        foreach (var row in departmentsRaw)
        {
            if (row.ParentId.HasValue && departmentsDictionary.TryGetValue(row.ParentId.Value, out var parent))
            {
                parent.Children.Add(departmentsDictionary[row.Id]);
            }
            else
            {
                departmentsRoot.Add(departmentsDictionary[row.Id]);
            }
        }

        return new RootDepartmentTreeResponse(departmentsRoot);
    }
}