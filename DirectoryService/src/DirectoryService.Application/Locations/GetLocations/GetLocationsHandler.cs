using System.Data;
using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions.Queries;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Locations.GetLocations;
using DirectoryService.Shared;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Locations.GetLocations;

public class GetLocationsHandler : IQueryHandler<Result<GetLocationsDto, Errors>, GetLocationsQuery>
{
    private readonly IDbConnectionFactory _dbConnection;

    public GetLocationsHandler(
        IDbConnectionFactory dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task<Result<GetLocationsDto, Errors>> Handle(
        GetLocationsQuery query,
        CancellationToken cancellationToken = default)
    {
        var connection = await _dbConnection.CreateConnectionAsync(cancellationToken);

        (string filterSql, DynamicParameters parameters) = BuildFilter(query);

        const string baseSql = """
                                   SELECT l."Id",
                                          l.name,
                                          l.city,
                                          l.street,
                                          l.house,
                                          l.postal_code,
                                          l.time_zone,
                                          l.is_active,
                                          l.created_at,
                                          l.updated_at,
                                          COUNT(*) OVER() AS total_count
                                   FROM locations l
                               """;

        string finalSql = $"{baseSql} {filterSql}";

        long? totalCount = null;

        // запрос
        var locations = await connection.QueryAsync<GetLocationDto, long, GetLocationDto>(
            finalSql,
            map: (loc, count) =>
            {
                totalCount ??= count;
                return loc;
            },
            splitOn: "total_count",
            param: parameters);

        return new GetLocationsDto(locations.ToList(), totalCount ?? 0);
    }

    private (string Sql, DynamicParameters Params) BuildFilter(GetLocationsQuery query)
    {
        var conditions = new List<string>();
        var parameters = new DynamicParameters();

        string @join = string.Empty;

        if (query.Request.DepartmentIds != null && query.Request.DepartmentIds.Any())
        {
            conditions.Add("dl.department_id = ANY(@departmentIds)");
            parameters.Add("departmentIds", query.Request.DepartmentIds.Distinct().ToList());
            @join = "JOIN department_locations dl ON dl.location_id = l.\"Id\"";
        }

        if (!string.IsNullOrWhiteSpace(query.Request.Search))
        {
            conditions.Add("l.name ILIKE @search");
            parameters.Add("search", $"%{query.Request.Search}%");
        }

        if (query.Request.IsActive.HasValue)
        {
            conditions.Add("l.is_active = @isActive");
            parameters.Add("isActive", query.Request.IsActive.Value);
        }

        string @where = conditions.Count > 0
            ? $"WHERE {string.Join(" AND ", conditions)}"
            : string.Empty;

        string orderDirection = query.Request.SortDirection?.ToLower() == "asc"
            ? "ASC"
            : "DESC";

        string sortByField = query.Request.SortBy?.ToLower() switch
        {
            "name" => "l.name",
            "date" => "l.created_at",
            _ => "l.name",
        };

        string order = $"ORDER BY {sortByField} {orderDirection}";

        parameters.Add("pageSize", query.Request.PageSize, DbType.Int32);
        parameters.Add("page", (query.Request.Page - 1) * query.Request.PageSize, DbType.Int32);

        string sql = $"""
                          {@join}
                          {@where}
                          {order}
                          LIMIT @pageSize OFFSET @page
                      """;

        return (sql, parameters);
    }
}