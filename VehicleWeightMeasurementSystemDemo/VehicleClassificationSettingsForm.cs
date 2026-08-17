using VehicleWeightMeasurementSystemDemo.ApplicationLayer.Configuration;
using VehicleWeightMeasurementSystemDemo.Theming;

namespace VehicleWeightMeasurementSystemDemo
{
    public partial class VehicleClassificationSettingsForm : Form
    {
        public VehicleClassificationSettings Settings { get; private set; }

        public VehicleClassificationSettingsForm(VehicleClassificationSettings settings)
        {
            InitializeComponent();
            Style();

            LoadSettings(settings);
        }

        private void LoadSettings(VehicleClassificationSettings settings)
        {
            nudSedan.Value = Clamp(settings.SedanMaxWeight);
            nudPickup.Value = Clamp(settings.PickupMaxWeight);
            nudLightTruck.Value = Clamp(settings.LightTruckMaxWeight);
            nudTruck3.Value = Clamp(settings.Truck3MaxWeight);
            nudTruck4.Value = Clamp(settings.Truck4MaxWeight);

            nudExpectedDirection.Value = settings.ExpectedDirection == 2 ? 2 : 1;
            nudMinConfidence.Value = (decimal)Math.Clamp(settings.MinConfidence, 0.0f, 1.0f);

            dgvClassWeights.Rows.Clear();
            for (int cls = 1; cls <= 14; cls++)
            {
                double max = settings.MaxAllowedWeightByClass.TryGetValue(cls, out var v) ? v : 0;
                dgvClassWeights.Rows.Add(cls, max);
            }
        }

        private void Style()
        {
            this.BackColor = UITheme.Background;
            this.Font = new Font("Segoe UI", 10);

            foreach (var grp in new[] { grpThresholds, grpDirection, grpClasses })
            {
                grp.BackColor = UITheme.CardBack;
                grp.ForeColor = Color.Black;
                grp.Font = new Font("Segoe UI", 10, FontStyle.Bold);

                foreach (var label in grp.Controls.OfType<Label>())
                {
                    label.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
                    label.ForeColor = Color.FromArgb(30, 30, 30);
                }
            }

            dgvClassWeights.BackgroundColor = Color.White;
            dgvClassWeights.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F);

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

        private static decimal Clamp(double value)
        {
            var max = (double)decimal.MaxValue;
            var v = Math.Clamp(value, 0, max);
            return (decimal)v;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgvClassWeights.Rows)
            {
                if (row.Cells[colMax.Index].Value is not string text || !double.TryParse(text, out var val) || val < 0)
                {
                    MessageBox.Show(this,
                        $"Invalid max weight for class {row.Cells[colClass.Index].Value}.\nAll values must be non-negative numbers.",
                        "Settings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            var weights = new Dictionary<int, double>();
            foreach (DataGridViewRow row in dgvClassWeights.Rows)
            {
                var cls = Convert.ToInt32(row.Cells[colClass.Index].Value);
                var val = Convert.ToDouble(row.Cells[colMax.Index].Value);
                weights[cls] = val;
            }

            Settings = new VehicleClassificationSettings
            {
                SedanMaxWeight = (double)nudSedan.Value,
                PickupMaxWeight = (double)nudPickup.Value,
                LightTruckMaxWeight = (double)nudLightTruck.Value,
                Truck3MaxWeight = (double)nudTruck3.Value,
                Truck4MaxWeight = (double)nudTruck4.Value,
                ExpectedDirection = (byte)nudExpectedDirection.Value,
                MinConfidence = (float)nudMinConfidence.Value,
                MaxAllowedWeightByClass = weights
            };

            DialogResult = DialogResult.OK;
        }
    }
}