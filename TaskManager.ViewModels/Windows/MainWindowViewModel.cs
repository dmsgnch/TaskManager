using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TaskManager.Core.Services.Abstracts;
using TaskManager.Models.Constants;
using TaskManager.Models.Models;
using TaskManager.ViewModels.Abstracts;
using TaskManager.ViewModels.Enums;
using TaskManager.ViewModels.Models;

namespace TaskManager.ViewModels.Windows;

public partial class MainWindowViewModel : ObservableObject
{
    private const string AllResourceKey = "All";
    private const string AllUsersResourceKey = "AllUsers";
    private const string CompletedResourceKey = "Completed";
    private const string InProgressResourceKey = "InProgress";
    private const string SelectedUsersFormatResourceKey = "SelectedUsersFormat";
    private const string UserFormatResourceKey = "UserFormat";

    private readonly ITaskService _taskService;
    private readonly IDialogService _dialogService;
    private readonly IUiDispatcher _uiDispatcher;
    private readonly ILocalizationService _localizationService;
    private readonly ITaskEditDialogViewModelFactory _taskEditDialogViewModelFactory;

    [ObservableProperty]
    private partial ObservableCollection<TaskItem>? TaskItems { get; set; }

    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial ObservableCollection<TaskFilterModeItem> CompletionFilterModes { get; set; }

