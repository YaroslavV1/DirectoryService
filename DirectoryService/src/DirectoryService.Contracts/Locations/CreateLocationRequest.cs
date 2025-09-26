namespace DirectoryService.Contracts.Locations;

public record CreateLocationRequest(
    string Name,
    CreateLocationAddressDto Address,
    string TimeZone);