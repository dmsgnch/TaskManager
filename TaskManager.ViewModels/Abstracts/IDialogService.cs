namespace TaskManager.ViewModels.Abstracts;

public interface IDialogService
{
    Task ShowMessageAsync(
        string message,
        string title = "Task manager");
}
