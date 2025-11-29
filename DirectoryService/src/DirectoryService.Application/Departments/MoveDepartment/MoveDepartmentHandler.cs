using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions.Commands;
using DirectoryService.Application.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Shared;
using DirectoryService.Shared.Departments;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.MoveDepartment;

public class MoveDepartmentHandler : ICommandHandler<Result<Guid, Errors>, MoveDepartmentCommand>
{
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly IValidator<MoveDepartmentCommand> _validator;
    private readonly ILogger<MoveDepartmentHandler> _logger;

    public MoveDepartmentHandler(
        IDepartmentsRepository departmentsRepository,
        ITransactionManager transactionManager,
        IValidator<MoveDepartmentCommand> validator,
        ILogger<MoveDepartmentHandler> logger)
    {
        _departmentsRepository = departmentsRepository;
        _transactionManager = transactionManager;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(
        MoveDepartmentCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            _logger.LogError("Validation Failed for MoveDepartmentCommand");
            return validationResult.ToErrorList();
        }

        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);

        if (transactionScopeResult.IsFailure)
        {
            _logger.LogError("Failed Transaction Begin");
            return transactionScopeResult.Error.ToErrors();
        }

        using var transactionScope = transactionScopeResult.Value;

        var departmentResult = await _departmentsRepository.GetByIdWithLockAsync(
            DepartmentId.Create(command.DepartmentId), cancellationToken);

        if (departmentResult.IsFailure || !departmentResult.Value.IsActive)
        {
            transactionScope.Rollback();

            _logger.LogError("Failed to get department by {id}", command.DepartmentId);
            return departmentResult.Error.ToErrors();
        }

        var departmentToMove = departmentResult.Value;

        // блокирую потомков подразделения
        var lockDescendantResult = await _departmentsRepository.LockDescendant(departmentToMove, cancellationToken);
        if (lockDescendantResult.IsFailure)
        {
            transactionScope.Rollback();
            _logger.LogError("Failed to lock department descendant {departmentId}", departmentToMove.Id.Value);
            return lockDescendantResult.Error.ToErrors();
        }

        Department? departmentParent = null;

        if (command.Request.ParentId is not null)
        {
            var newParentId = DepartmentId.Create(command.Request.ParentId.Value);

            // Проверяем, что parentId не равен departmentId
            if (command.Request.ParentId == command.DepartmentId)
            {
                transactionScope.Rollback();
                _logger.LogError("ParentId cannot be the same as DepartmentId");
                return DepartmentsErrors.CannotMoveToSelf().ToErrors();
            }

            // Получаем нового родителя
            var departmentParentResult = await _departmentsRepository.GetByIdWithLockAsync(
                newParentId, cancellationToken);

            if (departmentParentResult.IsFailure)
            {
                transactionScope.Rollback();
                _logger.LogError("Failed to get parent department by Id {id}", command.Request.ParentId);
                return departmentParentResult.Error.ToErrors();
            }

            departmentParent = departmentParentResult.Value;

            if (!departmentParent.IsActive)
            {
                transactionScope.Rollback();
                _logger.LogError("DepartmentParent is not active");
                return DepartmentsErrors.ParentNotActive().ToErrors();
            }

            // проверяем, что родитель не является потомком
            var isDescendantResult = await _departmentsRepository.IsDescendant(
                newParentId,
                DepartmentId.Create(command.DepartmentId),
                cancellationToken);

            if (isDescendantResult.IsFailure)
            {
                transactionScope.Rollback();
                _logger.LogError("Failed to check descendant relationship");
                return isDescendantResult.Error.ToErrors();
            }

            if (isDescendantResult.Value)
            {
                transactionScope.Rollback();
                _logger.LogError(
                    "Cannot move department {departmentId} under its descendant {parentId}",
                    command.DepartmentId, command.Request.ParentId);
                return DepartmentsErrors.CannotMoveToDescendant().ToErrors();
            }
        }

        var oldPath = departmentToMove.Path;
        int oldDepth = departmentToMove.Depth;

        var moveResult = departmentToMove.UpdateParent(departmentParent);
        if (moveResult.IsFailure)
        {
            transactionScope.Rollback();
            _logger.LogError("Failed to Move departments {depart}", departmentParent);
            return moveResult.Error.ToErrors();
        }

        var saveChangesResult = await _transactionManager.SaveChangesAsync(cancellationToken);

        if (saveChangesResult.IsFailure)
        {
            transactionScope.Rollback();
            _logger.LogError("Failed to save changes");
            return saveChangesResult.Error.ToErrors();
        }

        int newDepth = departmentToMove.Depth;
        int depthDifference = newDepth - oldDepth;

        var updateDescendantsResult = await _departmentsRepository.UpdateDescendantsPathAndDepthAsync(
            departmentToMove.Id,
            oldPath,
            departmentToMove.Path,
            depthDifference,
            cancellationToken);

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

        _logger.LogInformation(
            "Successfully moved department {departmentId} to parent {parentId}",
            command.DepartmentId, command.Request.ParentId);

        return command.DepartmentId;
    }
}