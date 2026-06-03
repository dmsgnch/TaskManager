using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using TaskManager.Core.ApiClients;
using TaskManager.Core.ApiClients.Abstracts;
using TaskManager.Core.Providers.Abstracts;
using TaskManager.Core.Services;
using TaskManager.Core.Services.Abstracts;
using TaskManager.Providers;
using TaskManager.Resources.Core;
using TaskManager.Services;
using TaskManager.ViewModels.Abstracts;
using TaskManager.ViewModels.Dialogs;
using TaskManager.ViewModels.Windows;
using TaskManager.Views.Windows;

namespace TaskManager.AppInitializers;

internal static class DependencyInjectionInitializer
{
    private const string JsonPlaceholderUri = "https://jsonplaceholder.typicode.com";

    internal static ServiceProvider DependencyInjectionInitialize(Dispatcher dispatcher)
    {
        ServiceCollection services = new ServiceCollection();

        services.AddSingleton<IAppConfigProvider, AppConfigProvider>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton(GetApplicationResource<LocalizationManager>("LocalizationManager"));
        services.AddSingleton<ILocalizationService, LocalizationService>();

        services.AddHttpClient<ITaskApiClient, JsonPlaceholderTaskApiClient>(client =>
        {
            client.BaseAddress = new Uri(JsonPlaceholderUri);
        });

        services.AddSingleton<ITaskService, TaskService>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<ITaskEditDialogViewModelFactory, TaskEditDialogViewModelFactory>();

        services.AddSingleton<MainWindow>();

        services.AddSingleton<MainWindowViewModel>();

        services.AddSingleton<IUiDispatcher>(
            new UiDispatcher(dispatcher));

        return services.BuildServiceProvider();
    }

    private static T GetApplicationResource<T>(string key)
        where T : class
    {
        return Application.Current.Resources[key] as T
               ?? throw new InvalidOperationException($"Application resource '{key}' was not found.");
    }
}
