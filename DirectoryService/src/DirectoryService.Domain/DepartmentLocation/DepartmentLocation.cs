using DirectoryService.Domain.DepartmentLocation.ValueObjects;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Locations.ValueObjects;

namespace DirectoryService.Domain.DepartmentLocation;

public class DepartmentLocation
{
    public DepartmentLocationId Id { get; private set; }

    public DepartmentId DepartmentId { get; private set; }

    public LocationId LocationId { get; private set; }
}