namespace DirectoryService.Shared.Departments;

public static class DepartmentsErrors
{
    public static Error IdentifierConflict(string identifier) =>
        Error.Conflict("department.identifier.conflict", $"Department с идентификатором {identifier} уже существует в этом родительском отделе!");

    public static Error RootIdentifierConflict(string identifier) =>
        Error.Conflict("department.root_identifier.conflict", $"Корневой department с идентификатором {identifier} уже существует!");

    public static Error ParentNotFound(Guid? parentId) =>
        Error.NotFound("department.parent.not_found", $"Родительский department с ID {parentId} не найден!");

    public static Error DatabaseError() =>
        Error.Failure("department.database.error", "Ошибка базы данных при работе с сервисом - department!");

    public static Error OperationCancelled() =>
        Error.Failure("department.operation.cancelled", "Операция с отделами отменена!");
}