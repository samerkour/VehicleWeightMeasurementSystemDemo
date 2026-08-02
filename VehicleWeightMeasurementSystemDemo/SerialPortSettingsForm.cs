using System.IO.Ports;
using VehicleWeightMeasurementSystemDemo.ApplicationLayer.Configuration;
using VehicleWeightMeasurementSystemDemo.Theming;

namespace VehicleWeightMeasurementSystemDemo
{
    public partial class SerialPortSettingsForm : Form
    {
        private readonly SerialPortSettings _settings;

        public SerialPortSettings Settings => new()
        {
            PortName = cmbPortName.Text,
            BaudRate = (int)cmbBaudRate.SelectedItem,
            Parity = cmbParity.Text,
            DataBits = (int)cmbDataBits.SelectedItem,
            StopBits = cmbStopBits.Text,
            Enabled = chkEnabled.Checked
        };

        public SerialPortSettingsForm(SerialPortSettings settings)
        {
            InitializeComponent();
            Style();

            _settings = settings;

            cmbPortName.Items.AddRange(SerialPort.GetPortNames());
            if (cmbPortName.Items.Count > 0 && !cmbPortName.Items.Contains(settings.PortName))
                cmbPortName.Items.Insert(0, settings.PortName);

            cmbPortName.SelectedItem = settings.PortName;
            cmbBaudRate.SelectedItem = settings.BaudRate;
            cmbParity.SelectedItem = settings.Parity;
            cmbDataBits.SelectedItem = settings.DataBits;
            cmbStopBits.SelectedItem = settings.StopBits;
            chkEnabled.Checked = settings.Enabled;
        }

        private void Style()
        {
            this.BackColor = UITheme.Background;
            this.Font = new Font("Segoe UI", 10);

            grpPort.BackColor = UITheme.CardBack;
            grpPort.ForeColor = Color.Black;
            grpPort.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            foreach (var label in grpPort.Controls.OfType<Label>())
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
            if (cmbPortName.SelectedItem == null ||
                cmbBaudRate.SelectedItem == null ||
                cmbParity.SelectedItem == null ||
                cmbDataBits.SelectedItem == null ||
                cmbStopBits.SelectedItem == null)
            {
                MessageBox.Show(this, "Please fill in all serial port settings.",
                    "Settings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult = DialogResult.OK;
        }
    }
}
