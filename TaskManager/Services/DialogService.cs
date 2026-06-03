using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using TaskManager.ViewModels.Abstracts;

namespace TaskManager.Services;

public class DialogService : ObservableObject, IDialogService
{
    private const string DefaultMessageBoxTitle = "Task manager";
    
    public Task ShowMessageAsync(
        string message,
        string title = DefaultMessageBoxTitle)
    {
        var dispatcher = Application.Current.Dispatcher;

        if (dispatcher.CheckAccess())
        {
            ShowMessage(message, title);
            return Task.CompletedTask;
        }

        return dispatcher
            .InvokeAsync(() => ShowMessage(message, title))
            .Task;
    }

    private static void ShowMessage(
        string message,
        string title)
    {
        var owner = GetOwnerWindow();

        if (owner is not null)
        {
            MessageBox.Show(
                owner,
                message,
                title,
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            return;
        }

        MessageBox.Show(
            message,
            title,
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private static Window? GetOwnerWindow()
    {
        var activeWindow = Application.Current.Windows
            .OfType<Window>()
            .FirstOrDefault(window => window.IsActive);

        if (activeWindow?.IsVisible == true)
        {
            return activeWindow;
        }

        return Application.Current.MainWindow?.IsVisible == true
            ? Application.Current.MainWindow
            : null;
    }
}
