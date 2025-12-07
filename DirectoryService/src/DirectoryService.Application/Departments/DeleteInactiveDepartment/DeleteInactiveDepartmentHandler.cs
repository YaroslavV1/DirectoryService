using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Positions;
using DirectoryService.Shared;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.DeleteInactiveDepartment;

public class DeleteInactiveDepartmentHandler
{
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ILocationsRepository _locationsRepository;
    private readonly IPositionRepository _positionsRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<DeleteInactiveDepartmentHandler> _logger;

    public DeleteInactiveDepartmentHandler(
        IDepartmentsRepository departmentsRepository,
        ILocationsRepository locationsRepository,
        IPositionRepository positionsRepository,
        ITransactionManager transactionManager,
        ILogger<DeleteInactiveDepartmentHandler> logger)
    {
        _departmentsRepository = departmentsRepository;
        _locationsRepository = locationsRepository;
        _positionsRepository = positionsRepository;
        _transactionManager = transactionManager;
        _logger = logger;
    }

    public async Task<UnitResult<Errors>> Handler(CancellationToken cancellationToken = default)
    {
        (_, bool isFailure, ITransactionScope? transactionScope, Error? error) =
            await _transactionManager.BeginTransactionAsync(cancellationToken);

        if (isFailure)
        {
            _logger.LogError("Begin transaction was failed!");
            return error.ToErrors();
        }

        var dateByMonth = DateTime.UtcNow.AddMonths(-1);

        var inactiveDepartmentsResult = await _departmentsRepository
            .GetInactiveByDate(dateByMonth, cancellationToken);

        if (inactiveDepartmentsResult.IsFailure)
        {
            transactionScope.Rollback();

            _logger.LogError("Failed to get inactive departments!");
            return inactiveDepartmentsResult.Error;
        }

        var inactiveDepartments = inactiveDepartmentsResult.Value;

        var departmentIds = inactiveDepartments.Select(d => d.ParentId?.Value);

        var parentOfInactiveDepartmentsResult =
            await _departmentsRepository.GetListByIds(departmentIds, cancellationToken);

        if (parentOfInactiveDepartmentsResult.IsFailure)
        {
            transactionScope.Rollback();

            _logger.LogError("Failed to get parent inactive departments!");
            return parentOfInactiveDepartmentsResult.Error;
        }

        var parentOfInactiveDepartments = parentOfInactiveDepartmentsResult.Value;

        foreach (var inactiveDepartment in inactiveDepartments)
        {
            var children = inactiveDepartment.ChildrenDepartments
                .Where(cd => cd.ParentId == inactiveDepartment.Id)
                .ToList();

            foreach (var child in children)
            {
                var newParent = parentOfInactiveDepartments
                    .FirstOrDefault(p => p.Id == inactiveDepartment.ParentId);

                var updateChildParentResult = child.UpdateParent(newParent);
                if (updateChildParentResult.IsFailure)
                {
                    transactionScope.Rollback();

                    _logger.LogError(
                        "Failed to update child {childId} {childName} by new parent {parentId} {ParentName}!",
                        child.Id.Value, child.Name.Value, newParent.Id.Value, newParent.Name.Value);

                    return updateChildParentResult.Error.ToErrors();
                }
            }
        }

        var saveChangesResult = await _transactionManager.SaveChangesAsync(cancellationToken);

        if (saveChangesResult.IsFailure)
        {
            transactionScope.Rollback();

            _logger.LogError("Failed to save changes after change parent!");
            return saveChangesResult.Error.ToErrors();
        }

        var deleteDepartmentsResult = await _departmentsRepository.DeleteInactiveByDate(dateByMonth, cancellationToken);

        if (deleteDepartmentsResult.IsFailure)
        {
            transactionScope.Rollback();

            _logger.LogError("Failed to delete Inactive departments!");
            return deleteDepartmentsResult.Error;
        }

        var deleteLocationsResult = await _locationsRepository.DeleteInactiveAsync(cancellationToken);

        if (deleteLocationsResult.IsFailure)
        {
            transactionScope.Rollback();

            _logger.LogError("Failed to delete inactive locations!");
            return deleteLocationsResult.Error;
        }

        var deletePositionsResult = await _positionsRepository.DeleteInactiveAsync(cancellationToken);

        if (deletePositionsResult.IsFailure)
        {
            transactionScope.Rollback();

            _logger.LogError("Failed to delete inactive positions!");
            return deletePositionsResult.Error;
        }

        var saveChangesSecondResult = await _transactionManager.SaveChangesAsync(cancellationToken);

        if (saveChangesSecondResult.IsFailure)
        {
            transactionScope.Rollback();

            _logger.LogError("Failed to save changes second!");
            return saveChangesSecondResult.Error.ToErrors();
        }

        var commitResult = transactionScope.Commit();

        if (commitResult.IsFailure)
        {
            transactionScope.Rollback();

            _logger.LogError("Failed to commit transaction!");
            return commitResult.Error.ToErrors();
        }

        _logger.LogInformation("Remove inactive departments with inactive relationships was successful!");
        return UnitResult.Success<Errors>();
    }
}