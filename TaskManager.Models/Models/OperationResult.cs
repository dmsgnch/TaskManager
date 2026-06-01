namespace TaskManager.Models.Models;

public readonly record struct OperationResult(bool IsValid, string ErrorMessage)
{
    public static OperationResult Success => new(true, string.Empty);

    public static OperationResult Fail(string errorMessage) =>
        new(false, errorMessage);
}

public readonly record struct OperationResult<T>(bool IsValid, T? Value, string ErrorMessage)
{
    public static OperationResult<T> Success(T value) =>
        new(true, value, string.Empty);

    public static OperationResult<T> Fail(string errorMessage) =>
        new(false, default, errorMessage);
}
