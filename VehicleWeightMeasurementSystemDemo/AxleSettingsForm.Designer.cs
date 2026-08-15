namespace VehicleWeightMeasurementSystemDemo
{
    partial class AxleSettingsForm
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
            grpAxle = new GroupBox();
            lblAlpha = new Label();
            nudAlpha = new NumericUpDown();
            lblHint = new Label();
            btnSave = new Button();
            btnCancel = new Button();
            grpAxle.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudAlpha).BeginInit();
            SuspendLayout();
            // 
            // grpAxle
            // 
            grpAxle.Controls.Add(lblAlpha);
            grpAxle.Controls.Add(nudAlpha);
            grpAxle.Location = new Point(12, 12);
            grpAxle.Name = "grpAxle";
            grpAxle.Padding = new Padding(14);
            grpAxle.Size = new Size(420, 110);
            grpAxle.TabIndex = 0;
            grpAxle.TabStop = false;
            grpAxle.Text = "Axle Coefficient";
            // 
            // lblAlpha
            // 
            lblAlpha.AutoSize = true;
            lblAlpha.Location = new Point(22, 55);
            lblAlpha.Name = "lblAlpha";
            lblAlpha.Size = new Size(100, 20);
            lblAlpha.TabIndex = 0;
            lblAlpha.Text = "Alpha (α)";
            // 
            // nudAlpha
            // 
            nudAlpha.DecimalPlaces = 3;
            nudAlpha.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            nudAlpha.Location = new Point(170, 52);
            nudAlpha.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            nudAlpha.Minimum = new decimal(new int[] { 1, 0, 0, 196608 });
            nudAlpha.Name = "nudAlpha";
            nudAlpha.Size = new Size(200, 28);
            nudAlpha.TabIndex = 1;
            nudAlpha.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // lblHint
            // 
            lblHint.ForeColor = SystemColors.GrayText;
            lblHint.Location = new Point(12, 128);
            lblHint.Name = "lblHint";
            lblHint.Size = new Size(420, 80);
            lblHint.TabIndex = 2;
            lblHint.Text = "All axle weights (w1..w6) are\nmultiplied by Alpha before display.\nChanges take effect after restarting the monitoring system.";
            // 
            // btnSave
            // 
            btnSave.Location = new Point(216, 200);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(100, 34);
            btnSave.TabIndex = 3;
            btnSave.Text = "Save";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // btnCancel
            // 
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(326, 200);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(100, 34);
            btnCancel.TabIndex = 4;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // AxleSettingsForm
            // 
            AcceptButton = btnSave;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(444, 260);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Controls.Add(btnCancel);
            Controls.Add(btnSave);
            Controls.Add(lblHint);
            Controls.Add(grpAxle);
            Name = "AxleSettingsForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Axle Settings";
            grpAxle.ResumeLayout(false);
            grpAxle.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudAlpha).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox grpAxle;
        private Label lblAlpha;
        private NumericUpDown nudAlpha;
        private Label lblHint;
        private Button btnSave;
        private Button btnCancel;
    }
}