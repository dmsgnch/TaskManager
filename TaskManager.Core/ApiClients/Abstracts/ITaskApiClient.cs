using TaskManager.Models.Models;

namespace TaskManager.Core.ApiClients.Abstracts;

public interface ITaskApiClient
{
    Task<OperationResult<IReadOnlyList<TaskItem>>> GetTasksAsync(
        CancellationToken cancellationToken = default);
}
