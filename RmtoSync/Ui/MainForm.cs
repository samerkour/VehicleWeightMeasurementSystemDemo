using System.Diagnostics;
using System.Globalization;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RmtoSync.Data;
using RmtoSync.Its;
using RmtoSync.Services;
using Serilog;

namespace RmtoSync.Ui;

public sealed class MainForm : Form
{
    private readonly string[] _args;
    private readonly IConfigurationRoot? _config;
    private readonly string? _connectionString;

    private IHost? _host;
    private RahdariSyncStatus? _syncStatus;
    private bool _starting;
    private bool _stopping;
    private DateTime? _startedAt;
    private QueueStatusSnapshot? _snapshot;
    private bool _snapshotLoading;

    private readonly Label _statusLabel;
    private readonly Label _lastResultLabel;
    private readonly DataGridView _recentGrid;
    private readonly Button _startStopButton;
    private readonly Button _lastErrorButton;
    private readonly Button _logsButton;
    private readonly Button _exitButton;
    private readonly System.Windows.Forms.Timer _clockTimer;
    private readonly System.Windows.Forms.Timer _queueTimer;

    public MainForm(string[] args)
    {
        _args = args;
        _config = TryLoadConfig();
        _connectionString = _config?.GetConnectionString("DefaultConnection");

        Text = "Rmto Sync";
        RightToLeft = RightToLeft.No;
        RightToLeftLayout = false;
        Font = new Font("Segoe UI", 9F);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;
        MinimizeBox = true;
        WindowState = FormWindowState.Maximized;
        ClientSize = new Size(900, 560);

        var titleLabel = new Label
        {
            Text = "Send records to Rahdari terminal",
            AutoSize = false,
            Dock = DockStyle.Top,
            Height = 38,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold)
        };

        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 52,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(16, 10, 16, 10),
            WrapContents = false
        };

        _statusLabel = new Label
        {
            Text = "آماده شروع سرویس...",
            AutoSize = false,
            Dock = DockStyle.Top,
            Height = 176,
            BorderStyle = BorderStyle.FixedSingle,
            Padding = new Padding(10, 8, 10, 8),
            TextAlign = ContentAlignment.TopLeft,
            BackColor = Color.FromArgb(247, 249, 252),
            ForeColor = Color.FromArgb(33, 37, 41),
            Font = new Font("Segoe UI", 9.5F)
        };

        _lastResultLabel = new Label
        {
            Text = "نتیجه راهداری: نامشخص",
            AutoSize = false,
            Dock = DockStyle.Bottom,
            Height = 64,
            BorderStyle = BorderStyle.FixedSingle,
            Padding = new Padding(10, 6, 10, 6),
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold)
        };

        _recentGrid = BuildRecentGrid();
        _recentGrid.CellDoubleClick += (_, e) =>
        {
            if (e.RowIndex >= 0 && _recentGrid.Rows[e.RowIndex].Tag is RahdariSyncEvent entry)
                ShowRecentEventDialog(entry);
        };

        _startStopButton = new Button
        {
            Text = "Start service",
            AutoSize = true,
            MinimumSize = new Size(150, 32),
            Margin = new Padding(8, 0, 0, 0)
        };
        _startStopButton.Click += async (_, _) => await ToggleServiceAsync();

        _logsButton = new Button
        {
            Text = "Logs folder",
            AutoSize = true,
            MinimumSize = new Size(150, 32),
            Margin = new Padding(8, 0, 0, 0)
        };
        _logsButton.Click += (_, _) => OpenLogsFolder();

        _lastErrorButton = new Button
        {
            Text = "نامشخص",
            AutoSize = true,
            MinimumSize = new Size(150, 32),
            Margin = new Padding(8, 0, 0, 0),
            FlatStyle = FlatStyle.Flat,
            UseVisualStyleBackColor = false,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold)
        };
        _lastErrorButton.FlatAppearance.BorderSize = 1;
        _lastErrorButton.Click += (_, _) => ShowLastRahdariResultDialog();
        ApplyRahdariResultVisual(RahdariOutcomeKind.Unknown);

        _exitButton = new Button
        {
            Text = "Exit",
            AutoSize = true,
            MinimumSize = new Size(150, 32),
            Margin = new Padding(8, 0, 0, 0)
        };
        _exitButton.Click += (_, _) => Close();

        buttonPanel.Controls.Add(_startStopButton);
        buttonPanel.Controls.Add(_lastErrorButton);
        buttonPanel.Controls.Add(_logsButton);
        buttonPanel.Controls.Add(_exitButton);

        Controls.Add(_recentGrid);
        Controls.Add(_lastResultLabel);
        Controls.Add(buttonPanel);
        Controls.Add(_statusLabel);
        Controls.Add(titleLabel);

        _clockTimer = new System.Windows.Forms.Timer { Interval = 1000 };
        _clockTimer.Tick += (_, _) => UpdateStatusText();

        _queueTimer = new System.Windows.Forms.Timer { Interval = 30_000 };
        _queueTimer.Tick += async (_, _) => await RefreshSnapshotAsync();

        FormClosing += async (_, e) =>
        {
            if (_host is not null && !_stopping)
            {
                var answer = MessageBox.Show(
                    "Service is running. Stop it before exit?",
                    "Confirm exit",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1);

                if (answer == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }

                if (answer == DialogResult.Yes)
                {
                    e.Cancel = true;
                    await StopServiceAsync();
                    Close();
                    return;
                }
            }
        };

        Load += async (_, _) =>
        {
            UpdateStatusText();
            _clockTimer.Start();
            _queueTimer.Start();
            await RefreshSnapshotAsync();

            if (ShouldAutoStartServiceOnLaunch())
                await StartServiceAsync();
        };
    }

    private static IConfigurationRoot? TryLoadConfig()
    {
        try
        {
            return new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to read appsettings.json");
            return null;
        }
    }

    private static DataGridView BuildRecentGrid()
    {
        var grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,
            AutoGenerateColumns = false,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            GridColor = Color.FromArgb(222, 226, 230),
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
            EnableHeadersVisualStyles = false,
            RowTemplate = { Height = 26 }
        };

        grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(33, 37, 41);
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        grid.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
        grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(200, 230, 201);
        grid.DefaultCellStyle.SelectionForeColor = Color.Black;
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 249, 250);

        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Time",
            HeaderText = "زمان",
            Width = 72
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Result",
            HeaderText = "نتیجه",
            Width = 130
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Operation",
            HeaderText = "عملیات",
            Width = 120
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "PhotoId",
            HeaderText = "PhotoId",
            Width = 82
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Code",
            HeaderText = "کد",
            Width = 55
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Summary",
            HeaderText = "توضیح",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            MinimumWidth = 200
        });

        return grid;
    }

    private bool ShouldAutoStartServiceOnLaunch()
    {
        try
        {
            return _config?.GetValue("RmtoSync:AutoStartServiceOnLaunch", true) ?? true;
        }
        catch
        {
            return true;
        }
    }

    private void UpdateStatusText()
    {
        if (_statusLabel.IsDisposed)
            return;

        if (_config is null)
        {
            _statusLabel.Text = "تنظیمات (appsettings.json) خوانده نشد — سرویس قابل شروع نیست.";
            return;
        }

        var status = _syncStatus;
        var builder = new StringBuilder();

        var state = _host is null
            ? "متوقف"
            : _startedAt is { } started
                ? $"در حال اجرا از {started:HH:mm:ss} ({FormatAgo(started)})"
                : "در حال اجرا";

        builder.AppendLine($"وضعیت سرویس: {state}");
        builder.AppendLine($"پایگاه‌داده: {HostBootstrap.TryGetDataSource(_connectionString)}");
        builder.AppendLine($"راهداری: {_config["Rahdari:ServiceUrl"] ?? "(تنظیم نشده)"}");

        var poll = _config.GetValue("RmtoSync:PollIntervalMs", 2000);
        var batch = _config.GetValue("RmtoSync:BatchSize", 50);
        var maxPerCycle = _config.GetValue("RmtoSync:MaxRecordsPerCycle", 200);
        var concurrency = _config.GetValue("RmtoSync:MaxConcurrentSends", 4);
        builder.AppendLine(
            $"چرخه: هر {poll} ms | batch {batch} | حداکثر {maxPerCycle} رکورد در چرخه | هم‌زمانی {concurrency}");

        if (status?.LastCycleAt is { } cycleAt)
        {
            builder.AppendLine(
                $"آخرین چرخه: {cycleAt:HH:mm:ss} ({FormatAgo(cycleAt)}) — " +
                $"{status.LastCycleProcessed} رکورد در {status.LastCycleDuration.TotalSeconds:0.0} ثانیه | " +
                $"تعداد چرخه‌ها: {status.CycleCount}");
        }
        else
        {
            builder.AppendLine("آخرین چرخه: — (سرویس شروع نشده)");
        }

        if (_snapshot is { } queue)
        {
            builder.AppendLine(
                $"قابل ارسال: {queue.ReadyTto:N0} TTO | {queue.ReadyImages:N0} عکس");
            builder.AppendLine(
                $"کل صف: {queue.PendingTto:N0} در انتظار TTO | {queue.PendingImages:N0} در انتظار عکس | " +
                $"{queue.PendingWithError:N0} با خطای قابل تلاش | {queue.Abandoned:N0} رهاشده");
            builder.AppendLine(
                $"ارسال: امروز {queue.SentToday:N0} | آخرین ارسال موفق {FormatAgo(queue.LastSentAt)} | " +
                $"این نشست: {(status?.SentCount ?? 0):N0} موفق، {(status?.FailedCount ?? 0):N0} خطا");
            builder.AppendLine(
                $"خواندن صف: {queue.CapturedAt:HH:mm:ss} ({FormatAgo(queue.CapturedAt)})");
        }
        else
        {
            builder.AppendLine("صف: در دسترس نیست (پایگاه‌داده یا اسکیمای CameraPhotos)");
        }

        _statusLabel.Text = builder.ToString().TrimEnd();
    }

    private static string FormatAgo(DateTime? value)
    {
        if (value is null)
            return "—";

        var seconds = Math.Max(0, (int)(DateTime.Now - value.Value).TotalSeconds);
        if (seconds < 60)
            return $"{seconds} ثانیه پیش";

        var minutes = seconds / 60;
        if (minutes < 60)
            return $"{minutes} دقیقه پیش";

        return $"{minutes / 60} ساعت و {minutes % 60} دقیقه پیش";
    }

    private async Task RefreshSnapshotAsync()
    {
        if (_snapshotLoading)
            return;

        _snapshotLoading = true;
        try
        {
            if (_config is null)
            {
                _snapshot = null;
            }
            else
            {
                var maxAttempts = _config.GetValue("RmtoSync:MaxSendAttempts", 3);
                var backoffSeconds = Math.Max(_config.GetValue("RmtoSync:RetryBackoffSeconds", 30), 0);
                var requireSensorData = !_config.GetValue("Rahdari:AnprOnlyStation", true);

                _snapshot = await QueueStatusReader.ReadAsync(
                    _connectionString,
                    maxAttempts,
                    TimeSpan.FromSeconds(backoffSeconds),
                    requireSensorData,
                    CancellationToken.None);
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Queue status snapshot failed");
            _snapshot = null;
        }
        finally
        {
            _snapshotLoading = false;
        }

        UpdateStatusText();
    }

    private async Task ToggleServiceAsync()
    {
        if (_host is null)
            await StartServiceAsync();
        else
            await StopServiceAsync();
    }

    private async Task StartServiceAsync()
    {
        if (_host is not null || _starting)
            return;

        _starting = true;
        SetButtonsEnabled(false);
        _statusLabel.Text = "Starting service...";

        try
        {
            _host = HostBootstrap.Build(_args);
            _syncStatus = _host.Services.GetRequiredService<RahdariSyncStatus>();
            _syncStatus.Changed += OnSyncStatusChanged;
            _startedAt = DateTime.Now;
            _recentGrid.Rows.Clear();

            await _host.StartAsync();
            _startStopButton.Text = "Stop service";
            UpdateLastResultLabel();
            RefreshResultGrid();
            UpdateStatusText();
        }
        catch (Exception ex)
        {
            _host?.Dispose();
            _host = null;
            _syncStatus = null;
            _startedAt = null;
            Log.Fatal(ex, "Failed to start RmtoSync");
            MessageBox.Show(
                $"Failed to start service:\n\n{ex.Message}",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1);
            UpdateStatusText();
        }
        finally
        {
            _starting = false;
            SetButtonsEnabled(true);
        }
    }

    private async Task StopServiceAsync()
    {
        if (_host is null || _stopping)
            return;

        _stopping = true;
        SetButtonsEnabled(false);
        _statusLabel.Text = "Stopping service...";

        try
        {
            if (_syncStatus is not null)
                _syncStatus.Changed -= OnSyncStatusChanged;

            await _host.StopAsync(TimeSpan.FromSeconds(15));
            _host.Dispose();
            _host = null;
            _syncStatus = null;
            _startedAt = null;
            _startStopButton.Text = "Start service";
            ApplyRahdariResultVisual(RahdariOutcomeKind.Unknown, "سرویس متوقف است");
            UpdateStatusText();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Failed to stop service:\n\n{ex.Message}",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button1);
        }
        finally
        {
            _stopping = false;
            SetButtonsEnabled(true);
        }
    }

    private void SetButtonsEnabled(bool enabled)
    {
        _startStopButton.Enabled = enabled;
        _lastErrorButton.Enabled = enabled;
        _logsButton.Enabled = enabled;
        _exitButton.Enabled = enabled;
    }

    private void OnSyncStatusChanged()
    {
        if (IsDisposed)
            return;

        try
        {
            BeginInvoke(() =>
            {
                UpdateLastResultLabel();
                RefreshResultGrid();
                UpdateStatusText();
            });
        }
        catch (ObjectDisposedException)
        {
            // form closing
        }
        catch (InvalidOperationException)
        {
            // handle not created / form closing
        }
    }

    private void UpdateLastResultLabel()
    {
        var last = _syncStatus?.LastEvent;
        var kind = RahdariOutcomeClassifier.Classify(
            last?.Explanation,
            last?.IsSuccess ?? false,
            last is not null,
            _host is not null);

        string? detail = null;
        if (last is not null)
        {
            var parts = new List<string> { last.Timestamp.ToString("HH:mm:ss") };
            if (!string.IsNullOrWhiteSpace(last.Explanation.Operation))
                parts.Add(last.Explanation.Operation!);
            if (last.Explanation.PhotoId is { } photoId)
                parts.Add($"PhotoId {photoId}");
            if (last.Explanation.ErrorCode is { } code)
                parts.Add($"کد {code}");

            detail = string.Join(" | ", parts) + Environment.NewLine + last.Explanation.Summary;
        }
        else if (kind == RahdariOutcomeKind.Waiting)
        {
            detail = "سرویس فعال — منتظر پاسخ ترمینال";
        }
        else if (_host is null)
        {
            detail = "سرویس هنوز شروع نشده";
        }

        ApplyRahdariResultVisual(kind, detail);
    }

    private void ApplyRahdariResultVisual(RahdariOutcomeKind kind, string? detail = null)
    {
        var p = RahdariOutcomeClassifier.Present(kind);

        _lastErrorButton.Text = p.ButtonText;
        _lastErrorButton.BackColor = p.BackColor;
        _lastErrorButton.ForeColor = p.ForeColor;
        _lastErrorButton.FlatAppearance.BorderColor = ControlPaint.Dark(p.BackColor);

        _lastResultLabel.BackColor = Color.FromArgb(
            Math.Min(p.BackColor.R + 20, 255),
            Math.Min(p.BackColor.G + 20, 255),
            Math.Min(p.BackColor.B + 20, 255));
        _lastResultLabel.ForeColor = p.ForeColor;
        _lastResultLabel.Text = string.IsNullOrWhiteSpace(detail)
            ? $"نتیجه راهداری: {p.LabelLong} — {p.DocHint}"
            : $"نتیجه راهداری ({p.LabelLong}) — {detail}";
    }

    private void RefreshResultGrid()
    {
        if (_recentGrid.IsDisposed)
            return;

        var events = _syncStatus?.RecentEvents ?? Array.Empty<RahdariSyncEvent>();

        _recentGrid.SuspendLayout();
        try
        {
            _recentGrid.Rows.Clear();
            foreach (var entry in events)
            {
                var kind = RahdariOutcomeClassifier.Classify(
                    entry.Explanation,
                    entry.IsSuccess,
                    hasEvent: true,
                    serviceRunning: _host is not null);
                var presentation = RahdariOutcomeClassifier.Present(kind);

                var index = _recentGrid.Rows.Add(
                    entry.Timestamp.ToString("HH:mm:ss"),
                    presentation.LabelLong,
                    entry.Explanation.Operation ?? "—",
                    entry.Explanation.PhotoId?.ToString(CultureInfo.InvariantCulture) ?? "—",
                    entry.Explanation.ErrorCode?.ToString(CultureInfo.InvariantCulture) ?? "—",
                    entry.Explanation.Summary);

                var row = _recentGrid.Rows[index];
                row.Tag = entry;
                row.DefaultCellStyle.BackColor = entry.IsSuccess
                    ? Color.FromArgb(232, 245, 233)
                    : Color.FromArgb(255, 235, 238);
                row.DefaultCellStyle.ForeColor = entry.IsSuccess
                    ? Color.FromArgb(20, 60, 25)
                    : Color.FromArgb(120, 20, 25);
            }
        }
        finally
        {
            _recentGrid.ResumeLayout();
        }
    }

    private void ShowRecentEventDialog(RahdariSyncEvent entry)
    {
        var kind = RahdariOutcomeClassifier.Classify(
            entry.Explanation,
            entry.IsSuccess,
            hasEvent: true,
            serviceRunning: _host is not null);

        var icon = entry.IsSuccess ? MessageBoxIcon.Information : MessageBoxIcon.Error;
        var body =
            $"زمان: {entry.Timestamp:yyyy-MM-dd HH:mm:ss}{Environment.NewLine}{Environment.NewLine}" +
            RahdariResponseInterpreter.FormatForDialog(entry.Explanation);

        MessageBox.Show(
            body,
            RahdariOutcomeClassifier.DialogTitle(kind),
            MessageBoxButtons.OK,
            icon,
            MessageBoxDefaultButton.Button1,
            MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
    }

    private void ShowLastRahdariResultDialog()
    {
        var last = _syncStatus?.LastEvent;
        var kind = RahdariOutcomeClassifier.Classify(
            last?.Explanation,
            last?.IsSuccess ?? false,
            last is not null,
            _host is not null);
        var presentation = RahdariOutcomeClassifier.Present(kind);

        if (last is null)
        {
            MessageBox.Show(
                $"{presentation.DocHint}\n\n{(_host is null ? "سرویس را Start کنید." : "چند ثانیه صبر کنید تا اولین پاسخ ثبت شود.")}\n\nمرجع: {presentation.DocReference}",
                RahdariOutcomeClassifier.DialogTitle(kind),
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
            return;
        }

        var icon = kind switch
        {
            RahdariOutcomeKind.Accepted or RahdariOutcomeKind.AcceptedNeedsImage => MessageBoxIcon.Information,
            RahdariOutcomeKind.Waiting or RahdariOutcomeKind.Unknown => MessageBoxIcon.Warning,
            _ => MessageBoxIcon.Error
        };

        var body = RahdariResponseInterpreter.FormatForDialog(last.Explanation);
        MessageBox.Show(
            body,
            last.Explanation.Title,
            MessageBoxButtons.OK,
            icon,
            MessageBoxDefaultButton.Button1,
            MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
    }

    private void OpenLogsFolder()
    {
        var logsDir = Path.Combine(AppContext.BaseDirectory, "logs");
        Directory.CreateDirectory(logsDir);
        Process.Start(new ProcessStartInfo(logsDir) { UseShellExecute = true });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _clockTimer?.Stop();
            _clockTimer?.Dispose();
            _queueTimer?.Stop();
            _queueTimer?.Dispose();

            if (_syncStatus is not null)
                _syncStatus.Changed -= OnSyncStatusChanged;
            _host?.Dispose();
        }

        base.Dispose(disposing);
    }
}
