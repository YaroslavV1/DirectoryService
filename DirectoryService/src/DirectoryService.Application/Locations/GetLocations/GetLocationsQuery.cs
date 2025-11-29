using DirectoryService.Application.Abstractions.Queries;
using DirectoryService.Contracts.Locations.GetLocations;

namespace DirectoryService.Application.Locations.GetLocations;

public record GetLocationsQuery(GetLocationsRequest Request): IQuery;