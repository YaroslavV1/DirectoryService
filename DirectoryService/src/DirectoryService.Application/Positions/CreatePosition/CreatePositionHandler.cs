using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions.Commands;
using DirectoryService.Application.Departments;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.DepartmentPositions.ValueObjects;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Positions;
using DirectoryService.Domain.Positions.ValueObjects;
using DirectoryService.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Positions.CreatePosition;

public class CreatePositionHandler : ICommandHandler<Result<Guid, Errors>, CreatePositionCommand>
{
    private readonly IPositionRepository _positionRepository;
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly IValidator<CreatePositionCommand> _validator;
    private readonly ILogger<CreatePositionHandler> _logger;

    public CreatePositionHandler(
        IPositionRepository positionRepository,
        IDepartmentsRepository departmentsRepository,
        IValidator<CreatePositionCommand> validator,
        ILogger<CreatePositionHandler> logger)
    {
        _positionRepository = positionRepository;
        _departmentsRepository = departmentsRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(
        CreatePositionCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            _logger.LogError("Validation Failed for CreatePositionCommand");
            return validationResult.ToErrorList();
        }

        var positionName = PositionName.Create(command.Request.Name).Value;

        var departmentIdsResult = await _departmentsRepository.CheckIfAllDepartmentsExistAsync(
            command.Request.DepartmentsIds,
            cancellationToken);

        if (departmentIdsResult.IsFailure)
            return departmentIdsResult.Error.ToErrors();

        var positionId = PositionId.NewId();

        var departmentIds = command.Request.DepartmentsIds;

        var departmentPositions = departmentIds.Select(dp => DepartmentPosition.Create(
            DepartmentPositionId.NewGuid(),
            DepartmentId.Create(dp),
            positionId).Value);

        var positionResult = Position.Create(
            positionId,
            positionName,
            command.Request.Description,
            departmentPositions);

        if (positionResult.IsFailure)
        {
            _logger.LogError("Position Creation Failed");
            return positionResult.Error.ToErrors();
        }

        var createResult = await _positionRepository.Create(positionResult.Value, cancellationToken);

        if (createResult.IsFailure)
        {
            _logger.LogError("Failed to create position to DB");
            return createResult.Error.ToErrors();
        }

        _logger.LogInformation("Position '{Name}' successfully created with Id {Id}", command.Request.Name,
            createResult.Value);

        return createResult.Value;
    }
}