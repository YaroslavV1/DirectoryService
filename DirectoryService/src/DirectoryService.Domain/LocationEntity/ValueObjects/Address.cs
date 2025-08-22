using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.LocationEntity.ValueObjects;

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

    public static Result<Address> Create(string city, string street, string postalCode, string house)
    {
        if (string.IsNullOrWhiteSpace(city))
            return Result.Failure<Address>("City cannot be empty");

        if (string.IsNullOrWhiteSpace(street))
            return Result.Failure<Address>("Street cannot be empty");

        if (string.IsNullOrWhiteSpace(postalCode))
            return Result.Failure<Address>("Postal code cannot be empty");

        if (string.IsNullOrWhiteSpace(house))
            return Result.Failure<Address>("House cannot be empty");

        return new Address(city, street, postalCode, house);
    }
}