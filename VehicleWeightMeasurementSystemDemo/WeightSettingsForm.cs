using VehicleWeightMeasurementSystemDemo.ApplicationLayer.Configuration;
using VehicleWeightMeasurementSystemDemo.Theming;

namespace VehicleWeightMeasurementSystemDemo
{
    public partial class WeightSettingsForm : Form
    {
        public decimal Alpha => nudAlpha.Value;

        public WeightSettingsForm(WeightSettings settings)
        {
            InitializeComponent();
            Style();

            nudAlpha.Value = settings.Alpha;
        }

        private void Style()
        {
            this.BackColor = UITheme.Background;
            this.Font = new Font("Segoe UI", 10);

            grpWeight.BackColor = UITheme.CardBack;
            grpWeight.ForeColor = Color.Black;
            grpWeight.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            foreach (var label in grpWeight.Controls.OfType<Label>())
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
            if (nudAlpha.Value <= 0)
            {
                MessageBox.Show(this, "Alpha must be greater than zero.",
                    "Settings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult = DialogResult.OK;
        }
    }
}
