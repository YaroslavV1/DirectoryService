using System.Data;
using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions.Queries;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Departments.GetDepartmentChildren;
using DirectoryService.Shared;

namespace DirectoryService.Application.Departments.GetDepartmentChildren;

public class GetDepartmentChildrenHandler : IQueryHandler<
    Result<GetDepartmentChildrenResponse, Errors>,
    GetDepartmentChildrenQuery>
{
    private readonly IDbConnectionFactory _dbConnection;

    public GetDepartmentChildrenHandler(
        IDbConnectionFactory dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task<Result<GetDepartmentChildrenResponse, Errors>> Handle(
        GetDepartmentChildrenQuery query,
        CancellationToken cancellationToken = default)
    {
        var connection = await _dbConnection.CreateConnectionAsync(cancellationToken);

        int page = query.Request.Page is null or <= 0 ? 1 : query.Request.Page.Value;
        int size = query.Request.PageSize is null or <= 0 ? 20 : query.Request.PageSize.Value;

        var parameters = new DynamicParameters();
        parameters.Add("parentId", query.ParentId, DbType.Guid);
        parameters.Add("pageSize", size, DbType.Int32);
        parameters.Add("offset", (page - 1) * size, DbType.Int32);

        var children = await connection.QueryAsync<DepartmentChildrenDto>(
            """
            SELECT 
                d.id,
                d.parent_id,
                d.name,
                d.identifier,
                d.path,
                d.depth,
                d.is_active,
                d.created_at,
                d.updated_at,
                (EXISTS(SELECT 1 FROM departments WHERE parent_id = d.id LIMIT 1)) as has_more_children
            FROM departments d
            WHERE d.parent_id = @parentId
            ORDER BY d.created_at DESC
            LIMIT @pageSize OFFSET @offset
            """,
            parameters);

        return new GetDepartmentChildrenResponse(children.ToList());
    }
}