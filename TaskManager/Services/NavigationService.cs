using Microsoft.Extensions.DependencyInjection;
using TaskManager.ViewModels.Abstracts;
using TaskManager.Views.Windows;

namespace TaskManager.Services;

public class NavigationService : INavigationService
{
    private readonly IServiceProvider _services;

    public NavigationService(IServiceProvider services)
    {
        _services = services;
    }

    public void ShowMain()
    {
        var window = _services.GetRequiredService<MainWindow>();
        window.Show();
    }
}