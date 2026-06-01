using System.Windows.Threading;
using TaskManager.ViewModels.Abstracts;

namespace TaskManager.Providers;

public class UiDispatcher : IUiDispatcher
{
    private readonly Dispatcher _dispatcher;

    public UiDispatcher(Dispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public bool CheckAccess()
    {
        return _dispatcher.CheckAccess();
    }

    public Task InvokeAsync(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (_dispatcher.CheckAccess())
        {
            action();
            return Task.CompletedTask;
        }

        return _dispatcher.InvokeAsync(action).Task;
    }

    public async Task InvokeAsync(Func<Task> asyncAction)
    {
        ArgumentNullException.ThrowIfNull(asyncAction);

        if (_dispatcher.CheckAccess())
        {
            await asyncAction();
            return;
        }

        await _dispatcher.InvokeAsync(asyncAction).Task.Unwrap();
    }

    public Task<T> InvokeAsync<T>(Func<T> function)
    {
        ArgumentNullException.ThrowIfNull(function);

        if (_dispatcher.CheckAccess())
        {
            return Task.FromResult(function());
        }

        return _dispatcher.InvokeAsync(function).Task;
    }

    public async Task<T> InvokeAsync<T>(Func<Task<T>> asyncFunction)
    {
        ArgumentNullException.ThrowIfNull(asyncFunction);

        if (_dispatcher.CheckAccess())
        {
            return await asyncFunction();
        }

        return await _dispatcher.InvokeAsync(asyncFunction).Task.Unwrap();
    }
}