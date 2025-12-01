using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments.GetDepartmentChildren;
using DirectoryService.Application.Departments.GetRootDepartmentsTree;
using DirectoryService.Application.Departments.GetTopDepartmentsByPositionCount;
using DirectoryService.Contracts.Departments.GetDepartmentChildren;
using DirectoryService.Contracts.Departments.GetRootDepartmentsTree;
using DirectoryService.Contracts.Departments.GetTopDepartments;
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

    protected async Task<Result<RootDepartmentTreeResponse, Errors>> GetRootDepartmentsTree(
        GetRootDepartmentsTreeRequest request)
    {
        var result = await ExecuteHandler(async (GetRootDepartmentsTreeHandler sut) =>
        {
            var query = new GetRootDepartmentsTreeQuery(request);

            return await sut.Handle(query, CancellationToken.None);
        });

        return result;
    }

    protected async Task<Result<GetDepartmentChildrenResponse, Errors>> GetDepartmentChildren(
        Guid parentId,
        GetDepartmentChildrenRequest request)
    {
        var result = await ExecuteHandler(async (GetDepartmentChildrenHandler sut) =>
        {
            var query = new GetDepartmentChildrenQuery(parentId, request);

            return await sut.Handle(query, CancellationToken.None);
        });

        return result;
    }
}