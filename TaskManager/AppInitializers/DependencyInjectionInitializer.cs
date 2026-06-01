using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using TaskManager.Core.ApiClients;
using TaskManager.Core.ApiClients.Abstracts;
using TaskManager.Core.Providers.Abstracts;
using TaskManager.Core.Services;
using TaskManager.Core.Services.Abstracts;
using TaskManager.Providers;
using TaskManager.Services;
using TaskManager.ViewModels.Abstracts;
using TaskManager.Views.Windows;

namespace TaskManager.AppInitializers;

internal static class DependencyInjectionInitializer
{
    private const string JsonPlaceholderUri = "https://jsonplaceholder.typicode.com";
    
    internal static ServiceProvider DependencyInjectionInitialize(Dispatcher dispatcher)
    {
        ServiceCollection services = new ServiceCollection();

        #region UI core

        services.AddSingleton<IAppConfigProvider, AppConfigProvider>();
        services.AddSingleton<INavigationService , NavigationService >();

        #endregion
        
        services.AddHttpClient<ITaskApiClient, JsonPlaceholderTaskApiClient>(client =>
        {
            client.BaseAddress = new Uri(JsonPlaceholderUri);
        });

        services.AddSingleton<ITaskService, TaskService>();

        services.AddTransient<MainWindow>();
        
        services.AddSingleton<IUiDispatcher>(
            new UiDispatcher(dispatcher));

        return services.BuildServiceProvider();
    }
}