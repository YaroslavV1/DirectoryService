using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.DepartmentLocations.ValueObjects;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Locations.ValueObjects;
using DirectoryService.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.UpdateDepartmentLocations;

public class UpdateDepartmentLocationsHandler : ICommandHandler<Result<Guid, Errors>, UpdateDepartmentLocationsCommand>
{
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ILocationsRepository _locationsRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly IValidator<UpdateDepartmentLocationsCommand> _validator;
    private readonly ILogger<UpdateDepartmentLocationsHandler> _logger;

    public UpdateDepartmentLocationsHandler(
        IDepartmentsRepository departmentsRepository,
        ILocationsRepository locationsRepository,
        ITransactionManager transactionManager,
        IValidator<UpdateDepartmentLocationsCommand> validator,
        ILogger<UpdateDepartmentLocationsHandler> logger)
    {
        _departmentsRepository = departmentsRepository;
        _locationsRepository = locationsRepository;
        _transactionManager = transactionManager;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(
        UpdateDepartmentLocationsCommand command,
        CancellationToken cancellationToken = default)
    {
        var validatorResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validatorResult.IsValid)
        {
            _logger.LogError("Validation failed for UpdateDepartmentLocationsCommand");
            return validatorResult.ToErrorList();
        }

        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);

        if (transactionScopeResult.IsFailure)
        {
            _logger.LogError("Failed Transaction Begin");
            return transactionScopeResult.Error.ToErrors();
        }

        using var transactionScope = transactionScopeResult.Value;

        var departmentId = DepartmentId.Create(command.DepartmentId);

        var departmentResult =
            await _departmentsRepository.GetByIdAsync(departmentId, cancellationToken);

        if (departmentResult.IsFailure)
        {
            transactionScope.Rollback();

            _logger.LogError("Failed to get department by id: {id}", departmentId);
            return departmentResult.Error.ToErrors();
        }

        var locationsId = command.Request.LocationIds.Select(LocationId.Create);

        var isAllLocationIdsExistResult =
            await _locationsRepository.CheckAllLocationsExistByIds(locationsId, cancellationToken);

        if (isAllLocationIdsExistResult.IsFailure)
        {
            transactionScope.Rollback();

            _logger.LogError("Failed to check if all location ids exist!");
            return isAllLocationIdsExistResult.Error;
        }

        if (!isAllLocationIdsExistResult.Value)
        {
            transactionScope.Rollback();

            _logger.LogError("One or more location ids doesn`t exist!");
            return Error.Validation(
                "locations.not-found.validation",
                "Одна или более локаций не найдено!").ToErrors();
        }

        var department = departmentResult.Value;

        var departmentLocations = locationsId.Select(dl =>
            DepartmentLocation.Create(DepartmentLocationId.NewGuid(), department.Id, dl).Value);

        var updateDepartmentLocationsResult = department.UpdateLocations(departmentLocations);

        if (updateDepartmentLocationsResult.IsFailure)
        {
            transactionScope.Rollback();

            _logger.LogError("Failed to update department locations by ids");
            return updateDepartmentLocationsResult.Error.ToErrors();
        }

        var saveChangesAsync = await _transactionManager.SaveChangesAsync(cancellationToken);

        if (saveChangesAsync.IsFailure)
        {
            transactionScope.Rollback();

            _logger.LogError("Failed to save changes");
            return saveChangesAsync.Error.ToErrors();
        }

        var commitResult = transactionScope.Commit();

        if (commitResult.IsFailure)
        {
            transactionScope.Rollback();

            _logger.LogError("Failed commit");
            return commitResult.Error.ToErrors();
        }

        return department.Id.Value;
    }
}