    [ObservableProperty]
    public partial TaskFilterModeItem? SelectedCompletionFilterMode { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<UserFilterItem> UserFilters { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<TaskItem>? FilteredTaskItems { get; set; }

    [ObservableProperty]
    public partial TaskItem? SelectedTaskItem { get; set; }

    [ObservableProperty]
    public partial string? UserFilterText { get; set; }

    [ObservableProperty]
    public partial object? DialogViewModel { get; set; }

    [ObservableProperty]
    public partial bool ShowLoadingControl { get; set; }

    [ObservableProperty]
    public partial bool IsUkrButtonChecked { get; set; }

    public MainWindowViewModel(ITaskService taskService,
        IDialogService dialogService,
        IUiDispatcher uiDispatcher,
        ILocalizationService localizationService,
        ITaskEditDialogViewModelFactory taskEditDialogViewModelFactory)
    {
        _taskService = taskService;
        _dialogService = dialogService;
        _uiDispatcher = uiDispatcher;
        _localizationService = localizationService;
        _taskEditDialogViewModelFactory = taskEditDialogViewModelFactory;

        CompletionFilterModes = CreateCompletionFilterModes();
        UserFilters = CreateUserFilters();
        SelectedCompletionFilterMode = CompletionFilterModes.FirstOrDefault();

        _taskService.TaskAdded += TaskService_OnTaskAdded;
        _taskService.TaskEdited += TaskService_OnTaskEdited;

        ReloadTasksCommand.Execute(null);

        UpdateUserFilterText();

        IsUkrButtonChecked = _localizationService.IsUkrainianCulture;
    }

    [RelayCommand]
    private async Task ReloadTasksAsync()
    {
        ShowLoadingControl = true;

        try
        {
            var result = await _taskService.LoadTasksAsync();

            if (result is { IsSuccess: true, Value: not null })
            {
                TaskItems?.CollectionChanged -= TaskItems_OnCollectionChanged;

                TaskItems = new ObservableCollection<TaskItem>(result.Value);

                TaskItems?.CollectionChanged += TaskItems_OnCollectionChanged;

                SynchronizeUserFiltersWithTasks();
                UpdateFilteredTaskItems();
            }
            else
            {
                await ShowErrorMessageAsync(result.ErrorKey);
            }
        }
        finally
        {
            ShowLoadingControl = false;
        }
    }

    [RelayCommand]
    private async Task AddTaskAsync()
    {
        OperationResult<TaskEditParams> createTaskResult;

        try
        {
            createTaskResult = await ShowTaskDialogAsync();
        }
        finally
        {
            CloseDialogs();
        }

        if (!createTaskResult.IsSuccess)
        {
            return;
        }

        var taskCreationResult = await _taskService.TryCreateLocalTaskAsync(createTaskResult.Value);

        if (taskCreationResult.IsFailed)
        {
            await ShowErrorMessageAsync(taskCreationResult.ErrorKey);
        }
    }

    [RelayCommand]
    private async Task EditSelectedTaskAsync()
    {
        var taskItem = SelectedTaskItem;

        if (taskItem is not null)
        {
            OperationResult<TaskEditParams> editTaskResult;

            try
            {
                editTaskResult = await ShowTaskDialogAsync(taskItem);
            }
            finally
            {
                CloseDialogs();
            }

            if (!editTaskResult.IsSuccess)
            {
                return;
            }

            var taskEditingResult = await _taskService.TryEditLocalTaskAsync(taskItem.Id, editTaskResult.Value);

            if (taskEditingResult.IsFailed)
            {
                await ShowErrorMessageAsync(taskEditingResult.ErrorKey);
            }
        }
    }

    [RelayCommand]
    private void CloseSelectedTask()
    {
        SelectedTaskItem = null;
    }
    
    private async Task<OperationResult<TaskEditParams>> ShowTaskDialogAsync(TaskItem? taskItem = null)
    {
        var taskDialog = _taskEditDialogViewModelFactory.Create();

        DialogViewModel = taskDialog;

        return await taskDialog.ShowDialogAsync(taskItem);
    }

    private void CloseDialogs()
    {
        DialogViewModel = null;
    }

    private Task ShowErrorMessageAsync(string errorKey)
    {
        var localizedMessage = _localizationService.GetString(
            string.IsNullOrWhiteSpace(errorKey)
                ? OperationErrorKeys.UnexpectedOperationError
                : errorKey);

        return _dialogService.ShowMessageAsync(localizedMessage);
    }

    private void EnsureTaskItemsCollection()
    {
        if (TaskItems is null)
        {
            TaskItems = new ObservableCollection<TaskItem>();
            TaskItems.CollectionChanged += TaskItems_OnCollectionChanged;
        }
    }

    private void UpdateFilteredTaskItems()
    {
        var tasks = TaskItems?.ToList() ?? [];

        if (tasks.Any())
        {
            var selectedCompletionFilterMode = SelectedCompletionFilterMode?.Value ?? TaskFilterMode.All;

            if (selectedCompletionFilterMode != TaskFilterMode.All)
            {
                var needCompleted = selectedCompletionFilterMode == TaskFilterMode.Completed;

                tasks = tasks.Where(t => t.IsCompleted == needCompleted).ToList();
            }

            var selectedUsers = UserFilters
                .Where(userFilter => userFilter.IsSelected)
                .Select(uf => uf.UserId)
                .ToList();

            if (selectedUsers.Any())
            {
                tasks = tasks.Where(t => selectedUsers.Contains(t.UserId)).ToList();
            }

            var searchText = SearchText;
            if (!string.IsNullOrEmpty(searchText))
            {
                tasks = tasks
                    .Where(ti => ti.Title.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
        }

        FilteredTaskItems = new ObservableCollection<TaskItem>(tasks);
    }

    private void OnUserFilterPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(UserFilterItem.IsSelected))
        {
            UpdateUserFilterText();
            UpdateFilteredTaskItems();
        }
    }

    private void UpdateUserFilterText()
    {
        var selectedUsers = UserFilters
            .Where(userFilter => userFilter.IsSelected)
            .ToList();

        UserFilterText = selectedUsers.Count switch
        {
            0 => _localizationService.GetString(AllUsersResourceKey),
            1 => selectedUsers[0].DisplayName,
            _ => _localizationService.FormatString(SelectedUsersFormatResourceKey, selectedUsers.Count)
        };
    }

    private ObservableCollection<TaskFilterModeItem> CreateCompletionFilterModes()
    {
        return new ObservableCollection<TaskFilterModeItem>(
            Enum.GetValues<TaskFilterMode>().Select(CreateCompletionFilterModeItem));
    }

    private TaskFilterModeItem CreateCompletionFilterModeItem(TaskFilterMode taskFilterMode)
    {
        return new TaskFilterModeItem(
            taskFilterMode,
            _localizationService.GetString(GetTaskFilterModeResourceKey(taskFilterMode)));
    }

    private static string GetTaskFilterModeResourceKey(TaskFilterMode taskFilterMode)
    {
        return taskFilterMode switch
        {
            TaskFilterMode.All => AllResourceKey,
            TaskFilterMode.Completed => CompletedResourceKey,
            TaskFilterMode.NotCompleted => InProgressResourceKey,
            _ => taskFilterMode.ToString()
        };
    }

    private ObservableCollection<UserFilterItem> CreateUserFilters()
    {
        return new ObservableCollection<UserFilterItem>();
    }

    private void UpdateLocalizedFilterTexts()
    {
        var selectedCompletionFilterMode = SelectedCompletionFilterMode?.Value ?? TaskFilterMode.All;

        CompletionFilterModes = CreateCompletionFilterModes();
        SelectedCompletionFilterMode = CompletionFilterModes.FirstOrDefault(mode => mode.Value == selectedCompletionFilterMode)
                                       ?? CompletionFilterModes.FirstOrDefault();

        foreach (var userFilter in UserFilters)
        {
            userFilter.UpdateDisplayName(GetUserDisplayName(userFilter.UserId));
        }

        UpdateUserFilterText();
    }

    private UserFilterItem CreateUserFilterItem(int userId)
    {
        var userFilter = new UserFilterItem(userId, GetUserDisplayName(userId));
        userFilter.PropertyChanged += OnUserFilterPropertyChanged;

        return userFilter;
    }

    private void SynchronizeUserFiltersWithTasks()
    {
        var existingSelectedUserIds = UserFilters
            .Where(userFilter => userFilter.IsSelected)
            .Select(userFilter => userFilter.UserId)
            .ToHashSet();

        foreach (var userFilter in UserFilters)
        {
            userFilter.PropertyChanged -= OnUserFilterPropertyChanged;
        }

        var taskUserIds = TaskItems?
            .Select(taskItem => taskItem.UserId)
            .Distinct()
            .Order()
            .ToList() ?? [];

        UserFilters = new ObservableCollection<UserFilterItem>(
            taskUserIds.Select(CreateUserFilterItem));

        foreach (var userFilter in UserFilters)
        {
            userFilter.IsSelected = existingSelectedUserIds.Contains(userFilter.UserId);
        }

        UpdateUserFilterText();
    }

    private string GetUserDisplayName(int userId)
    {
        return _localizationService.FormatString(UserFormatResourceKey, userId);
    }

    private void TaskService_OnTaskAdded(object? sender, TaskItem taskItem)
    {
        _uiDispatcher.RunAndForget(() =>
        {
            EnsureTaskItemsCollection();

            if (TaskItems is not null && TaskItems.All(t => t.Id != taskItem.Id))
            {
                TaskItems.Add(taskItem);
                SynchronizeUserFiltersWithTasks();
            }
        });
    }

    private void TaskService_OnTaskEdited(object? sender, TaskItem taskItem)
    {
        _uiDispatcher.RunAndForget(() =>
        {
            var foundTask = TaskItems?.FirstOrDefault(ti => ti.Id == taskItem.Id);

            if (foundTask is not null)
            {
                foundTask.UpdateFrom(taskItem);
                SynchronizeUserFiltersWithTasks();
                UpdateFilteredTaskItems();
            }
        });
    }

    private void TaskItems_OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateFilteredTaskItems();
    }

    partial void OnSearchTextChanged(string value)
    {
        UpdateFilteredTaskItems();
    }

    partial void OnSelectedCompletionFilterModeChanged(TaskFilterModeItem? value)
    {
        UpdateFilteredTaskItems();
    }

    partial void OnIsUkrButtonCheckedChanged(bool value)
    {
        _localizationService.SetUkrainianCulture(value);
        UpdateLocalizedFilterTexts();
    }
}
