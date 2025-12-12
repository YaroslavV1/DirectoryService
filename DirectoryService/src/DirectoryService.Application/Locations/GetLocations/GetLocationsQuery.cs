using DirectoryService.Contracts.Locations.GetLocations;
using SharedService.Core.Abstractions.Queries;

namespace DirectoryService.Application.Locations.GetLocations;

public record GetLocationsQuery(GetLocationsRequest Request): IQuery;