using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TaskManager.Models.Constants;
using TaskManager.Models.Models;
using TaskManager.ViewModels.Abstracts;

namespace TaskManager.ViewModels.Dialogs;

public partial class TaskEditDialogViewModel : ObservableObject, ITaskEditDialogViewModel
{
    private const string CreateTaskResourceKey = "TaskCreating";
    private const string EditTaskResourceKey = "TaskEditing";

    [ObservableProperty]
    public partial string? DialogTitle { get; private set; }

    [ObservableProperty]
    public partial string TaskTitle { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string UserIdText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsCompleted { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial bool ShowUserIdForm { get; set; }

    private bool IsCreationMode { get; set; }

    private TaskCompletionSource<OperationResult<TaskEditParams>>? DismissDialog { get; set; }

    private readonly ILocalizationService _localizationService;

    public TaskEditDialogViewModel(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
    }

    public async Task<OperationResult<TaskEditParams>> ShowDialogAsync(TaskItem? taskItem = null)
    {
        IsCreationMode = taskItem is null;

        var itemParams = taskItem?.ToTaskEditParams()
                         ?? new TaskEditParams(string.Empty, 0, false);

        DismissDialog = new TaskCompletionSource<OperationResult<TaskEditParams>>();

        InitializeDialog(itemParams);

        return await DismissDialog.Task.ConfigureAwait(true);
    }

    private void InitializeDialog(TaskEditParams itemParams)
    {
        var dialogTitleKey = IsCreationMode ? CreateTaskResourceKey : EditTaskResourceKey;

        DialogTitle = _localizationService.GetString(dialogTitleKey);

        ShowUserIdForm = IsCreationMode;

        TaskTitle = itemParams.Title;
        UserIdText = itemParams.UserId == 0 ? string.Empty : itemParams.UserId.ToString();
        IsCompleted = itemParams.IsCompleted;
    }

    [RelayCommand]
    private void Confirm()
    {
        var editParams = GetValidFormResultOrDefault();

        if (editParams is {} taskEditParams)
        {
            DismissDialog?.TrySetResult(OperationResult<TaskEditParams>.Success(taskEditParams));
        }
    }

    private TaskEditParams? GetValidFormResultOrDefault()
    {
        TaskEditParams? result = null;

        string title = TaskTitle.Trim();

        if (string.IsNullOrWhiteSpace(title))
        {
            ErrorMessage = _localizationService.GetString(OperationErrorKeys.TaskTitleRequired);
        }
        else if (!int.TryParse(UserIdText, out int userId) || userId <= 0)
        {
            ErrorMessage = _localizationService.GetString(OperationErrorKeys.UserIdMustBeGreaterThanZero);
        }
        else
        {
            result = new TaskEditParams(title, userId, IsCompleted);

            ErrorMessage = null;
        }

        return result;
    }

    [RelayCommand]
    private void Cancel()
    {
        DismissDialog?.TrySetResult(OperationResult<TaskEditParams>.Cancelled);
    }
}
