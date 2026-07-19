namespace Farasoo.AppGuard;

public sealed class AppCrashGuardOptions
{
    public required string AppId { get; init; }

    /// <summary>WinForms <see cref="System.Windows.Forms.Form.Text"/> used to focus an already-running instance.</summary>
    public string? MainWindowTitle { get; init; }
}
