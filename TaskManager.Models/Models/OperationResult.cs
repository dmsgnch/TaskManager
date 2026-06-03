namespace TaskManager.Models.Models;

public readonly record struct OperationResult(
    OperationStatus Status,
    string ErrorKey)
{
    public bool IsSuccess => Status == OperationStatus.Success;
    public bool IsFailed => Status == OperationStatus.Failed;
    public bool IsCancelled => Status == OperationStatus.Cancelled;

    public static OperationResult Success =>
        new(OperationStatus.Success, string.Empty);

    public static OperationResult Fail(string errorKey) =>
        new(OperationStatus.Failed, errorKey);

    public static OperationResult Cancelled =>
        new(OperationStatus.Cancelled, string.Empty);
}

public readonly record struct OperationResult<T>(
    OperationStatus Status,
    T? Value,
    string ErrorKey)
{
    public bool IsSuccess => Status == OperationStatus.Success;
    public bool IsFailed => Status == OperationStatus.Failed;
    public bool IsCancelled => Status == OperationStatus.Cancelled;

    public static OperationResult<T> Success(T value) =>
        new(OperationStatus.Success, value, string.Empty);

    public static OperationResult<T> Fail(string errorKey) =>
        new(OperationStatus.Failed, default, errorKey);

    public static OperationResult<T> Cancelled =>
        new(OperationStatus.Cancelled, default, string.Empty);
}

public enum OperationStatus
{
    Success,
    Failed,
    Cancelled
}
