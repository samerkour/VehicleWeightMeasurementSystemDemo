using System.Data.SqlClient;
using Microsoft.Data.SqlClient;
using VehicleWeightMeasurementSystemDemo.Theming;

namespace VehicleWeightMeasurementSystemDemo
{
    public partial class DatabaseSettingsForm : Form
    {
        private readonly SqlConnectionStringBuilder _builder;

        public string ConnectionString => _builder.ConnectionString;

        public DatabaseSettingsForm(string connectionString)
        {
            InitializeComponent();
            Style();

            _builder = new SqlConnectionStringBuilder(connectionString);

            txtServer.Text = _builder.DataSource;
            txtDatabase.Text = _builder.InitialCatalog;
            txtUsername.Text = _builder.UserID;
            txtPassword.Text = _builder.Password;
            chkTrusted.Checked = _builder.IntegratedSecurity;
            chkTrustServerCertificate.Checked = _builder.TrustServerCertificate;

            UpdateCredentialUi();
            RefreshPreview();
        }

        private void Style()
        {
            this.BackColor = UITheme.Background;
            this.Font = new Font("Segoe UI", 10);

            grpDb.BackColor = UITheme.CardBack;
            grpDb.ForeColor = Color.Black;
            grpDb.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            foreach (var label in grpDb.Controls.OfType<Label>())
            {
                label.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
                label.ForeColor = Color.FromArgb(30, 30, 30);
            }

            StyleButton(btnSave, UITheme.Success);
            StyleButton(btnCancel, Color.FromArgb(120, 120, 120));
            StyleButton(btnTest, Color.FromArgb(33, 150, 243));
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

        private void RefreshPreview()
        {
            _builder.DataSource = txtServer.Text.Trim();
            _builder.InitialCatalog = txtDatabase.Text.Trim();
            _builder.IntegratedSecurity = chkTrusted.Checked;
            _builder.TrustServerCertificate = chkTrustServerCertificate.Checked;

            if (!chkTrusted.Checked)
            {
                _builder.UserID = txtUsername.Text.Trim();
                _builder.Password = txtPassword.Text;
            }
            else
            {
                _builder.Remove("User ID");
                _builder.Remove("Password");
            }

            txtConnectionString.Text = _builder.ConnectionString;
        }

        private void chkTrusted_CheckedChanged(object sender, EventArgs e)
        {
            UpdateCredentialUi();
            RefreshPreview();
        }

        private void UpdateCredentialUi()
        {
            bool enabled = !chkTrusted.Checked;
            txtUsername.Enabled = enabled;
            txtPassword.Enabled = enabled;
            lblUsername.Enabled = enabled;
            lblPassword.Enabled = enabled;
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            RefreshPreview();

            try
            {
                using var conn = new SqlConnection(_builder.ConnectionString);
                conn.Open();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT 1";
                cmd.ExecuteScalar();

                MessageBox.Show(this, "Connection successful.", "Database",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Connection failed: {ex.Message}", "Database",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtServer.Text) ||
                string.IsNullOrWhiteSpace(txtDatabase.Text))
            {
                MessageBox.Show(this, "Please fill in the server and database name.",
                    "Settings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            RefreshPreview();
            DialogResult = DialogResult.OK;
        }
    }
}