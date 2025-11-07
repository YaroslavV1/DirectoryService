using CSharpFunctionalExtensions;
using DirectoryService.Domain.DepartmentLocation.ValueObjects;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Locations.ValueObjects;
using DirectoryService.Shared;

namespace DirectoryService.Domain.DepartmentLocation;

public class DepartmentLocation
{

    //ef core
    private DepartmentLocation()
    {
    }

    private DepartmentLocation(
        DepartmentLocationId departmentLocationId,
        DepartmentId departmentId,
        LocationId locationId)
    {
        Id = departmentLocationId;
        DepartmentId = departmentId;
        LocationId = locationId;
    }

    public DepartmentLocationId Id { get; private set; }

    public DepartmentId DepartmentId { get; private set; }

    public LocationId LocationId { get; private set; }

    public static Result<DepartmentLocation, Errors> Create(
        DepartmentLocationId departmentLocationId,
        DepartmentId departmentId,
        LocationId locationId) =>
        new DepartmentLocation(departmentLocationId, departmentId, locationId);
}