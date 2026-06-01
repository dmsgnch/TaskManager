namespace TaskManager.ViewModels.Abstracts;

public interface IUiDispatcher
{
    bool CheckAccess();

    Task InvokeAsync(Action action);

    Task InvokeAsync(Func<Task> asyncAction);

    Task<T> InvokeAsync<T>(Func<T> function);

    Task<T> InvokeAsync<T>(Func<Task<T>> asyncFunction);
}