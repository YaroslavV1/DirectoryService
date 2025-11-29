namespace DirectoryService.Contracts.Locations.CreateLocation;

public record CreateLocationAddressDto(
    string City,
    string Street,
    string House,
    string PostalCode);