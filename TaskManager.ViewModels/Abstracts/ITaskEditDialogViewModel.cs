using TaskManager.Models.Models;

namespace TaskManager.ViewModels.Abstracts;

public interface ITaskEditDialogViewModel
{
    Task<OperationResult<TaskEditParams>> ShowDialogAsync(TaskItem? taskItem = null);
}
