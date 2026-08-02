namespace VehicleWeightMeasurementSystemDemo
{
    partial class SerialPortSettingsForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            grpPort = new GroupBox();
            lblStopBits = new Label();
            lblDataBits = new Label();
            lblParity = new Label();
            lblBaudRate = new Label();
            lblPortName = new Label();
            cmbStopBits = new ComboBox();
            cmbDataBits = new ComboBox();
            cmbParity = new ComboBox();
            cmbBaudRate = new ComboBox();
            cmbPortName = new ComboBox();
            chkEnabled = new CheckBox();
            lblHint = new Label();
            btnSave = new Button();
            btnCancel = new Button();
            grpPort.SuspendLayout();
            SuspendLayout();
            // 
            // grpPort
            // 
            grpPort.Controls.Add(lblStopBits);
            grpPort.Controls.Add(lblDataBits);
            grpPort.Controls.Add(lblParity);
            grpPort.Controls.Add(lblBaudRate);
            grpPort.Controls.Add(lblPortName);
            grpPort.Controls.Add(cmbStopBits);
            grpPort.Controls.Add(cmbDataBits);
            grpPort.Controls.Add(cmbParity);
            grpPort.Controls.Add(cmbBaudRate);
            grpPort.Controls.Add(cmbPortName);
            grpPort.Controls.Add(chkEnabled);
            grpPort.Location = new Point(12, 12);
            grpPort.Name = "grpPort";
            grpPort.Padding = new Padding(14);
            grpPort.Size = new Size(420, 310);
            grpPort.TabIndex = 0;
            grpPort.TabStop = false;
            grpPort.Text = "Serial Port";
            // 
            // lblPortName
            // 
            lblPortName.AutoSize = true;
            lblPortName.Location = new Point(22, 46);
            lblPortName.Name = "lblPortName";
            lblPortName.Size = new Size(80, 20);
            lblPortName.TabIndex = 0;
            lblPortName.Text = "Port Name";
            // 
            // lblBaudRate
            // 
            lblBaudRate.AutoSize = true;
            lblBaudRate.Location = new Point(22, 94);
            lblBaudRate.Name = "lblBaudRate";
            lblBaudRate.Size = new Size(80, 20);
            lblBaudRate.TabIndex = 0;
            lblBaudRate.Text = "Baud Rate";
            // 
            // lblParity
            // 
            lblParity.AutoSize = true;
            lblParity.Location = new Point(22, 142);
            lblParity.Name = "lblParity";
            lblParity.Size = new Size(80, 20);
            lblParity.TabIndex = 0;
            lblParity.Text = "Parity";
            // 
            // lblDataBits
            // 
            lblDataBits.AutoSize = true;
            lblDataBits.Location = new Point(22, 190);
            lblDataBits.Name = "lblDataBits";
            lblDataBits.Size = new Size(80, 20);
            lblDataBits.TabIndex = 0;
            lblDataBits.Text = "Data Bits";
            // 
            // lblStopBits
            // 
            lblStopBits.AutoSize = true;
            lblStopBits.Location = new Point(22, 238);
            lblStopBits.Name = "lblStopBits";
            lblStopBits.Size = new Size(80, 20);
            lblStopBits.TabIndex = 0;
            lblStopBits.Text = "Stop Bits";
            // 
            // cmbPortName
            // 
            cmbPortName.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPortName.Location = new Point(170, 42);
            cmbPortName.Name = "cmbPortName";
            cmbPortName.Size = new Size(200, 28);
            cmbPortName.TabIndex = 1;
            // 
            // cmbBaudRate
            // 
            cmbBaudRate.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbBaudRate.Items.AddRange(new object[] { 9600, 19200, 38400, 57600, 115200, 230400 });
            cmbBaudRate.Location = new Point(170, 90);
            cmbBaudRate.Name = "cmbBaudRate";
            cmbBaudRate.Size = new Size(200, 28);
            cmbBaudRate.TabIndex = 2;
            // 
            // cmbParity
            // 
            cmbParity.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbParity.Items.AddRange(new object[] { "None", "Odd", "Even", "Mark", "Space" });
            cmbParity.Location = new Point(170, 138);
            cmbParity.Name = "cmbParity";
            cmbParity.Size = new Size(200, 28);
            cmbParity.TabIndex = 3;
            // 
            // cmbDataBits
            // 
            cmbDataBits.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbDataBits.Items.AddRange(new object[] { 5, 6, 7, 8 });
            cmbDataBits.Location = new Point(170, 186);
            cmbDataBits.Name = "cmbDataBits";
            cmbDataBits.Size = new Size(200, 28);
            cmbDataBits.TabIndex = 4;
            // 
            // cmbStopBits
            // 
            cmbStopBits.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbStopBits.Items.AddRange(new object[] { "One", "OnePointFive", "Two" });
            cmbStopBits.Location = new Point(170, 234);
            cmbStopBits.Name = "cmbStopBits";
            cmbStopBits.Size = new Size(200, 28);
            cmbStopBits.TabIndex = 5;
            // 
            // chkEnabled
            // 
            chkEnabled.AutoSize = true;
            chkEnabled.Location = new Point(170, 278);
            chkEnabled.Name = "chkEnabled";
            chkEnabled.Size = new Size(84, 24);
            chkEnabled.TabIndex = 6;
            chkEnabled.Text = "Enabled";
            chkEnabled.UseVisualStyleBackColor = true;
            // 
            // lblHint
            // 
            lblHint.ForeColor = SystemColors.GrayText;
            lblHint.Location = new Point(12, 328);
            lblHint.Name = "lblHint";
            lblHint.Size = new Size(420, 40);
            lblHint.TabIndex = 7;
            lblHint.Text = "Changes take effect after restarting the monitoring system.";
            // 
            // btnSave
            // 
            btnSave.Location = new Point(216, 380);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(100, 34);
            btnSave.TabIndex = 8;
            btnSave.Text = "Save";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // btnCancel
            // 
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(326, 380);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(100, 34);
            btnCancel.TabIndex = 9;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // SerialPortSettingsForm
            // 
            AcceptButton = btnSave;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(444, 432);
            Controls.Add(btnCancel);
            Controls.Add(btnSave);
            Controls.Add(lblHint);
            Controls.Add(grpPort);
            Name = "SerialPortSettingsForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Serial Port Settings";
            grpPort.ResumeLayout(false);
            grpPort.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox grpPort;
        private Label lblStopBits;
        private Label lblDataBits;
        private Label lblParity;
        private Label lblBaudRate;
        private Label lblPortName;
        private ComboBox cmbStopBits;
        private ComboBox cmbDataBits;
        private ComboBox cmbParity;
        private ComboBox cmbBaudRate;
        private ComboBox cmbPortName;
        private CheckBox chkEnabled;
        private Label lblHint;
        private Button btnSave;
        private Button btnCancel;
    }
}
