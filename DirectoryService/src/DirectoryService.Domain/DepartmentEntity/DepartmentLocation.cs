using DirectoryService.Domain.LocationEntity;

namespace DirectoryService.Domain.DepartmentEntity;

public class DepartmentLocation
{
    //ef core
    private DepartmentLocation(){}

    public DepartmentLocation(
        Guid departmentId,
        Guid locationId,
        Department department,
        Location location)
    {
        DepartmentId = departmentId;
        LocationId = locationId;
        Department = department;
        Location = location;
    }

    public Guid DepartmentId { get; private set; }

    public Guid LocationId { get; private set; }

    public Department Department { get; private set; }

    public Location Location { get; private set; }
}