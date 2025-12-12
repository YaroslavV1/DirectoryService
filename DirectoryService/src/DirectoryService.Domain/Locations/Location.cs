using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Locations.ValueObjects;
using SharedService;

namespace DirectoryService.Domain.Locations;

public class Location : Entity<LocationId>
{
    private readonly List<DepartmentLocation> _departments = [];

    // ef core
    private Location(LocationId id)
        : base(id)
    {
    }

    private Location(
        LocationId locationId,
        LocationName locationName,
        Address address,
        LocationTimeZone timeZone)
        : base(locationId)
    {
        Name = locationName;
        Address = address;
        TimeZone = timeZone;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public LocationName Name { get; private set; } = default!;

    public Address Address { get; private set; } = default!;

    public LocationTimeZone TimeZone { get; private set; } = default!;

    public bool IsActive { get; private set; } = true;

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public DateTime DeletedAt { get; private set; }

    public IReadOnlyList<DepartmentLocation> Departments => _departments;

    public static Location Create(
        LocationId locationId,
        LocationName locationName,
        Address address,
        LocationTimeZone timeZone) => new Location(locationId, locationName, address, timeZone);
}