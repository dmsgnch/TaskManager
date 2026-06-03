using Serilog;
using TaskManager.Core.Services.Abstracts;
using TaskManager.Core.ApiClients.Abstracts;
using TaskManager.Models.Constants;
using TaskManager.Models.Models;

namespace TaskManager.Core.Services;

public class TaskService : ITaskService
{
    private readonly ITaskApiClient _taskApiClient;
    private readonly SemaphoreSlim _tasksGate = new(1, 1);

    private int _nextLocalTaskId = 1;

    private List<TaskItem> Tasks { get; set; } = new();

    public event EventHandler<TaskItem>? TaskAdded;

    public event EventHandler<TaskItem>? TaskEdited;

    public TaskService(ITaskApiClient taskApiClient)
    {
        _taskApiClient = taskApiClient;
    }

    public async Task<OperationResult<IReadOnlyList<TaskItem>>> LoadTasksAsync(CancellationToken cancellationToken =
        default)
    {
        OperationResult<IReadOnlyList<TaskItem>> result;

        await _tasksGate.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            var tasksResult = await _taskApiClient
                .GetTasksAsync(cancellationToken)
                .ConfigureAwait(false);

            if (!tasksResult.IsSuccess)
            {
                result = OperationResult<IReadOnlyList<TaskItem>>.Fail(tasksResult.ErrorKey);
            }
            else
            {
                Tasks = tasksResult.Value?.Select(CloneTask).ToList() ?? [];

                _nextLocalTaskId = Tasks.Count > 0
                    ? Tasks.Max(task => task.Id) + 1
                    : 1;

                result = OperationResult<IReadOnlyList<TaskItem>>.Success(CreateTasksSnapshot());
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to initialize tasks");

            result = OperationResult<IReadOnlyList<TaskItem>>.Fail(OperationErrorKeys.InitializeTasksFailed);
        }
        finally
        {
            _tasksGate.Release();
        }
        
        return result;
    }

    public async Task<OperationResult<TaskItem>> TryCreateLocalTaskAsync(TaskEditParams taskEditParams,
        CancellationToken cancellationToken = default)
    {
        OperationResult<TaskItem> result;

        await _tasksGate.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            if (!ValidateTaskEditParams(taskEditParams, out string errorKey))
            {
                result = OperationResult<TaskItem>.Fail(errorKey);
            }
            else
            {
                var createdTask = new TaskItem(_nextLocalTaskId++)
                {
                    UserId = taskEditParams.UserId,
                    Title = taskEditParams.Title.Trim(),
                    IsCompleted = taskEditParams.IsCompleted
                };

                Tasks.Add(createdTask);

                result = OperationResult<TaskItem>.Success(CloneTask(createdTask));
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to create local task");

            result = OperationResult<TaskItem>.Fail(OperationErrorKeys.CreateTaskFailed);
        }
        finally
        {
            _tasksGate.Release();
        }

        if (result is { IsSuccess: true, Value: not null })
        {
            RaiseTaskAdded(result.Value);
        }

        return result;
    }

    public async Task<OperationResult<TaskItem>> TryEditLocalTaskAsync(int taskId,
        TaskEditParams taskEditParams,
        CancellationToken cancellationToken = default)
    {
        OperationResult<TaskItem> result;

        await _tasksGate.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            if (!ValidateTaskEditParams(taskEditParams, out string errorKey))
            {
                result = OperationResult<TaskItem>.Fail(errorKey);
            }
            else
            {
                int taskIndex = Tasks.FindIndex(task => task.Id == taskId);

                if (taskIndex < 0)
                {
                    result = OperationResult<TaskItem>.Fail(OperationErrorKeys.TaskNotFound);
                }
                else
                {
                    var editedTask = new TaskItem(taskId)
                    {
                        UserId = taskEditParams.UserId,
                        Title = taskEditParams.Title.Trim(),
                        IsCompleted = taskEditParams.IsCompleted
                    };

                    Tasks[taskIndex] = editedTask;

                    result = OperationResult<TaskItem>.Success(CloneTask(editedTask));
                }
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to edit local task. Task id: {TaskId}", taskId);

            result = OperationResult<TaskItem>.Fail(OperationErrorKeys.EditTaskFailed);
        }
        finally
        {
            _tasksGate.Release();
        }

        if (result is { IsSuccess: true, Value: not null })
        {
            RaiseTaskEdited(result.Value);
        }

        return result;
    }

    private void RaiseTaskAdded(TaskItem taskItem)
    {
        try
        {
            TaskAdded?.Invoke(this, taskItem);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during TaskAdded invoking. Task id: {TaskId}", taskItem.Id);
        }
    }

    private void RaiseTaskEdited(TaskItem taskItem)
    {
        try
        {
            TaskEdited?.Invoke(this, taskItem);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during TaskEdited invoking. Task id: {TaskId}", taskItem.Id);
        }
    }

    private static bool ValidateTaskEditParams(TaskEditParams taskEditParams,
        out string errorKey)
    {
        if (string.IsNullOrWhiteSpace(taskEditParams.Title))
        {
            errorKey = OperationErrorKeys.TaskTitleRequired;
            return false;
        }

        if (taskEditParams.UserId <= 0)
        {
            errorKey = OperationErrorKeys.UserIdMustBeGreaterThanZero;
            return false;
        }

        errorKey = string.Empty;
        return true;
    }

    private IReadOnlyList<TaskItem> CreateTasksSnapshot()
    {
        return Tasks
            .Select(CloneTask)
            .ToList()
            .AsReadOnly();
    }

    private static TaskItem CloneTask(TaskItem taskItem)
    {
        return new TaskItem(taskItem.Id)
        {
            UserId = taskItem.UserId,
            Title = taskItem.Title,
            IsCompleted = taskItem.IsCompleted
        };
    }
}
