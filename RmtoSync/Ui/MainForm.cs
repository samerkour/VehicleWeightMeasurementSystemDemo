using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RmtoSync.Its;
using RmtoSync.Services;
using Serilog;

namespace RmtoSync.Ui;

public sealed class MainForm : Form
{
    private readonly string[] _args;
    private IHost? _host;
    private RahdariSyncStatus? _syncStatus;
    private bool _starting;
    private bool _stopping;

    private readonly Label _statusLabel;
    private readonly Label _lastResultLabel;
    private readonly Button _startStopButton;
    private readonly Button _lastErrorButton;
    private readonly Button _logsButton;
    private readonly Button _exitButton;

    public MainForm(string[] args)
    {
        _args = args;

        Text = "Rmto Sync";
        RightToLeft = RightToLeft.No;
        RightToLeftLayout = false;
        Font = new Font("Segoe UI", 9F);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;
        MinimizeBox = true;
        WindowState = FormWindowState.Maximized;
        ClientSize = new Size(520, 200);

        var titleLabel = new Label
        {
            Text = "Send records to Rahdari terminal",
            AutoSize = false,
            Dock = DockStyle.Top,
            Height = 36,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold)
        };

        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 48,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(16, 8, 16, 8),
            WrapContents = false
        };

        _statusLabel = new Label
        {
            Text = "Ready to start service.",
            AutoSize = false,
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.FixedSingle,
            Padding = new Padding(8),
            TextAlign = ContentAlignment.TopLeft
        };

        _lastResultLabel = new Label
        {
            Text = "نتیجه راهداری: نامشخص",
            AutoSize = false,
            Dock = DockStyle.Bottom,
            Height = 48,
            BorderStyle = BorderStyle.FixedSingle,
            Padding = new Padding(8),
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold)
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

        Controls.Add(_statusLabel);
        Controls.Add(_lastResultLabel);
        Controls.Add(buttonPanel);
        Controls.Add(titleLabel);

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

            //if (!e.Cancel)
            //    AppCrashGuard.MarkNormalShutdown();
        };

        Load += async (_, _) =>
        {
            UpdateStatusText();
            if (ShouldAutoStartServiceOnLaunch())
                await StartServiceAsync();
        };
    }

    private bool ShouldAutoStartServiceOnLaunch()
    {
        try
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();
            return config.GetValue("RmtoSync:AutoStartServiceOnLaunch", true);
        }
        catch
        {
            return true;
        }
    }

    private void UpdateStatusText()
    {
        try
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var sql = HostBootstrap.TryGetDataSource(config.GetConnectionString("FarasooCamera"));
            var rahdariUrl = config["Rahdari:ServiceUrl"] ?? "(not set)";
            var pollMs = config.GetValue("RmtoSync:PollIntervalMs", 2000);

            _statusLabel.Text =
                $"SQL Server: {sql}\r\n" +
                $"Rahdari: {rahdariUrl}\r\n" +
                $"Send interval: {pollMs} ms\r\n" +
                $"Status: {(_host is null ? "Stopped" : "Running")}";
        }
        catch (Exception ex)
        {
            _statusLabel.Text = $"Error reading appsettings.json:\r\n{ex.Message}";
        }
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
            await _host.StartAsync();
            _startStopButton.Text = "Stop service";
            UpdateLastResultLabel();
            UpdateStatusText();
        }
        catch (Exception ex)
        {
            _host?.Dispose();
            _host = null;
            _syncStatus = null;
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
            BeginInvoke(UpdateLastResultLabel);
        }
        catch (ObjectDisposedException)
        {
            // form closing
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
            var code = last.Explanation.ErrorCode.HasValue ? $" [{last.Explanation.ErrorCode}]" : string.Empty;
            detail = $"{last.Timestamp:HH:mm:ss}{code} — {last.Explanation.Summary}";
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
            if (_syncStatus is not null)
                _syncStatus.Changed -= OnSyncStatusChanged;
            _host?.Dispose();
        }

        base.Dispose(disposing);
    }
}
