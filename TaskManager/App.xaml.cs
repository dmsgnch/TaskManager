using System.Windows;
using System.Windows.Threading;
using Serilog;
using TaskManager.AppInitializers;
using TaskManager.Models.Constants;

namespace TaskManager;

public partial class App
{
    private const int FailedExitCode = -1;
    private const int AnotherInstanceAlreadyRunningExitCode = 1;

    private const string UnhandledExceptionText = "Unhandled exception (App)";

    private Mutex? _singleInstanceMutex;
    private bool _ownsSingleInstanceMutex;

    protected override async void OnStartup(StartupEventArgs e)
    {
        RegisterGlobalExceptionHandlers();

        base.OnStartup(e);

        if (!TryEnsureSingleInstance())
        {
            Shutdown(AnotherInstanceAlreadyRunningExitCode);
            return;
        }

        ShutdownMode = ShutdownMode.OnMainWindowClose;

        try
        {
            ApplicationInitializer.InitializeApplication();

            await ApplicationInitializer.ExecuteCustomInitializationsAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application startup failed");
            Shutdown(FailedExitCode);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (_ownsSingleInstanceMutex)
        {
            _singleInstanceMutex?.ReleaseMutex();
        }

        _singleInstanceMutex?.Dispose();

        Log.CloseAndFlush();

        base.OnExit(e);
    }

    private void RegisterGlobalExceptionHandlers()
    {
        DispatcherUnhandledException += OnDispatcherUnhandledException;

        AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;

        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Log.Error(e.Exception, UnhandledExceptionText);

        e.Handled = true;
    }

    private void OnDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception exception)
        {
            Log.Fatal(exception, "Unhandled domain exception");
        }
        else
        {
            Log.Fatal("Unhandled domain exception: {ExceptionObject}", e.ExceptionObject);
        }
    }

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        e.SetObserved();

        Log.Error(e.Exception, "Unobserved task exception");
    }

    private bool TryEnsureSingleInstance()
    {
        _singleInstanceMutex = new Mutex(
            initiallyOwned: true,
            name: AppConstants.MutexName,
            createdNew: out bool createdNew);

        _ownsSingleInstanceMutex = createdNew;

        if (!createdNew)
        {
            _singleInstanceMutex.Dispose();
            _singleInstanceMutex = null;
        }

        return createdNew;
    }
}