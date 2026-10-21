using System.Threading;
using System.Windows.Forms;
using RmtoSync.Ui;
using Xunit;

namespace RmtoSync.Tests;

/// <summary>
/// Smoke test for the WinForms designer file: the parameterless constructor used by Visual
/// Studio must build the form and its controls without touching configuration or the service.
/// </summary>
public sealed class MainFormDesignerTests
{
    [Fact]
    public void MainForm_designer_builds_the_expected_controls()
    {
        Exception? error = null;
        var title = string.Empty;
        var columnCount = -1;
        var statusHeight = -1;

        var thread = new Thread(() =>
        {
            try
            {
                using var form = new MainForm();
                title = form.Text;

                var grid = form.Controls.Find("_recentGrid", searchAllChildren: true);
                if (grid.Length == 1 && grid[0] is DataGridView dataGrid)
                    columnCount = dataGrid.ColumnCount;

                var status = form.Controls.Find("_statusLabel", searchAllChildren: true);
                if (status.Length == 1)
                    statusHeight = status[0].Height;
            }
            catch (Exception ex)
            {
                error = ex;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(30)), "MainForm construction timed out.");

        Assert.Null(error);
        Assert.Equal("Rmto Sync", title);
        Assert.Equal(6, columnCount);
        Assert.True(statusHeight > 0);
    }
}
