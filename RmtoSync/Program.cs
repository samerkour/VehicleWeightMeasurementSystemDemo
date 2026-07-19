using RmtoSync.Ui;
using RmtoSync;
using Farasoo.AppGuard;

AppCrashGuard.Configure(new AppCrashGuardOptions
{
    AppId = "RmtoSync",
    MainWindowTitle = "Rmto Sync"
});

if (AppCrashGuard.IsWatchdogMode(args))
{
    AppCrashGuard.RunWatchdogLoop(args);
    return;
}

var guarded = AppCrashGuard.ShouldGuardProcess(args);
if (guarded)
{
    AppCrashGuard.Initialize();
    if (!AppCrashGuard.TryAcquireWorkerInstance())
        Environment.Exit(AppCrashGuard.ExitCodeAlreadyRunning);
    AppCrashGuard.EnsureWatchdogProcess();
}

try
{
    var runHeadless = args.Contains("--service", StringComparer.OrdinalIgnoreCase)
        || args.Contains("--console", StringComparer.OrdinalIgnoreCase)
        || (!Environment.UserInteractive && !args.Contains("--gui", StringComparer.OrdinalIgnoreCase));

    if (runHeadless)
    {
        Environment.Exit(await HostBootstrap.RunAsync(args));
    }

    ApplicationConfiguration.Initialize();
    Application.Run(new MainForm(args));
}
catch (Exception ex) when (guarded)
{
    AppCrashGuard.LogException(ex, "Main");
    AppCrashGuard.Log("Fatal startup/run error: " + ex.Message);
    Environment.Exit(1);
}
finally
{
    if (guarded)
        AppCrashGuard.ReleaseWorkerInstance();
}
