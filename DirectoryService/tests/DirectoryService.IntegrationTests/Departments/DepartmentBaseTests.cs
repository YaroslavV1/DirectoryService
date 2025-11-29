using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments.GetTopDepartmentsByPositionCount;
using DirectoryService.Application.Locations.GetLocations;
using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Locations.GetLocations;
using DirectoryService.Shared;

namespace DirectoryService.IntegrationTests.Departments;

public class DepartmentBaseTests : DirectoryBaseTests
{
    protected DepartmentBaseTests(DirectoryTestWebFactory factory)
        : base(factory)
    {
    }

    protected async Task<Result<GetTopDepartmentsResponse, Errors>> GetTopDepartments(
        int? topLimit)
    {
        var result = await ExecuteHandler(async (GetTopDepartmentsHandler sut) =>
        {
            var query = new GetTopDepartmentsQuery(new GetTopDepartmentsRequest(topLimit));

            return await sut.Handle(query, CancellationToken.None);
        });

        return result;
    }
}