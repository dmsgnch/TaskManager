using System.Windows.Threading;
using Serilog;
using TaskManager.ViewModels.Abstracts;

namespace TaskManager.Providers;

public class UiDispatcher : IUiDispatcher
{
    private readonly Dispatcher _dispatcher;

    public UiDispatcher(Dispatcher dispatcher)
    {
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
    }

    public bool CheckAccess()
    {
        return _dispatcher.CheckAccess();
    }

    public async Task InvokeAsync(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);

        try
        {
            if (_dispatcher.CheckAccess())
            {
                action();
                return;
            }

            await _dispatcher.InvokeAsync(action).Task;
        }
        catch (Exception ex)
        {
            HandleException(ex);
            throw;
        }
    }

    public async Task InvokeAsync(Func<Task> asyncAction)
    {
        ArgumentNullException.ThrowIfNull(asyncAction);

        try
        {
            if (_dispatcher.CheckAccess())
            {
                await asyncAction();
                return;
            }

            Task task = await _dispatcher.InvokeAsync(asyncAction).Task;
            await task;
        }
        catch (Exception ex)
        {
            HandleException(ex);
            throw;
        }
    }

    public async Task<T> InvokeAsync<T>(Func<T> function)
    {
        ArgumentNullException.ThrowIfNull(function);

        try
        {
            if (_dispatcher.CheckAccess())
            {
                return function();
            }

            return await _dispatcher.InvokeAsync(function).Task;
        }
        catch (Exception ex)
        {
            HandleException(ex);
            throw;
        }
    }

    public async Task<T> InvokeAsync<T>(Func<Task<T>> asyncFunction)
    {
        ArgumentNullException.ThrowIfNull(asyncFunction);

        try
        {
            if (_dispatcher.CheckAccess())
            {
                return await asyncFunction();
            }

            Task<T> task = await _dispatcher.InvokeAsync(asyncFunction).Task;
            return await task;
        }
        catch (Exception ex)
        {
            HandleException(ex);
            throw;
        }
    }

    public void RunAndForget(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);

        _ = RunAndForgetInternalAsync(() =>
        {
            action();
            return Task.CompletedTask;
        });
    }

    public void RunAndForget(Func<Task> asyncAction)
    {
        ArgumentNullException.ThrowIfNull(asyncAction);

        _ = RunAndForgetInternalAsync(asyncAction);
    }

    private async Task RunAndForgetInternalAsync(Func<Task> asyncAction)
    {
        try
        {
            await InvokeAsync(asyncAction);
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "RunAndForget UI operation failed.");
        }
    }

    private void HandleException(Exception exception)
    {
        Log.Error(exception, "UI dispatcher operation failed.");
    }
}