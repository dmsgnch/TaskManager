namespace TaskManager.Models.Constants;

public static class OperationErrorKeys
{
    public const string UnexpectedOperationError = nameof(UnexpectedOperationError);
    public const string InitializeTasksFailed = nameof(InitializeTasksFailed);
    public const string CreateTaskFailed = nameof(CreateTaskFailed);
    public const string EditTaskFailed = nameof(EditTaskFailed);
    public const string TaskTitleRequired = nameof(TaskTitleRequired);
    public const string UserIdMustBeGreaterThanZero = nameof(UserIdMustBeGreaterThanZero);
    public const string TaskNotFound = nameof(TaskNotFound);
    public const string LoadTasksEmptyResponse = nameof(LoadTasksEmptyResponse);
    public const string LoadTasksInvalidResponse = nameof(LoadTasksInvalidResponse);
    public const string LoadTasksNetworkError = nameof(LoadTasksNetworkError);
    public const string LoadTasksTimeout = nameof(LoadTasksTimeout);
    public const string LoadTasksUnexpectedError = nameof(LoadTasksUnexpectedError);
    public const string LoadTasksServerError = nameof(LoadTasksServerError);
}
