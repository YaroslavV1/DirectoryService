using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions.Queries;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Departments.GetTopDepartments;
using DirectoryService.Shared;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.GetTopDepartmentsByPositionCount;

public class GetTopDepartmentsHandler :
    IQueryHandler<
        Result<GetTopDepartmentsResponse, Errors>,
        GetTopDepartmentsQuery>
{
    private readonly IDbConnectionFactory _dbConnection;

    public GetTopDepartmentsHandler(
        IDbConnectionFactory dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task<Result<GetTopDepartmentsResponse, Errors>> Handle(
        GetTopDepartmentsQuery query,
        CancellationToken cancellationToken = default)
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