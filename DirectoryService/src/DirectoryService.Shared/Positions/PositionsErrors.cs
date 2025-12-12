using SharedService;

namespace DirectoryService.Shared.Positions;

public static class PositionsErrors
{
    public static Error NameConflict(string name) =>
        Error.Conflict("position.name.conflict", $"Position с именем {name} уже существует!");

    public static Error DepartmentNotFound(Guid departmentId) =>
        Error.NotFound("position.department.not_found", $"Department с ID {departmentId} не найден!");

    public static Error DatabaseError() =>
        Error.Failure("position.database.error", "Ошибка базы данных при работе с сервисом - position!");

    public static Error OperationCancelled() =>
        Error.Failure("position.operation.cancelled", "Операция с позициями отменена!");
}