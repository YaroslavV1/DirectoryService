namespace DirectoryService.Contracts.Locations;

public record CreateLocationAddressDto(
    string City,
    string Street,
    string House,
    string PostalCode);