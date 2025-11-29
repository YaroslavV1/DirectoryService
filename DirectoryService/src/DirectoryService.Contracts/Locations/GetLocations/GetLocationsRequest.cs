namespace DirectoryService.Contracts.Locations.GetLocations;

public record GetLocationsRequest(
    IEnumerable<Guid>? DepartmentIds,
    string? Search,
    bool? IsActive,
    int? Page = 1,
    int? PageSize = 20,
    string? SortBy = "name",
    string? SortDirection = "asc");