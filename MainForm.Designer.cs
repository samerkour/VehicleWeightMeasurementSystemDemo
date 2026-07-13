namespace VehicleWeightMeasurementSystemDemo 
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            lblSerialStatus = new Label();
            lblCameraStatus = new Label();
            grpVehicleInfo = new GroupBox();
            lblADC4 = new Label();
            lblAxleTitle = new Label();
            lblADC3 = new Label();
            lblADC2 = new Label();
            lblADC1 = new Label();
            lblTotalWeight = new Label();
            lblAxles = new Label();
            lblLine = new Label();
            dgvAxles = new DataGridView();
            lblSpeed = new Label();
            backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            grpImage = new GroupBox();
            lblDetectedPlate = new Label();
            pictureBoxVehicle = new PictureBox();
            dgvRecords = new DataGridView();
            lblRecordsTitle = new Label();
            grpVehicleInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvAxles).BeginInit();
            grpImage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxVehicle).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvRecords).BeginInit();
            SuspendLayout();
            // 
            // lblSerialStatus
            // 
            lblSerialStatus.AutoSize = true;
            lblSerialStatus.Location = new Point(11, 17);
            lblSerialStatus.Name = "lblSerialStatus";
            lblSerialStatus.Size = new Size(167, 20);
            lblSerialStatus.TabIndex = 1;
            lblSerialStatus.Text = "Serial Port: ● Connected";
            // 
            // lblCameraStatus
            // 
            lblCameraStatus.AutoSize = true;
            lblCameraStatus.Location = new Point(343, 17);
            lblCameraStatus.Name = "lblCameraStatus";
            lblCameraStatus.Size = new Size(200, 20);
            lblCameraStatus.TabIndex = 2;
            lblCameraStatus.Text = "Camera Folder: ● Monitoring";
            // 
            // grpVehicleInfo
            // 
            grpVehicleInfo.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            grpVehicleInfo.Controls.Add(lblADC4);
            grpVehicleInfo.Controls.Add(lblAxleTitle);
            grpVehicleInfo.Controls.Add(lblADC3);
            grpVehicleInfo.Controls.Add(lblADC2);
            grpVehicleInfo.Controls.Add(lblADC1);
            grpVehicleInfo.Controls.Add(lblTotalWeight);
            grpVehicleInfo.Controls.Add(lblAxles);
            grpVehicleInfo.Controls.Add(lblLine);
            grpVehicleInfo.Controls.Add(dgvAxles);
            grpVehicleInfo.Controls.Add(lblSpeed);
            grpVehicleInfo.Font = new Font("Segoe UI Semibold", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            grpVehicleInfo.Location = new Point(588, 59);
            grpVehicleInfo.Margin = new Padding(3, 4, 3, 4);
            grpVehicleInfo.Name = "grpVehicleInfo";
            grpVehicleInfo.Padding = new Padding(3, 4, 3, 4);
            grpVehicleInfo.Size = new Size(560, 500);
            grpVehicleInfo.TabIndex = 3;
            grpVehicleInfo.TabStop = false;
            grpVehicleInfo.Text = "Vehicle Information";
            // 
            // lblADC4
            // 
            lblADC4.AutoSize = true;
            lblADC4.Location = new Point(11, 242);
            lblADC4.Name = "lblADC4";
            lblADC4.Size = new Size(84, 23);
            lblADC4.TabIndex = 11;
            lblADC4.Text = "ADC4: ---";
            // 
            // lblAxleTitle
            // 
            lblAxleTitle.AutoSize = true;
            lblAxleTitle.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblAxleTitle.ForeColor = Color.RoyalBlue;
            lblAxleTitle.Location = new Point(9, 267);
            lblAxleTitle.Name = "lblAxleTitle";
            lblAxleTitle.Size = new Size(165, 28);
            lblAxleTitle.TabIndex = 7;
            lblAxleTitle.Text = "Axle Information";
            // 
            // lblADC3
            // 
            lblADC3.AutoSize = true;
            lblADC3.Location = new Point(11, 209);
            lblADC3.Name = "lblADC3";
            lblADC3.Size = new Size(83, 23);
            lblADC3.TabIndex = 10;
            lblADC3.Text = "ADC3: ---";
            // 
            // lblADC2
            // 
            lblADC2.AutoSize = true;
            lblADC2.Location = new Point(11, 175);
            lblADC2.Name = "lblADC2";
            lblADC2.Size = new Size(83, 23);
            lblADC2.TabIndex = 9;
            lblADC2.Text = "ADC2: ---";
            // 
            // lblADC1
            // 
            lblADC1.AutoSize = true;
            lblADC1.Location = new Point(11, 142);
            lblADC1.Name = "lblADC1";
            lblADC1.Size = new Size(81, 23);
            lblADC1.TabIndex = 8;
            lblADC1.Text = "ADC1: ---";
            // 
            // lblTotalWeight
            // 
            lblTotalWeight.AutoSize = true;
            lblTotalWeight.Location = new Point(11, 109);
            lblTotalWeight.Name = "lblTotalWeight";
            lblTotalWeight.Size = new Size(131, 23);
            lblTotalWeight.TabIndex = 7;
            lblTotalWeight.Text = "TotalWeight: ---";
            // 
            // lblAxles
            // 
            lblAxles.AutoSize = true;
            lblAxles.Location = new Point(11, 75);
            lblAxles.Name = "lblAxles";
            lblAxles.Size = new Size(108, 23);
            lblAxles.TabIndex = 6;
            lblAxles.Text = "Axles No: ---";
            // 
            // lblLine
            // 
            lblLine.AutoSize = true;
            lblLine.Location = new Point(11, 42);
            lblLine.Name = "lblLine";
            lblLine.Size = new Size(71, 23);
            lblLine.TabIndex = 5;
            lblLine.Text = "Line: ---";
            // 
            // dgvAxles
            // 
            dgvAxles.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            dgvAxles.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvAxles.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvAxles.Location = new Point(6, 296);
            dgvAxles.Margin = new Padding(3, 4, 3, 4);
            dgvAxles.Name = "dgvAxles";
            dgvAxles.RowHeadersWidth = 51;
            dgvAxles.Size = new Size(548, 191);
            dgvAxles.TabIndex = 5;
            // 
            // lblSpeed
            // 
            lblSpeed.AutoSize = true;
            lblSpeed.Location = new Point(11, -21);
            lblSpeed.Name = "lblSpeed";
            lblSpeed.Size = new Size(87, 23);
            lblSpeed.TabIndex = 4;
            lblSpeed.Text = "Speed: ---";
            // 
            // grpImage
            // 
            grpImage.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grpImage.Controls.Add(lblDetectedPlate);
            grpImage.Controls.Add(pictureBoxVehicle);
            grpImage.Font = new Font("Segoe UI Semibold", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            grpImage.Location = new Point(10, 59);
            grpImage.Margin = new Padding(3, 4, 3, 4);
            grpImage.Name = "grpImage";
            grpImage.Padding = new Padding(3, 4, 3, 4);
            grpImage.Size = new Size(560, 500);
            grpImage.TabIndex = 4;
            grpImage.TabStop = false;
            grpImage.Text = "Captured Vehicle Image";
            // 
            // lblDetectedPlate
            // 
            lblDetectedPlate.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblDetectedPlate.AutoSize = true;
            lblDetectedPlate.Location = new Point(11, 464);
            lblDetectedPlate.Name = "lblDetectedPlate";
            lblDetectedPlate.Size = new Size(180, 23);
            lblDetectedPlate.TabIndex = 1;
            lblDetectedPlate.Text = "Detected Plate No: ---";
            // 
            // pictureBoxVehicle
            // 
            pictureBoxVehicle.BorderStyle = BorderStyle.FixedSingle;
            pictureBoxVehicle.Dock = DockStyle.Fill;
            pictureBoxVehicle.Location = new Point(3, 27);
            pictureBoxVehicle.Margin = new Padding(3, 4, 3, 4);
            pictureBoxVehicle.Name = "pictureBoxVehicle";
            pictureBoxVehicle.Size = new Size(554, 469);
            pictureBoxVehicle.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxVehicle.TabIndex = 0;
            pictureBoxVehicle.TabStop = false;
            // 
            // dgvRecords
            // 
            dgvRecords.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvRecords.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvRecords.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvRecords.Location = new Point(10, 601);
            dgvRecords.Margin = new Padding(3, 4, 3, 4);
            dgvRecords.Name = "dgvRecords";
            dgvRecords.RowHeadersWidth = 51;
            dgvRecords.Size = new Size(1138, 284);
            dgvRecords.TabIndex = 6;
            dgvRecords.DataBindingComplete += dgvRecords_DataBindingComplete;
            // 
            // lblRecordsTitle
            // 
            lblRecordsTitle.AutoSize = true;
            lblRecordsTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblRecordsTitle.Location = new Point(13, 570);
            lblRecordsTitle.Name = "lblRecordsTitle";
            lblRecordsTitle.Size = new Size(221, 28);
            lblRecordsTitle.TabIndex = 8;
            lblRecordsTitle.Text = "Database Records Log";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1160, 900);
            Controls.Add(lblRecordsTitle);
            Controls.Add(dgvRecords);
            Controls.Add(grpImage);
            Controls.Add(grpVehicleInfo);
            Controls.Add(lblCameraStatus);
            Controls.Add(lblSerialStatus);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(3, 4, 3, 4);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Vehicle Weight Measurement System";
            Load += MainForm_Load;
            grpVehicleInfo.ResumeLayout(false);
            grpVehicleInfo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvAxles).EndInit();
            grpImage.ResumeLayout(false);
            grpImage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxVehicle).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvRecords).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Label lblSerialStatus;
        private Label lblCameraStatus;
        private GroupBox grpVehicleInfo;
        private Label lblSpeed;
        private Label lblLine;
        private Label lblAxles;
        private Label lblTotalWeight;
        private Label lblADC4;
        private Label lblADC3;
        private Label lblADC2;
        private Label lblADC1;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private GroupBox grpImage;
        private PictureBox pictureBoxVehicle;
        private Label lblDetectedPlate;
        private DataGridView dgvAxles;
        private DataGridView dgvRecords;
        private Label lblAxleTitle;
        private Label lblRecordsTitle;
    }
}
