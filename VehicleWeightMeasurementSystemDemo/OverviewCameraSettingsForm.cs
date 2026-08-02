using VehicleWeightMeasurementSystemDemo.ApplicationLayer.Configuration;
using VehicleWeightMeasurementSystemDemo.Theming;

namespace VehicleWeightMeasurementSystemDemo
{
    public partial class OverviewCameraSettingsForm : Form
    {
        private readonly OverviewCameraSettings _settings;

        public OverviewCameraSettings Settings => new()
        {
            Host = txtHost.Text.Trim(),
            PictureUrl = txtPictureUrl.Text.Trim(),
            Username = txtUsername.Text.Trim(),
            Password = txtPassword.Text,
            Enabled = chkEnabled.Checked,
            RefreshIntervalMs = (int)nudRefresh.Value
        };

        public OverviewCameraSettingsForm(OverviewCameraSettings settings)
        {
            InitializeComponent();
            Style();

            _settings = settings;

            txtHost.Text = settings.Host;
            txtPictureUrl.Text = settings.PictureUrl;
            txtUsername.Text = settings.Username;
            txtPassword.Text = settings.Password;
            nudRefresh.Value = settings.RefreshIntervalMs;
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

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtHost.Text) ||
                string.IsNullOrWhiteSpace(txtPictureUrl.Text))
            {
                MessageBox.Show(this, "Please fill in the host and picture URL.",
                    "Settings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult = DialogResult.OK;
        }
    }
}
