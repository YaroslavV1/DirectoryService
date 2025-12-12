using SharedService;

namespace DirectoryService.Shared.Departments;

public static class DepartmentsErrors
{
    public static Error IdentifierConflict(string identifier) =>
        Error.Conflict("department.identifier.conflict",
            $"Department с идентификатором {identifier} уже существует в этом родительском отделе!");

    public static Error RootIdentifierConflict(string identifier) =>
        Error.Conflict("department.root_identifier.conflict",
            $"Корневой department с идентификатором {identifier} уже существует!");

    public static Error ParentNotFound(Guid? parentId) =>
        Error.NotFound("department.parent.not_found", $"Родительский department с ID {parentId} не найден!");

    public static Error DatabaseError() =>
        Error.Failure("department.database.error", "Ошибка базы данных при работе с сервисом - department!");

    public static Error OperationCancelled() =>
        Error.Failure("department.operation.cancelled", "Операция с отделами отменена!");

    public static Error CannotMoveToSelf() =>
        Error.Validation("department.move.to_self", "Нельзя переместить департамент в самого себя!");

    public static Error ParentNotActive() =>
        Error.Validation("department.parent.not_active", "Родительский департамент не активен!");

    public static Error CannotMoveToDescendant() =>
        Error.Validation(
            "department.move.to_descendant",
            "Нельзя переместить департамент в свой дочерний департамент!");
}