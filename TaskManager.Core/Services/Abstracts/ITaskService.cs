using TaskManager.Models.Models;

namespace TaskManager.Core.Services.Abstracts;

public interface ITaskService
{
    event EventHandler<TaskItem>? TaskAdded;

    event EventHandler<TaskItem>? TaskEdited;

    public Task<OperationResult<IReadOnlyList<TaskItem>>> LoadTasksAsync(CancellationToken cancellationToken =
        default);

    public Task<OperationResult<TaskItem>> TryCreateLocalTaskAsync(TaskEditParams taskEditParams,
        CancellationToken cancellationToken = default);

    public Task<OperationResult<TaskItem>> TryEditLocalTaskAsync(int taskId,
        TaskEditParams taskEditParams,
        CancellationToken cancellationToken = default);
}