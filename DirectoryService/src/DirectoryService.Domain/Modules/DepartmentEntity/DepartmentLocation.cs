using DirectoryService.Domain.Modules.DepartmentEntity.ValueObjects;
using DirectoryService.Domain.Modules.LocationEntity;
using DirectoryService.Domain.Modules.LocationEntity.ValueObjects;

namespace DirectoryService.Domain.Modules.DepartmentEntity;

public class DepartmentLocation
{
    //ef core
    private DepartmentLocation(){}

    public DepartmentLocation(
        DepartmentId departmentId,
        LocationId locationId,
        Department department,
        Location location)
    {
        DepartmentId = departmentId;
        LocationId = locationId;
        Department = department;
        Location = location;
    }

    public DepartmentId DepartmentId { get; private set; }

    public LocationId LocationId { get; private set; }

    public Department Department { get; private set; }

    public Location Location { get; private set; }
}