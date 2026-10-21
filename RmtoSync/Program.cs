using RmtoSync.Ui;
using RmtoSync;

// A second copy of RmtoSync would race the same pending rows and could register/send the same
// vehicle twice. Farasoo.AppGuard used to provide this guard; it was retired, so the check is
// restored here with a plain process-wide mutex.
Mutex? singleInstance = null;
var ownsSingleInstance = false;
try
{
    singleInstance = new Mutex(initiallyOwned: true, @"Global\RmtoSync.SingleInstance", out ownsSingleInstance);
}
catch (UnauthorizedAccessException)
{
    // The mutex already exists but we are not allowed to open it — treat as "already running".
    ownsSingleInstance = false;
}

if (!ownsSingleInstance)
{
    var message =
        "RmtoSync is already running. Only one instance may synchronise with Rahdari, " +
        "otherwise the same vehicle can be sent twice.";
    Console.Error.WriteLine(message);
    if (Environment.UserInteractive)
        MessageBox.Show(message, "RmtoSync", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    return;
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
catch (Exception ex)
{
    // Never swallow fatal errors silently: without this the app disappeared with no trace.
    var logPath = Path.Combine(AppContext.BaseDirectory, "logs", "fatal.log");
    try
    {
        Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
        File.AppendAllText(
            logPath,
            $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} FATAL{Environment.NewLine}{ex}{Environment.NewLine}{Environment.NewLine}");
    }
    catch
    {
        // Logging must never mask the original failure.
    }

    Console.Error.WriteLine(ex);
    if (Environment.UserInteractive)
        MessageBox.Show(ex.ToString(), "RmtoSync — fatal error", MessageBoxButtons.OK, MessageBoxIcon.Error);

    Environment.Exit(1);
}
finally
{
    if (ownsSingleInstance)
        singleInstance?.ReleaseMutex();

    singleInstance?.Dispose();
}
