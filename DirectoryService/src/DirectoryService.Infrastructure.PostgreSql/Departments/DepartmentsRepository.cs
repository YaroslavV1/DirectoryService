using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Infrastructure.Departments;

public class DepartmentsRepository : IDepartmentsRepository
{
    private readonly DirectoryServiceDbContext _dbContext;
    private readonly ILogger<DepartmentsRepository> _logger;

    public DepartmentsRepository(
        DirectoryServiceDbContext dbContext,
        ILogger<DepartmentsRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<Guid, Error>> Create(
        Department department,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbContext.Departments.AddAsync(department, cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully added to the database with {department}", department.Id.Value);

            return department.Id.Value;
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, "Fail to create department");
            return GeneralErrors.Failure();
        }
    }

    public async Task<Result<Department?, Error>> GetByIdAsync(
        DepartmentId departmentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var department =
                await _dbContext.Departments.FirstOrDefaultAsync(
                    d => d.Id == departmentId,
                    cancellationToken);

            if (department is null)
                return GeneralErrors.NotFound("Department");

            return department;
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, "Failed to get department");
            return GeneralErrors.Failure();
        }
    }

    public async Task<Result<bool, Error>> ExistsWithSameIdentifierAsync(DepartmentId parentId, Identifier identifier,
        CancellationToken cancellationToken = default)
    {
        try
        {
            bool result = await _dbContext.Departments
                .AnyAsync(
                    d => d.Identifier == identifier &&
                         d.ParentId == parentId, cancellationToken);

            if (!result)
            {
                return false;
            }

            _logger.LogError("Department with identifier {identifier} already exists at same level", identifier);
            return GeneralErrors.AlreadyExists("Identifier");
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, "Server Fail to check existing department with  identifier {identifier}", identifier);
            return GeneralErrors.Failure();
        }
    }

    public async Task<Result<bool, Error>> CheckIfAllDepartmentsExistAsync(
        IEnumerable<Guid> departmentIds,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var departmentIdList = departmentIds
                .Select(DepartmentId.Create);

            int checkResult = await _dbContext.Departments
                .Where(d => departmentIdList.Contains(d.Id) && d.IsActive)
                .CountAsync(cancellationToken);

            if (checkResult == departmentIds.Count())
                return true;

            _logger.LogError("Department with ids {departmentIds} does not exist", departmentIds);
            return GeneralErrors.NotFound("DepartmentIds");
        }
        catch (Exception e)
        {
            _logger.LogError(
                e.Message,
                "Failed to check existence of departments by IDs. Requested IDs: {RequestedIds}",
                departmentIds);

            return GeneralErrors.Failure();
        }
    }
}