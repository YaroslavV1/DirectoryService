using CSharpFunctionalExtensions;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Positions;
using DirectoryService.Domain.Departments.ValueObjects;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedService;
using SharedService.Core.Abstractions.Commands;
using SharedService.Core.Caching;
using SharedService.Core.Database;
using SharedService.Core.Validation;

namespace DirectoryService.Application.Departments.DeleteDepartment;

public class SoftDeleteDepartmentHandler : ICommandHandler<Result<Guid, Errors>, SoftDeleteDepartmentCommand>
{
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ILocationsRepository _locationsRepository;
    private readonly IPositionRepository _positionsRepository;
    private readonly IValidator<SoftDeleteDepartmentCommand> _validator;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<SoftDeleteDepartmentHandler> _logger;
    private readonly ICacheService _cacheService;

    private const string KEY = "departments_";

    public SoftDeleteDepartmentHandler(
        IDepartmentsRepository departmentsRepository,
        ILocationsRepository locationsRepository,
        IPositionRepository positionsRepository,
        ICacheService cacheService,
        IValidator<SoftDeleteDepartmentCommand> validator,
        ITransactionManager transactionManager,
        ILogger<SoftDeleteDepartmentHandler> logger)
    {
        _departmentsRepository = departmentsRepository;
        _locationsRepository = locationsRepository;
        _positionsRepository = positionsRepository;
        _validator = validator;
        _transactionManager = transactionManager;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<Result<Guid, Errors>> Handle(
        SoftDeleteDepartmentCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            _logger.LogError("Department delete was unsuccessful by validation");
            return validationResult.ToErrorList();
        }

        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);

        if (transactionScopeResult.IsFailure)
        {
            _logger.LogError("Failed Transaction Begin");
            return transactionScopeResult.Error.ToErrors();
        }

        using var transactionScope = transactionScopeResult.Value;

        var departmentId = DepartmentId.Create(command.DepartmentId);

        var departmentResult = await _departmentsRepository.GetByIdWithLockAsync(departmentId, cancellationToken);

        if (departmentResult.IsFailure || !departmentResult.Value.IsActive)
        {
            transactionScope.Rollback();

            _logger.LogError("Failed to get department by {id}", command.DepartmentId);
            return departmentResult.Error.ToErrors();
        }

        var department = departmentResult.Value;

        var oldPath = department.Path;

        var departmentSoftDeleteResult = department.SoftDelete();

        if (departmentSoftDeleteResult.IsFailure)
        {
            transactionScope.Rollback();

            _logger.LogError("Failed soft delete by domain Error");
            return departmentSoftDeleteResult.Error.ToErrors();
        }

        var saveChangesResult = await _transactionManager.SaveChangesAsync(cancellationToken);

        if (saveChangesResult.IsFailure)
        {
            transactionScope.Rollback();
            _logger.LogError("Failed to save changes");
            return saveChangesResult.Error.ToErrors();
        }

        var lockDescendantResult = await _departmentsRepository.LockDescendant(
            department, cancellationToken);

        if (lockDescendantResult.IsFailure)
        {
            transactionScope.Rollback();

            _logger.LogError("Failed to lock descendant");
            return lockDescendantResult.Error.ToErrors();
        }

        var deactivateUnusedLocationsResult =
            await _locationsRepository.DeactivateUnusedLocationsByDepartmentIdAsync(departmentId, cancellationToken);

        if (deactivateUnusedLocationsResult.IsFailure)
        {
            transactionScope.Rollback();

            _logger.LogError("Failed to deactivate unused locations");
            return deactivateUnusedLocationsResult.Error;
        }

        var deactivateUnusedPositionsResult =
            await _positionsRepository.DeactivateUnusedPositionsByDepartmentIdAsync(departmentId, cancellationToken);

        if (deactivateUnusedPositionsResult.IsFailure)
        {
            transactionScope.Rollback();

            _logger.LogError("Failed to deactivate unused positions");
            return deactivateUnusedPositionsResult.Error;
        }

        var updateDescendantsResult = await _departmentsRepository.UpdateDescendantsPathAndDepthAsync(
            departmentId,
            oldPath,
            department.Path,
            cancellationToken: cancellationToken);

        if (updateDescendantsResult.IsFailure)
        {
            transactionScope.Rollback();
            _logger.LogError("Failed to update descendants Path and Depth");
            return updateDescendantsResult.Error.ToErrors();
        }

        var commitResult = transactionScope.Commit();

        if (commitResult.IsFailure)
        {
            transactionScope.Rollback();
            _logger.LogError("Failed to commit changes");
            return commitResult.Error.ToErrors();
        }

        await _cacheService.RemoveByPrefixAsync(KEY, cancellationToken);

        _logger.LogInformation(
            "Successfully soft delete department {departmentId}",
            command.DepartmentId);

        return department.Id.Value;
    }
}