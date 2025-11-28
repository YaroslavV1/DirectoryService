namespace DirectoryService.Contracts.Locations.GetLocations;

public record GetLocationsDto(List<GetLocationDto> Locations, long TotalCount);