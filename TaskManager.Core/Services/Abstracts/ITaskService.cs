using TaskManager.Models.Models;

namespace TaskManager.Core.Services.Abstracts;

public interface ITaskService
{
    bool IsInitialized { get; }

    event EventHandler? TasksInitialized;

    event EventHandler<TaskItem>? TaskAdded;

    event EventHandler<TaskItem>? TaskEdited;

    Task<OperationResult<IReadOnlyList<TaskItem>>> InitializeTasksAsync(
        CancellationToken cancellationToken = default);

    Task<OperationResult<IReadOnlyList<TaskItem>>> GetTasksSnapshotAsync(
        CancellationToken cancellationToken = default);

    Task<OperationResult<TaskItem>> TryCreateLocalTaskAsync(
        TaskEditParams taskEditParams,
        CancellationToken cancellationToken = default);

    Task<OperationResult<TaskItem>> TryEditLocalTaskAsync(
        int taskId,
        TaskEditParams taskEditParams,
        CancellationToken cancellationToken = default);
}
