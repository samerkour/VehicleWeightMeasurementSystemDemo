namespace VehicleWeightMeasurementSystemDemo
{
    partial class VehicleClassificationSettingsForm
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
            grpThresholds = new GroupBox();
            lblSedan = new Label();
            nudSedan = new NumericUpDown();
            lblPickup = new Label();
            nudPickup = new NumericUpDown();
            lblLightTruck = new Label();
            nudLightTruck = new NumericUpDown();
            lblTruck3 = new Label();
            nudTruck3 = new NumericUpDown();
            lblTruck4 = new Label();
            nudTruck4 = new NumericUpDown();
            grpDirection = new GroupBox();
            lblExpectedDirection = new Label();
            nudExpectedDirection = new NumericUpDown();
            lblMinConfidence = new Label();
            nudMinConfidence = new NumericUpDown();
            grpClasses = new GroupBox();
            dgvClassWeights = new DataGridView();
            colClass = new DataGridViewTextBoxColumn();
            colMax = new DataGridViewTextBoxColumn();
            btnSave = new Button();
            btnCancel = new Button();
            grpThresholds.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudSedan).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudPickup).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudLightTruck).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudTruck3).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudTruck4).BeginInit();
            grpDirection.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudExpectedDirection).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudMinConfidence).BeginInit();
            grpClasses.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvClassWeights).BeginInit();
            SuspendLayout();
            // 
            // grpThresholds
            // 
            grpThresholds.Controls.Add(lblSedan);
            grpThresholds.Controls.Add(nudSedan);
            grpThresholds.Controls.Add(lblPickup);
            grpThresholds.Controls.Add(nudPickup);
            grpThresholds.Controls.Add(lblLightTruck);
            grpThresholds.Controls.Add(nudLightTruck);
            grpThresholds.Controls.Add(lblTruck3);
            grpThresholds.Controls.Add(nudTruck3);
            grpThresholds.Controls.Add(lblTruck4);
            grpThresholds.Controls.Add(nudTruck4);
            grpThresholds.Location = new Point(12, 12);
            grpThresholds.Name = "grpThresholds";
            grpThresholds.Padding = new Padding(14);
            grpThresholds.Size = new Size(658, 250);
            grpThresholds.TabIndex = 0;
            grpThresholds.TabStop = false;
            grpThresholds.Text = "Classification Weight Thresholds (kg)";
            // 
            // lblSedan
            // 
            lblSedan.AutoSize = true;
            lblSedan.Location = new Point(22, 40);
            lblSedan.Name = "lblSedan";
            lblSedan.Size = new Size(163, 20);
            lblSedan.TabIndex = 0;
            lblSedan.Text = "Sedan Max Weight (kg)";
            // 
            // nudSedan
            // 
            nudSedan.Location = new Point(398, 37);
            nudSedan.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            nudSedan.Name = "nudSedan";
            nudSedan.Size = new Size(240, 27);
            nudSedan.TabIndex = 1;
            // 
            // lblPickup
            // 
            lblPickup.AutoSize = true;
            lblPickup.Location = new Point(22, 78);
            lblPickup.Name = "lblPickup";
            lblPickup.Size = new Size(165, 20);
            lblPickup.TabIndex = 2;
            lblPickup.Text = "Pickup Max Weight (kg)";
            // 
            // nudPickup
            // 
            nudPickup.Location = new Point(398, 75);
            nudPickup.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            nudPickup.Name = "nudPickup";
            nudPickup.Size = new Size(240, 27);
            nudPickup.TabIndex = 3;
            // 
            // lblLightTruck
            // 
            lblLightTruck.AutoSize = true;
            lblLightTruck.Location = new Point(22, 116);
            lblLightTruck.Name = "lblLightTruck";
            lblLightTruck.Size = new Size(248, 20);
            lblLightTruck.TabIndex = 4;
            lblLightTruck.Text = "Light Truck (2-axle) Max Weight (kg)";
            // 
            // nudLightTruck
            // 
            nudLightTruck.Location = new Point(398, 113);
            nudLightTruck.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            nudLightTruck.Name = "nudLightTruck";
            nudLightTruck.Size = new Size(240, 27);
            nudLightTruck.TabIndex = 5;
            // 
            // lblTruck3
            // 
            lblTruck3.AutoSize = true;
            lblTruck3.Location = new Point(22, 154);
            lblTruck3.Name = "lblTruck3";
            lblTruck3.Size = new Size(201, 20);
            lblTruck3.TabIndex = 6;
            lblTruck3.Text = "3-axle Truck Max Weight (kg)";
            // 
            // nudTruck3
            // 
            nudTruck3.Location = new Point(398, 151);
            nudTruck3.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            nudTruck3.Name = "nudTruck3";
            nudTruck3.Size = new Size(240, 27);
            nudTruck3.TabIndex = 7;
            // 
            // lblTruck4
            // 
            lblTruck4.AutoSize = true;
            lblTruck4.Location = new Point(22, 192);
            lblTruck4.Name = "lblTruck4";
            lblTruck4.Size = new Size(201, 20);
            lblTruck4.TabIndex = 8;
            lblTruck4.Text = "4-axle Truck Max Weight (kg)";
            // 
            // nudTruck4
            // 
            nudTruck4.Location = new Point(398, 189);
            nudTruck4.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            nudTruck4.Name = "nudTruck4";
            nudTruck4.Size = new Size(240, 27);
            nudTruck4.TabIndex = 9;
            // 
            // grpDirection
            // 
            grpDirection.Controls.Add(lblExpectedDirection);
            grpDirection.Controls.Add(nudExpectedDirection);
            grpDirection.Controls.Add(lblMinConfidence);
            grpDirection.Controls.Add(nudMinConfidence);
            grpDirection.Location = new Point(12, 268);
            grpDirection.Name = "grpDirection";
            grpDirection.Padding = new Padding(14);
            grpDirection.Size = new Size(658, 120);
            grpDirection.TabIndex = 1;
            grpDirection.TabStop = false;
            grpDirection.Text = "Direction / Confidence";
            // 
            // lblExpectedDirection
            // 
            lblExpectedDirection.AutoSize = true;
            lblExpectedDirection.Location = new Point(22, 40);
            lblExpectedDirection.Name = "lblExpectedDirection";
            lblExpectedDirection.Size = new Size(327, 20);
            lblExpectedDirection.TabIndex = 0;
            lblExpectedDirection.Text = "Expected Direction (1=COMING, 2=DEPARTING)";
            // 
            // nudExpectedDirection
            // 
            nudExpectedDirection.Location = new Point(398, 37);
            nudExpectedDirection.Maximum = new decimal(new int[] { 2, 0, 0, 0 });
            nudExpectedDirection.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudExpectedDirection.Name = "nudExpectedDirection";
            nudExpectedDirection.Size = new Size(240, 27);
            nudExpectedDirection.TabIndex = 1;
            nudExpectedDirection.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // lblMinConfidence
            // 
            lblMinConfidence.AutoSize = true;
            lblMinConfidence.Location = new Point(22, 78);
            lblMinConfidence.Name = "lblMinConfidence";
            lblMinConfidence.Size = new Size(150, 20);
            lblMinConfidence.TabIndex = 2;
            lblMinConfidence.Text = "Min Plate Confidence";
            // 
            // nudMinConfidence
            // 
            nudMinConfidence.DecimalPlaces = 2;
            nudMinConfidence.Increment = new decimal(new int[] { 5, 0, 0, 131072 });
            nudMinConfidence.Location = new Point(398, 75);
            nudMinConfidence.Maximum = new decimal(new int[] { 1, 0, 0, 0 });
            nudMinConfidence.Minimum = new decimal(new int[] { 1, 0, 0, 131072 });
            nudMinConfidence.Name = "nudMinConfidence";
            nudMinConfidence.Size = new Size(240, 27);
            nudMinConfidence.TabIndex = 3;
            nudMinConfidence.Value = new decimal(new int[] { 65, 0, 0, 131072 });
            // 
            // grpClasses
            // 
            grpClasses.Controls.Add(dgvClassWeights);
            grpClasses.Location = new Point(12, 394);
            grpClasses.Name = "grpClasses";
            grpClasses.Padding = new Padding(14);
            grpClasses.Size = new Size(658, 290);
            grpClasses.TabIndex = 2;
            grpClasses.TabStop = false;
            grpClasses.Text = "Max Allowed Weight by Class (kg)";
            // 
            // dgvClassWeights
            // 
            dgvClassWeights.AllowUserToAddRows = false;
            dgvClassWeights.AllowUserToDeleteRows = false;
            dgvClassWeights.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvClassWeights.Columns.AddRange(new DataGridViewColumn[] { colClass, colMax });
            dgvClassWeights.Dock = DockStyle.Fill;
            dgvClassWeights.Location = new Point(14, 34);
            dgvClassWeights.MultiSelect = false;
            dgvClassWeights.Name = "dgvClassWeights";
            dgvClassWeights.RowHeadersVisible = false;
            dgvClassWeights.RowHeadersWidth = 51;
            dgvClassWeights.RowTemplate.Height = 28;
            dgvClassWeights.Size = new Size(630, 242);
            dgvClassWeights.TabIndex = 0;
            // 
            // colClass
            // 
            colClass.HeaderText = "Class";
            colClass.MinimumWidth = 6;
            colClass.Name = "colClass";
            colClass.ReadOnly = true;
            colClass.Width = 120;
            // 
            // colMax
            // 
            colMax.HeaderText = "Max Weight (kg)";
            colMax.MinimumWidth = 6;
            colMax.Name = "colMax";
            colMax.Width = 200;
            // 
            // btnSave
            // 
            btnSave.Location = new Point(334, 700);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(90, 34);
            btnSave.TabIndex = 3;
            btnSave.Text = "Save";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // btnCancel
            // 
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(434, 700);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(90, 34);
            btnCancel.TabIndex = 4;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // VehicleClassificationSettingsForm
            // 
            AcceptButton = btnSave;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(682, 753);
            Controls.Add(btnCancel);
            Controls.Add(btnSave);
            Controls.Add(grpClasses);
            Controls.Add(grpDirection);
            Controls.Add(grpThresholds);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Name = "VehicleClassificationSettingsForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Vehicle Classification Settings";
            grpThresholds.ResumeLayout(false);
            grpThresholds.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudSedan).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudPickup).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudLightTruck).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudTruck3).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudTruck4).EndInit();
            grpDirection.ResumeLayout(false);
            grpDirection.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudExpectedDirection).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudMinConfidence).EndInit();
            grpClasses.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvClassWeights).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox grpThresholds;
        private Label lblSedan;
        private NumericUpDown nudSedan;
        private Label lblPickup;
        private NumericUpDown nudPickup;
        private Label lblLightTruck;
        private NumericUpDown nudLightTruck;
        private Label lblTruck3;
        private NumericUpDown nudTruck3;
        private Label lblTruck4;
        private NumericUpDown nudTruck4;
        private GroupBox grpDirection;
        private Label lblExpectedDirection;
        private NumericUpDown nudExpectedDirection;
        private Label lblMinConfidence;
        private NumericUpDown nudMinConfidence;
        private GroupBox grpClasses;
        private DataGridView dgvClassWeights;
        private DataGridViewTextBoxColumn colClass;
        private DataGridViewTextBoxColumn colMax;
        private Button btnSave;
        private Button btnCancel;
    }
}
