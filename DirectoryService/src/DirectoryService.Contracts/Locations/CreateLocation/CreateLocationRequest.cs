namespace DirectoryService.Contracts.Locations.CreateLocation;

public record CreateLocationRequest(
    string Name,
    CreateLocationAddressDto Address,
    string TimeZone);