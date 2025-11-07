namespace DirectoryService.Shared.Locations;

public static class LocationsErrors
{
    public static Error NameConflict(string name) =>
        Error.Conflict("location.name.conflict", $"Location с именем {name} уже существует!");

    public static Error AddressConflict() =>
        Error.Conflict("location.address.conflict", "Location с таким адрессом уже существует!");

    public static Error DatabaseError() =>
        Error.Failure("location.database.error", "Ошибка базы данных при работе с сервисом - location!");

    public static Error OperationCancelled() =>
        Error.Failure("location.operation.cancelled", "Операция с локациями отменена!");
}