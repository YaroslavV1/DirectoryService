using CSharpFunctionalExtensions;
using DirectoryService.Domain.DepartmentPositions.ValueObjects;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Positions.ValueObjects;
using SharedService;

namespace DirectoryService.Domain.DepartmentPositions;

public class DepartmentPosition
{
    //ef core
    private DepartmentPosition()
    {
    }

    private DepartmentPosition(
        DepartmentPositionId departmentPositionId,
        DepartmentId departmentId,
        PositionId positionId)
    {
        Id = departmentPositionId;
        PositionId = positionId;
        DepartmentId = departmentId;
    }

    public DepartmentPositionId Id { get; private set; }

    public DepartmentId DepartmentId { get; private set; }

    public PositionId PositionId { get; private set; }

    public static Result<DepartmentPosition, Error> Create(
        DepartmentPositionId departmentPositionId,
        DepartmentId departmentId,
        PositionId positionId) => new DepartmentPosition(departmentPositionId, departmentId, positionId);
}