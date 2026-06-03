using CommunityToolkit.Mvvm.DependencyInjection;
using TaskManager.ViewModels.Windows;

namespace TaskManager.ViewModels;

public class ViewModelLocator
{
    public static MainWindowViewModel MainWindowViewModel =>
        Ioc.Default.GetRequiredService<MainWindowViewModel>();
}