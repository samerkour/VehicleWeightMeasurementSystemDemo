using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Farasoo.AppGuard;

/// <summary>
/// Global exception handling, crash logging, rate-limited auto-restart, and an external watchdog process.
/// </summary>
public static class AppCrashGuard
{
    private const string WatchdogArg = "--watchdog";
    private const string WatchdogPidArg = "--pid";
    public const int ExitCodeAlreadyRunning = 2;
    private const int WorkerStartupGraceMs = 3000;
    private const string ShutdownFlagFileName = "shutdown.requested";
    private const int MaxRestartsPerWindow = 15;
    private static readonly TimeSpan RestartWindow = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan RestartDelay = TimeSpan.FromSeconds(3);

    private static AppCrashGuardOptions _options = null!;
    private static Mutex? _workerInstanceMutex;
    private static int _restartCount;
    private static DateTime _restartWindowStart = DateTime.UtcNow;

    private static string WatchdogMutexName => $@"Global\{_options.AppId}_Watchdog_Mutex";
    private static string WorkerInstanceMutexName => $@"Global\{_options.AppId}_Worker_Instance";

    public static string LogDirectory =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            _options.AppId,
            "logs");

    public static void Configure(AppCrashGuardOptions options) =>
        _options = options ?? throw new ArgumentNullException(nameof(options));

    /// <summary>GUI worker with watchdog; not used for --console, --service, or diagnostics.</summary>
    public static bool ShouldGuardProcess(string[] args)
    {
        if (IsWatchdogMode(args))
            return false;

        if (args.Any(a => string.Equals(a, "--console", StringComparison.OrdinalIgnoreCase)
                          || string.Equals(a, "--service", StringComparison.OrdinalIgnoreCase)
                          || string.Equals(a, "--satpa-diag", StringComparison.OrdinalIgnoreCase)
                          || string.Equals(a, "--diagnose-satpa", StringComparison.OrdinalIgnoreCase)))
            return false;

        if (!Environment.UserInteractive && !args.Contains("--gui", StringComparer.OrdinalIgnoreCase))
            return false;

        return true;
    }

    public static void Initialize()
    {
        Directory.CreateDirectory(LogDirectory);

        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += OnUiThreadException;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
    }

    public static void RunWatchdogLoop(string[] args)
    {
        Directory.CreateDirectory(LogDirectory);

        Mutex? watchdogMutex = null;
        try
        {
            watchdogMutex = new Mutex(true, WatchdogMutexName, out bool created);
            if (!created)
            {
                Log("Watchdog: another instance already running.");
                return;
            }
        }
        catch (Exception ex)
        {
            Log("Watchdog: mutex error: " + ex.Message);
            return;
        }

        int? attachedWorkerPid = TryParseAttachedWorkerPid(args);
        if (attachedWorkerPid.HasValue)
            Log("Watchdog started; monitoring worker PID " + attachedWorkerPid.Value + ".");
        else
            Log("Watchdog started.");

        while (true)
        {
            if (!CanRestart())
            {
                Log("Watchdog: restart limit reached; waiting 60s.");
                Thread.Sleep(TimeSpan.FromMinutes(1));
                continue;
            }

            if (attachedWorkerPid.HasValue)
            {
                if (WaitForAttachedWorker(attachedWorkerPid.Value, out int attachedExitCode))
                {
                    Log("Attached worker exited with code " + attachedExitCode + ".");
                    attachedWorkerPid = null;

                    if (IsShutdownRequested())
                    {
                        ClearShutdownFlag();
                        Log("Watchdog: user shutdown, exiting.");
                        break;
                    }

                    if (attachedExitCode == 0)
                    {
                        Log("Watchdog: attached worker ended cleanly, exiting.");
                        break;
                    }

                    Thread.Sleep(RestartDelay);
                }

                continue;
            }

            if (IsWorkerInstanceRunning())
            {
                if (IsShutdownRequested())
                {
                    ClearShutdownFlag();
                    Log("Watchdog: user shutdown, exiting.");
                    break;
                }

                Thread.Sleep(500);
                continue;
            }

            if (IsShutdownRequested())
            {
                ClearShutdownFlag();
                Log("Watchdog: shutdown flag without worker, exiting.");
                break;
            }

            WaitForWorkerMutex(WorkerStartupGraceMs);
            if (IsWorkerInstanceRunning() || IsShutdownRequested())
                continue;

            Process? worker = null;
            try
            {
                worker = StartWorkerProcess();
                if (worker is null)
                {
                    Thread.Sleep(5000);
                    continue;
                }

                worker.WaitForExit();
                int code = worker.ExitCode;
                Log("Worker exited with code " + code + ".");

                if (code == ExitCodeAlreadyRunning)
                    continue;

                if (IsShutdownRequested())
                {
                    ClearShutdownFlag();
                    Log("Watchdog: user shutdown, exiting.");
                    break;
                }

                if (code == 0)
                {
                    Log("Watchdog: worker exited cleanly, exiting.");
                    break;
                }
            }
            catch (Exception ex)
            {
                Log("Watchdog error: " + ex);
            }

            Thread.Sleep(RestartDelay);
        }

        try { watchdogMutex?.ReleaseMutex(); } catch { }
        watchdogMutex?.Dispose();
    }

    public static bool IsWatchdogMode(string[] args) =>
        args.Length > 0 && string.Equals(args[0], WatchdogArg, StringComparison.OrdinalIgnoreCase);

    public static void EnsureWatchdogProcess()
    {
        try
        {
            using var probe = Mutex.OpenExisting(WatchdogMutexName);
            return;
        }
        catch (WaitHandleCannotBeOpenedException) { }
        catch (UnauthorizedAccessException) { return; }

        try
        {
            int workerPid = Process.GetCurrentProcess().Id;
            string arguments = WatchdogArg + " " + WatchdogPidArg + " " + workerPid;
            var psi = new ProcessStartInfo(Application.ExecutablePath, arguments)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = Application.StartupPath
            };
            Process.Start(psi);
            Log("Watchdog process launched for worker PID " + workerPid + ".");
        }
        catch (Exception ex)
        {
            Log("Failed to start watchdog: " + ex.Message);
        }
    }

    public static bool TryAcquireWorkerInstance()
    {
        try
        {
            _workerInstanceMutex = new Mutex(true, WorkerInstanceMutexName, out bool created);
            if (!created)
            {
                ActivateExistingInstance();
                return false;
            }

            ClearShutdownFlag();
            return true;
        }
        catch (Exception ex)
        {
            Log("Worker mutex error: " + ex.Message);
            return false;
        }
    }

    public static void ReleaseWorkerInstance()
    {
        try
        {
            _workerInstanceMutex?.ReleaseMutex();
            _workerInstanceMutex?.Dispose();
        }
        catch { }
        finally
        {
            _workerInstanceMutex = null;
        }
    }

    public static void ActivateExistingInstance()
    {
        var title = _options.MainWindowTitle;
        if (string.IsNullOrWhiteSpace(title))
            return;

        try
        {
            IntPtr hwnd = FindWindow(null, title);
            if (hwnd == IntPtr.Zero)
                return;
            if (IsIconic(hwnd))
                ShowWindow(hwnd, SwRestore);
            SetForegroundWindow(hwnd);
        }
        catch { }
    }

    public static void MarkNormalShutdown()
    {
        try { File.WriteAllText(ShutdownFlagPath, "1"); } catch { }
        try { Environment.ExitCode = 0; } catch { }
    }

    public static void Log(string message)
    {
        try
        {
            string path = Path.Combine(LogDirectory, "app.log");
            string line = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + message + Environment.NewLine;
            File.AppendAllText(path, line);
        }
        catch { }
    }

    public static void LogException(Exception? ex, string source)
    {
        if (ex is null) return;
        try
        {
            string path = Path.Combine(LogDirectory, "crash.log");
            string line = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " [" + source + "] " +
                          ex + Environment.NewLine + Environment.NewLine;
            File.AppendAllText(path, line);
        }
        catch { }
    }

    private static void OnUiThreadException(object sender, ThreadExceptionEventArgs e)
    {
        LogException(e.Exception, "UI");
        Log("UI thread exception (continuing): " + e.Exception.Message);
    }

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var ex = e.ExceptionObject as Exception;
        LogException(ex, "Unhandled");
        Log("Unhandled exception (terminating=" + e.IsTerminating + "): " + (ex?.Message ?? e.ExceptionObject?.ToString()));

        if (e.IsTerminating)
        {
            try { Environment.ExitCode = 1; } catch { }
        }
    }

    private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        LogException(e.Exception, "Task");
        e.SetObserved();
    }

    private static Process? StartWorkerProcess()
    {
        var psi = new ProcessStartInfo(Application.ExecutablePath)
        {
            UseShellExecute = true,
            WorkingDirectory = Application.StartupPath
        };
        return Process.Start(psi);
    }

    private static bool CanRestart()
    {
        DateTime now = DateTime.UtcNow;
        if (now - _restartWindowStart > RestartWindow)
        {
            _restartWindowStart = now;
            _restartCount = 0;
        }

        _restartCount++;
        return _restartCount <= MaxRestartsPerWindow;
    }

    private static string ShutdownFlagPath => Path.Combine(LogDirectory, ShutdownFlagFileName);

    private static bool IsWorkerInstanceRunning()
    {
        try
        {
            using var _ = Mutex.OpenExisting(WorkerInstanceMutexName);
            return true;
        }
        catch (WaitHandleCannotBeOpenedException)
        {
            return false;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsShutdownRequested() => File.Exists(ShutdownFlagPath);

    private static void ClearShutdownFlag()
    {
        try
        {
            if (File.Exists(ShutdownFlagPath))
                File.Delete(ShutdownFlagPath);
        }
        catch { }
    }

    private static int? TryParseAttachedWorkerPid(string[] args)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (string.Equals(args[i], WatchdogPidArg, StringComparison.OrdinalIgnoreCase) &&
                int.TryParse(args[i + 1], out int pid) &&
                pid > 0)
            {
                return pid;
            }
        }

        return null;
    }

    private static bool WaitForAttachedWorker(int pid, out int exitCode)
    {
        exitCode = -1;
        try
        {
            using var proc = Process.GetProcessById(pid);
            proc.WaitForExit();
            exitCode = proc.HasExited ? proc.ExitCode : -1;
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (Exception ex)
        {
            Log("Watchdog: cannot attach to PID " + pid + ": " + ex.Message);
            return false;
        }
    }

    private static void WaitForWorkerMutex(int timeoutMs)
    {
        var deadline = Environment.TickCount + timeoutMs;
        while (Environment.TickCount < deadline)
        {
            if (IsWorkerInstanceRunning() || IsShutdownRequested())
                return;
            Thread.Sleep(100);
        }
    }

    private const int SwRestore = 9;

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr FindWindow(string? lpClassName, string lpWindowName);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool IsIconic(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
}
