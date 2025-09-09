using DirectoryService.Domain.Modules.DepartmentEntity.ValueObjects;
using DirectoryService.Domain.Modules.PositionEntity;
using DirectoryService.Domain.Modules.PositionEntity.ValueObjects;

namespace DirectoryService.Domain.Modules.DepartmentEntity;

public class DepartmentPosition
{
    //ef core
    private DepartmentPosition() { }

    public DepartmentPosition(
        DepartmentId departmentId,
        PositionId positionId,
        Department department,
        Position position)
    {
        DepartmentId = departmentId;
        PositionId = positionId;
        Department = department;
        Position = position;
    }

    public DepartmentId DepartmentId { get; private set; }

    public Department Department { get; private set; }

    public PositionId PositionId { get; private set; }

    public Position Position { get; private set; }
}