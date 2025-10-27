using CSharpFunctionalExtensions;
using DirectoryService.Shared;

namespace DirectoryService.Domain.Locations.ValueObjects;

public record Address
{
    public string City { get; }

    public string Street { get; }

    public string PostalCode { get; }

    public string House { get; }

    private Address(string city, string street, string postalCode, string house)
    {
        City = city;
        Street = street;
        PostalCode = postalCode;
        House = house;
    }

    public static Result<Address, Error> Create(string city, string street, string postalCode, string house)
    {
        if (string.IsNullOrWhiteSpace(city))
            return GeneralErrors.ValueIsRequired("LocationCity");
        if (string.IsNullOrWhiteSpace(street))
            return GeneralErrors.ValueIsRequired("LocationStreet");

        if (string.IsNullOrWhiteSpace(postalCode))
            return GeneralErrors.ValueIsRequired("LocationPostalCode");
        if (string.IsNullOrWhiteSpace(house))
            return GeneralErrors.ValueIsRequired("LocationHouse");

        return new Address(city, street, postalCode, house);
    }
}