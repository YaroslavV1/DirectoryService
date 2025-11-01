namespace DirectoryService.Shared;

public static class GeneralErrors
{
    public static Error ValueIsInvalid(string? name = null)
    {
        string label = name ?? "значение";
        return Error.Validation("value.is.invalid", $"{label} недействительно", name);
    }

    public static Error ValueIsRequired(string? name = null)
    {
        string label = name == null ? string.Empty : " " + name + " ";
        return Error.Validation("length.is.invalid", $"Поле{label}обязательно", name);
    }

    public static Error Failure()
    {
        return Error.Failure("server.failure", "Серверная ошибка");
    }

    public static Error AlreadyExists(string? name = null)
    {
        string label = name ?? "значение";
        return Error.Validation("value.already.exists", $"{label} уже существует", name);
    }
}