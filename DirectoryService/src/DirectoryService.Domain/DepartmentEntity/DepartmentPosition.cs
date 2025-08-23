using DirectoryService.Domain.PositionEntity;

namespace DirectoryService.Domain.DepartmentEntity;

public class DepartmentPosition
{
    //ef core
    private DepartmentPosition() { }

    public DepartmentPosition(
        Guid departmentId,
        Guid positionId,
        Department department,
        Position position)
    {
        DepartmentId = departmentId;
        PositionId = positionId;
        Department = department;
        Position = position;
    }

    public Guid DepartmentId { get; private set; }

    public Department Department { get; private set; }

    public Guid PositionId { get; private set; }

    public Position Position { get; private set; }
}