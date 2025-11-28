using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Commands;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.DepartmentLocations.ValueObjects;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Locations.ValueObjects;
using DirectoryService.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.CreateDepartment;

public class CreateDepartmentHandler : ICommandHandler<Result<Guid, Errors>, CreateDepartmentCommand>
{
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ILocationsRepository _locationsRepository;
    private readonly IValidator<CreateDepartmentCommand> _validator;
    private readonly ILogger<CreateDepartmentHandler> _logger;

    public CreateDepartmentHandler(
        IDepartmentsRepository departmentsRepository,
        ILocationsRepository locationsRepository,
        IValidator<CreateDepartmentCommand> validator,
        ILogger<CreateDepartmentHandler> logger)
    {
        _departmentsRepository = departmentsRepository;
        _locationsRepository = locationsRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(
        CreateDepartmentCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            _logger.LogError("Validation failed for CreateDepartmentCommand");
            return validationResult.ToErrorList();
        }

        Department? departmentParent = null;
        if (command.Request.ParentId is not null)
        {
            var parentId = DepartmentId.Create(command.Request.ParentId.Value);
            var parentResult = await _departmentsRepository.GetByIdAsync(parentId, cancellationToken);

            if (parentResult.IsFailure)
            {
                _logger.LogError("Parent department not found (Id: {ParentId})", command.Request.ParentId);
                return parentResult.Error.ToErrors();
            }

            departmentParent = parentResult.Value;
        }

        var locationIds = command.Request.LocationIds.Select(LocationId.Create).ToList();
        var locationsCheck = await _locationsRepository.CheckAllLocationsExistByIds(locationIds, cancellationToken);

        if (locationsCheck.IsFailure)
        {
            _logger.LogError("Not all provided locations exist");
            return locationsCheck.Error;
        }

        var identifier = Identifier.Create(command.Request.Identifier).Value;

        var departmentId = DepartmentId.NewId();
        var departmentLocations = locationIds
            .Select(locationId => DepartmentLocation.Create(
                DepartmentLocationId.NewGuid(),
                departmentId,
                locationId).Value)
            .ToList();

        var departmentName = DepartmentName.Create(command.Request.Name).Value;

        var departmentResult = departmentParent is null
            ? Department.CreateParent(departmentName, identifier, departmentLocations)
            : Department.CreateChild(departmentName, identifier, departmentParent, departmentLocations);

        if (departmentResult.IsFailure)
        {
            _logger.LogError("Failed to create department entity");
            return departmentResult.Error.ToErrors();
        }

        var createResult = await _departmentsRepository.Create(departmentResult.Value, cancellationToken);

        if (createResult.IsFailure)
        {
            _logger.LogError("Failed to persist department (Name: {Name})", command.Request.Name);
            return createResult.Error.ToErrors();
        }

        _logger.LogInformation("Department '{Name}' successfully created with Id {Id}", command.Request.Name,
            createResult.Value);
        return createResult.Value;
    }
}