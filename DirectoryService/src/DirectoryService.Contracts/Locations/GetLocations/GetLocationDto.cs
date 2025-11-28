namespace DirectoryService.Contracts.Locations.GetLocations;

public record GetLocationDto
{
    public Guid Id { get; init; }

    public string Name { get; init; }

    public string City { get; init; }

    public string Street { get; init; }

    public string PostalCode { get; init; }

    public string House { get; init; }

    public string TimeZone { get; init; }

    public bool IsActive { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime UpdatedAt { get; init; }
}