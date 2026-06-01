using Serilog;
using TaskManager.Core.Services.Abstracts;
using TaskManager.Core.ApiClients.Abstracts;
using TaskManager.Models.Models;

namespace TaskManager.Core.Services;

public class TaskService : ITaskService
{
    private const string TasksNotInitializedMessage = "Tasks are not initialized.";
    private const string InitializeTasksFailedMessage = "Failed to initialize tasks.";
    private const string CreateTaskFailedMessage = "Failed to create task.";
    private const string EditTaskFailedMessage = "Failed to edit task.";

    private readonly ITaskApiClient _taskApiClient;
    private readonly SemaphoreSlim _tasksGate = new(1, 1);

    private int _nextLocalTaskId = 1;
    private bool _isInitialized;

    private List<TaskItem>? Tasks { get; set; }

    public bool IsInitialized => Volatile.Read(ref _isInitialized);

    public event EventHandler? TasksInitialized;

    public event EventHandler<TaskItem>? TaskAdded;

    public event EventHandler<TaskItem>? TaskEdited;

    public TaskService(ITaskApiClient taskApiClient)
    {
        _taskApiClient = taskApiClient;
    }

    public async Task<OperationResult<IReadOnlyList<TaskItem>>> InitializeTasksAsync(
        CancellationToken cancellationToken = default)
    {
        OperationResult<IReadOnlyList<TaskItem>> result;
        bool shouldRaiseTasksInitialized = false;

        await _tasksGate.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            if (Tasks is not null)
            {
                result = OperationResult<IReadOnlyList<TaskItem>>.Success(CreateTasksSnapshot());
            }
            else
            {
                var tasksResult = await _taskApiClient
                    .GetTasksAsync(cancellationToken)
                    .ConfigureAwait(false);

                if (!tasksResult.IsValid)
                {
                    result = OperationResult<IReadOnlyList<TaskItem>>.Fail(tasksResult.ErrorMessage);
                }
                else
                {
                    Tasks = tasksResult.Value?.Select(CloneTask).ToList() ?? [];

                    _nextLocalTaskId = Tasks.Count > 0
                        ? Tasks.Max(task => task.Id) + 1
                        : 1;

                    Volatile.Write(ref _isInitialized, true);
                    shouldRaiseTasksInitialized = true;

                    result = OperationResult<IReadOnlyList<TaskItem>>.Success(CreateTasksSnapshot());
                }
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to initialize tasks");

            result = OperationResult<IReadOnlyList<TaskItem>>.Fail(InitializeTasksFailedMessage);
        }
        finally
        {
            _tasksGate.Release();
        }

        if (shouldRaiseTasksInitialized && result.IsValid)
        {
            RaiseTasksInitialized();
        }

        return result;
    }

    public async Task<OperationResult<IReadOnlyList<TaskItem>>> GetTasksSnapshotAsync(
        CancellationToken cancellationToken = default)
    {
        await _tasksGate.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            if (Tasks is null)
            {
                return OperationResult<IReadOnlyList<TaskItem>>.Fail(TasksNotInitializedMessage);
            }

            return OperationResult<IReadOnlyList<TaskItem>>.Success(CreateTasksSnapshot());
        }
        finally
        {
            _tasksGate.Release();
        }
    }

    public async Task<OperationResult<TaskItem>> TryCreateLocalTaskAsync(
        TaskEditParams taskEditParams,
        CancellationToken cancellationToken = default)
    {
        OperationResult<TaskItem> result;

        await _tasksGate.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            if (Tasks is null)
            {
                result = OperationResult<TaskItem>.Fail(TasksNotInitializedMessage);
            }
            else if (!ValidateTaskEditParams(taskEditParams, out string errorMessage))
            {
                result = OperationResult<TaskItem>.Fail(errorMessage);
            }
            else
            {
                var createdTask = new TaskItem
                {
                    Id = _nextLocalTaskId++,
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

            result = OperationResult<TaskItem>.Fail(CreateTaskFailedMessage);
        }
        finally
        {
            _tasksGate.Release();
        }

        if (result is { IsValid: true, Value: not null })
        {
            RaiseTaskAdded(result.Value);
        }

        return result;
    }

    public async Task<OperationResult<TaskItem>> TryEditLocalTaskAsync(
        int taskId,
        TaskEditParams taskEditParams,
        CancellationToken cancellationToken = default)
    {
        OperationResult<TaskItem> result;

        await _tasksGate.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            if (Tasks is null)
            {
                result = OperationResult<TaskItem>.Fail(TasksNotInitializedMessage);
            }
            else if (!ValidateTaskEditParams(taskEditParams, out string errorMessage))
            {
                result = OperationResult<TaskItem>.Fail(errorMessage);
            }
            else
            {
                int taskIndex = Tasks.FindIndex(task => task.Id == taskId);

                if (taskIndex < 0)
                {
                    result = OperationResult<TaskItem>.Fail($"Task with id {taskId} was not found.");
                }
                else
                {
                    var editedTask = new TaskItem
                    {
                        Id = taskId,
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

            result = OperationResult<TaskItem>.Fail(EditTaskFailedMessage);
        }
        finally
        {
            _tasksGate.Release();
        }

        if (result is { IsValid: true, Value: not null })
        {
            RaiseTaskEdited(result.Value);
        }

        return result;
    }

    private void RaiseTasksInitialized()
    {
        try
        {
            TasksInitialized?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during TasksInitialized invoking");
        }
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

    private static bool ValidateTaskEditParams(
        TaskEditParams taskEditParams,
        out string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(taskEditParams.Title))
        {
            errorMessage = "Task title is required.";
            return false;
        }

        if (taskEditParams.UserId <= 0)
        {
            errorMessage = "User id must be greater than zero.";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }

    private IReadOnlyList<TaskItem> CreateTasksSnapshot()
    {
        if (Tasks is null)
        {
            return [];
        }

        return Tasks
            .Select(CloneTask)
            .ToList()
            .AsReadOnly();
    }

    private static TaskItem CloneTask(TaskItem taskItem)
    {
        return new TaskItem
        {
            Id = taskItem.Id,
            UserId = taskItem.UserId,
            Title = taskItem.Title,
            IsCompleted = taskItem.IsCompleted
        };
    }
}
