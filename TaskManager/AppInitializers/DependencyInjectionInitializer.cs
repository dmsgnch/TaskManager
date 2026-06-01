using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using TaskManager.Core.Providers.Abstracts;
using TaskManager.Providers;
using TaskManager.Services;
using TaskManager.ViewModels.Abstracts;
using TaskManager.Views.Windows;

namespace TaskManager.AppInitializers;

internal static class DependencyInjectionInitializer
{
    internal static ServiceProvider DependencyInjectionInitialize(Dispatcher dispatcher)
    {
        ServiceCollection services = new ServiceCollection();

        #region UI core

        services.AddSingleton<IAppConfigProvider, AppConfigProvider>();
        services.AddSingleton<INavigationService , NavigationService >();

        #endregion

        services.AddTransient<MainWindow>();
        
        services.AddSingleton<IUiDispatcher>(
            new UiDispatcher(dispatcher));

        return services.BuildServiceProvider();
    }
}