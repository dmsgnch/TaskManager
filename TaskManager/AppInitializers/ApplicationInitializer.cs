using System.Windows;
using CommunityToolkit.Mvvm.DependencyInjection;
using Serilog;
using TaskManager.Sinks;
using TaskManager.ViewModels.Abstracts;

namespace TaskManager.AppInitializers;

internal static class ApplicationInitializer
{
    internal static void InitializeApplication()
    {
        var serviceProvider = DependencyInjectionInitializer.DependencyInjectionInitialize(Application.Current.Dispatcher);
        
        Ioc.Default.ConfigureServices(serviceProvider);

        InitializeLogging();
    }

    private static void InitializeLogging()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            .WriteTo.Sink(new DebugSink())
            .CreateLogger();
    }

    internal static async Task ExecuteCustomInitializationsAsync()
    {
        var uiDispatcher = Ioc.Default.GetRequiredService<IUiDispatcher>();
        
        await uiDispatcher.InvokeAsync(() =>
        {
            var windowNavigationService = Ioc.Default.GetRequiredService<INavigationService>();

            windowNavigationService.ShowMain();
        });
    }
}
