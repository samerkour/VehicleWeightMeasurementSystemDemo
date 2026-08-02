using VehicleWeightMeasurementSystemDemo.ApplicationLayer.Configuration;
using VehicleWeightMeasurementSystemDemo.Theming;

namespace VehicleWeightMeasurementSystemDemo
{
    public partial class SnapshotCameraSettingsForm : Form
    {
        private readonly SnapshotCameraSettings _settings;

        public SnapshotCameraSettings Settings => new()
        {
            WatchRootPath = txtWatchRoot.Text.Trim(),
            Filter = txtFilter.Text.Trim(),
            IncludeSubfolders = chkIncludeSubfolders.Checked,
            Enabled = chkEnabled.Checked,
            ImageLookbackMs = (int)nudLookback.Value,
            ImageWaitTimeoutMs = (int)nudWaitTimeout.Value
        };

        public SnapshotCameraSettingsForm(SnapshotCameraSettings settings)
        {
            InitializeComponent();
            Style();

            _settings = settings;

            txtWatchRoot.Text = settings.WatchRootPath;
            txtFilter.Text = settings.Filter;
            nudLookback.Value = settings.ImageLookbackMs;
            nudWaitTimeout.Value = settings.ImageWaitTimeoutMs;
            chkIncludeSubfolders.Checked = settings.IncludeSubfolders;
            chkEnabled.Checked = settings.Enabled;
        }

        private void Style()
        {
            this.BackColor = UITheme.Background;
            this.Font = new Font("Segoe UI", 10);

            grpCamera.BackColor = UITheme.CardBack;
            grpCamera.ForeColor = Color.Black;
            grpCamera.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            foreach (var label in grpCamera.Controls.OfType<Label>())
            {
                label.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
                label.ForeColor = Color.FromArgb(30, 30, 30);
            }

            StyleButton(btnSave, UITheme.Success);
            StyleButton(btnCancel, Color.FromArgb(120, 120, 120));
            StyleButton(btnBrowse, Color.FromArgb(120, 120, 120));
        }

        private void StyleButton(Button button, Color back)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.BackColor = back;
            button.ForeColor = Color.White;
            button.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            button.Cursor = Cursors.Hand;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using var fbd = new FolderBrowserDialog
            {
                Description = "Select the snapshot watch folder",
                SelectedPath = txtWatchRoot.Text
            };

            if (fbd.ShowDialog(this) == DialogResult.OK)
                txtWatchRoot.Text = fbd.SelectedPath;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtWatchRoot.Text) ||
                string.IsNullOrWhiteSpace(txtFilter.Text))
            {
                MessageBox.Show(this, "Please fill in the watch folder and filter.",
                    "Settings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult = DialogResult.OK;
        }
    }
}
