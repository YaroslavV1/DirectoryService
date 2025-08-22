using DirectoryService.Domain.DepartmentEntity;
using DirectoryService.Domain.LocationEntity.ValueObjects;

namespace DirectoryService.Domain.LocationEntity;

public class Location
{
    private readonly List<DepartmentLocation> _departments = [];

    //ef core
    private Location() {
    }

    private Location(
        Guid locationId,
        LocationName locationName,
        Address address,
        LocationTimeZone timeZone)
    {
        Id = locationId;
        Name = locationName;
        Address = address;
        TimeZone = timeZone;
        CreatedAt = DateTime.Now;
    }

    public Guid Id { get; }

    public LocationName Name { get;  private set; }

    public Address Address { get;  private set; }

    public LocationTimeZone TimeZone { get;  private set; }

    public bool IsActive { get; private set; } = true;

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyList<DepartmentLocation> Departments => _departments;

    public static Location Create(
        Guid locationId,
        LocationName locationName,
        Address address,
        LocationTimeZone timeZone) => new Location(locationId, locationName, address, timeZone);

}