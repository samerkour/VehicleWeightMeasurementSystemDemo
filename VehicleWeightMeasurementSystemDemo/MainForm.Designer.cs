using Microsoft.EntityFrameworkCore.Metadata.Internal;

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
            lblAxle56 = new Label();
            lblAxle45 = new Label();
            lblAxle34 = new Label();
            lblAxle23 = new Label();
            lblAxle12 = new Label();
            lblADC4 = new Label();
            lblADC3 = new Label();
            lblADC2 = new Label();
            lblADC1 = new Label();
            lblTotalWeight = new Label();
            lblAxles = new Label();
            lblLine = new Label();
            lblSpeed = new Label();
            lblDetectedPlate = new Label();
            pictureBoxVehicle = new PictureBox();
            backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            grpImage = new GroupBox();
            picCam1 = new PictureBox();
            dgvRecords = new DataGridView();
            lblRecordsTitle = new Label();
            menuStrip1 = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            exitToolStripMenuItem = new ToolStripMenuItem();
            monitoringToolStripMenuItem = new ToolStripMenuItem();
            startSystemToolStripMenuItem = new ToolStripMenuItem();
            stopSystemToolStripMenuItem = new ToolStripMenuItem();
            reportsToolStripMenuItem = new ToolStripMenuItem();
            VehicleReportToolStripMenuItem = new ToolStripMenuItem();
            settingsToolStripMenuItem = new ToolStripMenuItem();
            serialPortSettingsToolStripMenuItem = new ToolStripMenuItem();
            cameraSettingsToolStripMenuItem = new ToolStripMenuItem();
            databaseSettingsToolStripMenuItem = new ToolStripMenuItem();
            helpToolStripMenuItem = new ToolStripMenuItem();
            aboutToolStripMenuItem = new ToolStripMenuItem();
            sqlCommand1 = new Microsoft.Data.SqlClient.SqlCommand();
            grpVehicleInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxVehicle).BeginInit();
            grpImage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picCam1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvRecords).BeginInit();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // lblSerialStatus
            // 
            lblSerialStatus.AutoSize = true;
            lblSerialStatus.Location = new Point(13, 36);
            lblSerialStatus.Name = "lblSerialStatus";
            lblSerialStatus.Size = new Size(167, 20);
            lblSerialStatus.TabIndex = 1;
            lblSerialStatus.Text = "Serial Port: ● Connected";
            // 
            // lblCameraStatus
            // 
            lblCameraStatus.AutoSize = true;
            lblCameraStatus.Location = new Point(343, 36);
            lblCameraStatus.Name = "lblCameraStatus";
            lblCameraStatus.Size = new Size(200, 20);
            lblCameraStatus.TabIndex = 2;
            lblCameraStatus.Text = "Camera Folder: ● Monitoring";
            // 
            // grpVehicleInfo
            // 
            grpVehicleInfo.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            grpVehicleInfo.Controls.Add(lblAxle56);
            grpVehicleInfo.Controls.Add(lblAxle45);
            grpVehicleInfo.Controls.Add(lblAxle34);
            grpVehicleInfo.Controls.Add(lblAxle23);
            grpVehicleInfo.Controls.Add(lblAxle12);
            grpVehicleInfo.Controls.Add(lblADC4);
            grpVehicleInfo.Controls.Add(lblADC3);
            grpVehicleInfo.Controls.Add(lblADC2);
            grpVehicleInfo.Controls.Add(lblADC1);
            grpVehicleInfo.Controls.Add(lblTotalWeight);
            grpVehicleInfo.Controls.Add(lblAxles);
            grpVehicleInfo.Controls.Add(lblLine);
            grpVehicleInfo.Controls.Add(lblSpeed);
            grpVehicleInfo.Controls.Add(lblDetectedPlate);
            grpVehicleInfo.Controls.Add(pictureBoxVehicle);
            grpVehicleInfo.Font = new Font("Segoe UI Semibold", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            grpVehicleInfo.Location = new Point(588, 71);
            grpVehicleInfo.Margin = new Padding(3, 4, 3, 4);
            grpVehicleInfo.Name = "grpVehicleInfo";
            grpVehicleInfo.Padding = new Padding(3, 4, 3, 4);
            grpVehicleInfo.Size = new Size(560, 500);
            grpVehicleInfo.TabIndex = 3;
            grpVehicleInfo.TabStop = false;
            grpVehicleInfo.Text = "Vehicle Information";
            // 
            // lblAxle56
            // 
            lblAxle56.AutoSize = true;
            lblAxle56.Location = new Point(484, 392);
            lblAxle56.Name = "lblAxle56";
            lblAxle56.Size = new Size(61, 23);
            lblAxle56.TabIndex = 16;
            lblAxle56.Text = "Axle56";
            // 
            // lblAxle45
            // 
            lblAxle45.AutoSize = true;
            lblAxle45.Location = new Point(368, 392);
            lblAxle45.Name = "lblAxle45";
            lblAxle45.Size = new Size(62, 23);
            lblAxle45.TabIndex = 15;
            lblAxle45.Text = "Axle45";
            // 
            // lblAxle34
            // 
            lblAxle34.AutoSize = true;
            lblAxle34.Location = new Point(252, 392);
            lblAxle34.Name = "lblAxle34";
            lblAxle34.Size = new Size(62, 23);
            lblAxle34.TabIndex = 14;
            lblAxle34.Text = "Axle34";
            // 
            // lblAxle23
            // 
            lblAxle23.AutoSize = true;
            lblAxle23.Location = new Point(137, 392);
            lblAxle23.Name = "lblAxle23";
            lblAxle23.Size = new Size(61, 23);
            lblAxle23.TabIndex = 13;
            lblAxle23.Text = "Axle23";
            // 
            // lblAxle12
            // 
            lblAxle12.AutoSize = true;
            lblAxle12.Location = new Point(24, 392);
            lblAxle12.Name = "lblAxle12";
            lblAxle12.Size = new Size(59, 23);
            lblAxle12.TabIndex = 12;
            lblAxle12.Text = "Axle12";
            // 
            // lblADC4
            // 
            lblADC4.AutoSize = true;
            lblADC4.Location = new Point(388, 289);
            lblADC4.Name = "lblADC4";
            lblADC4.Size = new Size(54, 23);
            lblADC4.TabIndex = 11;
            lblADC4.Text = "ADC4";
            // 
            // lblADC3
            // 
            lblADC3.AutoSize = true;
            lblADC3.Location = new Point(266, 289);
            lblADC3.Name = "lblADC3";
            lblADC3.Size = new Size(53, 23);
            lblADC3.TabIndex = 10;
            lblADC3.Text = "ADC3";
            // 
            // lblADC2
            // 
            lblADC2.AutoSize = true;
            lblADC2.Location = new Point(144, 289);
            lblADC2.Name = "lblADC2";
            lblADC2.Size = new Size(53, 23);
            lblADC2.TabIndex = 9;
            lblADC2.Text = "ADC2";
            // 
            // lblADC1
            // 
            lblADC1.AutoSize = true;
            lblADC1.Location = new Point(24, 289);
            lblADC1.Name = "lblADC1";
            lblADC1.Size = new Size(51, 23);
            lblADC1.TabIndex = 8;
            lblADC1.Text = "ADC1";
            // 
            // lblTotalWeight
            // 
            lblTotalWeight.AutoSize = true;
            lblTotalWeight.Location = new Point(11, 158);
            lblTotalWeight.Name = "lblTotalWeight";
            lblTotalWeight.Size = new Size(131, 23);
            lblTotalWeight.TabIndex = 7;
            lblTotalWeight.Text = "TotalWeight: ---";
            // 
            // lblAxles
            // 
            lblAxles.AutoSize = true;
            lblAxles.Location = new Point(14, 117);
            lblAxles.Name = "lblAxles";
            lblAxles.Size = new Size(108, 23);
            lblAxles.TabIndex = 6;
            lblAxles.Text = "Axles No: ---";
            // 
            // lblLine
            // 
            lblLine.AutoSize = true;
            lblLine.Location = new Point(14, 76);
            lblLine.Name = "lblLine";
            lblLine.Size = new Size(71, 23);
            lblLine.TabIndex = 5;
            lblLine.Text = "Line: ---";
            // 
            // lblSpeed
            // 
            lblSpeed.AutoSize = true;
            lblSpeed.Location = new Point(11, 35);
            lblSpeed.Name = "lblSpeed";
            lblSpeed.Size = new Size(87, 23);
            lblSpeed.TabIndex = 4;
            lblSpeed.Text = "Speed: ---";
            // 
            // lblDetectedPlate
            // 
            lblDetectedPlate.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblDetectedPlate.AutoSize = true;
            lblDetectedPlate.Location = new Point(209, 215);
            lblDetectedPlate.Name = "lblDetectedPlate";
            lblDetectedPlate.Size = new Size(180, 23);
            lblDetectedPlate.TabIndex = 1;
            lblDetectedPlate.Text = "Detected Plate No: ---";
            // 
            // pictureBoxVehicle
            // 
            pictureBoxVehicle.BorderStyle = BorderStyle.FixedSingle;
            pictureBoxVehicle.Location = new Point(204, 15);
            pictureBoxVehicle.Margin = new Padding(3, 4, 3, 4);
            pictureBoxVehicle.Name = "pictureBoxVehicle";
            pictureBoxVehicle.Size = new Size(350, 227);
            pictureBoxVehicle.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxVehicle.TabIndex = 0;
            pictureBoxVehicle.TabStop = false;
            // 
            // grpImage
            // 
            grpImage.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grpImage.Controls.Add(picCam1);
            grpImage.Font = new Font("Segoe UI Semibold", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            grpImage.Location = new Point(10, 71);
            grpImage.Margin = new Padding(3, 4, 3, 4);
            grpImage.Name = "grpImage";
            grpImage.Padding = new Padding(3, 4, 3, 4);
            grpImage.Size = new Size(560, 500);
            grpImage.TabIndex = 4;
            grpImage.TabStop = false;
            grpImage.Text = "Vehicle Camera OverView";
            // 
            // picCam1
            // 
            picCam1.BorderStyle = BorderStyle.FixedSingle;
            picCam1.Dock = DockStyle.Fill;
            picCam1.Location = new Point(3, 27);
            picCam1.Name = "picCam1";
            picCam1.Size = new Size(554, 469);
            picCam1.SizeMode = PictureBoxSizeMode.StretchImage;
            picCam1.TabIndex = 0;
            picCam1.TabStop = false;
            // 
            // dgvRecords
            // 
            dgvRecords.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvRecords.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvRecords.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvRecords.Location = new Point(10, 606);
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
            lblRecordsTitle.Location = new Point(13, 575);
            lblRecordsTitle.Name = "lblRecordsTitle";
            lblRecordsTitle.Size = new Size(221, 28);
            lblRecordsTitle.TabIndex = 8;
            lblRecordsTitle.Text = "Database Records Log";
            // 
            // menuStrip1
            // 
            menuStrip1.BackColor = Color.FromArgb(33, 150, 243);
            menuStrip1.ForeColor = Color.White;
            menuStrip1.ImageScalingSize = new Size(20, 20);
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, monitoringToolStripMenuItem, reportsToolStripMenuItem, settingsToolStripMenuItem, helpToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.RenderMode = ToolStripRenderMode.System;
            menuStrip1.Size = new Size(1160, 28);
            menuStrip1.TabIndex = 9;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { exitToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(46, 24);
            fileToolStripMenuItem.Text = "File";
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new Size(116, 26);
            exitToolStripMenuItem.Text = "Exit";
            exitToolStripMenuItem.Click += exitToolStripMenuItem_Click;
            // 
            // monitoringToolStripMenuItem
            // 
            monitoringToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { startSystemToolStripMenuItem, stopSystemToolStripMenuItem });
            monitoringToolStripMenuItem.Name = "monitoringToolStripMenuItem";
            monitoringToolStripMenuItem.Size = new Size(97, 24);
            monitoringToolStripMenuItem.Text = "Monitoring";
            // 
            // startSystemToolStripMenuItem
            // 
            startSystemToolStripMenuItem.Name = "startSystemToolStripMenuItem";
            startSystemToolStripMenuItem.Size = new Size(174, 26);
            startSystemToolStripMenuItem.Text = "Start System";
            startSystemToolStripMenuItem.Click += startSystemToolStripMenuItem_Click;
            // 
            // stopSystemToolStripMenuItem
            // 
            stopSystemToolStripMenuItem.Name = "stopSystemToolStripMenuItem";
            stopSystemToolStripMenuItem.Size = new Size(174, 26);
            stopSystemToolStripMenuItem.Text = "Stop System";
            stopSystemToolStripMenuItem.Click += stopSystemToolStripMenuItem_Click;
            // 
            // reportsToolStripMenuItem
            // 
            reportsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { VehicleReportToolStripMenuItem });
            reportsToolStripMenuItem.Name = "reportsToolStripMenuItem";
            reportsToolStripMenuItem.Size = new Size(74, 24);
            reportsToolStripMenuItem.Text = "Reports";
            // 
            // VehicleReportToolStripMenuItem
            // 
            VehicleReportToolStripMenuItem.Name = "VehicleReportToolStripMenuItem";
            VehicleReportToolStripMenuItem.Size = new Size(188, 26);
            VehicleReportToolStripMenuItem.Text = "Vehicle Report";
            VehicleReportToolStripMenuItem.Click += VehicleReportToolStripMenuItem_Click;
            // 
            // settingsToolStripMenuItem
            // 
            settingsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { serialPortSettingsToolStripMenuItem, cameraSettingsToolStripMenuItem, databaseSettingsToolStripMenuItem });
            settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            settingsToolStripMenuItem.Size = new Size(76, 24);
            settingsToolStripMenuItem.Text = "Settings";
            // 
            // serialPortSettingsToolStripMenuItem
            // 
            serialPortSettingsToolStripMenuItem.Name = "serialPortSettingsToolStripMenuItem";
            serialPortSettingsToolStripMenuItem.Size = new Size(216, 26);
            serialPortSettingsToolStripMenuItem.Text = "Serial Port Settings";
            // 
            // cameraSettingsToolStripMenuItem
            // 
            cameraSettingsToolStripMenuItem.Name = "cameraSettingsToolStripMenuItem";
            cameraSettingsToolStripMenuItem.Size = new Size(216, 26);
            cameraSettingsToolStripMenuItem.Text = "Camera Settings";
            // 
            // databaseSettingsToolStripMenuItem
            // 
            databaseSettingsToolStripMenuItem.Name = "databaseSettingsToolStripMenuItem";
            databaseSettingsToolStripMenuItem.Size = new Size(216, 26);
            databaseSettingsToolStripMenuItem.Text = "Database Settings";
            // 
            // helpToolStripMenuItem
            // 
            helpToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { aboutToolStripMenuItem });
            helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            helpToolStripMenuItem.Size = new Size(55, 24);
            helpToolStripMenuItem.Text = "Help";
            // 
            // aboutToolStripMenuItem
            // 
            aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            aboutToolStripMenuItem.Size = new Size(133, 26);
            aboutToolStripMenuItem.Text = "About";
            aboutToolStripMenuItem.Click += aboutToolStripMenuItem_Click;
            // 
            // sqlCommand1
            // 
            sqlCommand1.CommandTimeout = 30;
            sqlCommand1.EnableOptimizedParameterBinding = false;
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
            Controls.Add(menuStrip1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = menuStrip1;
            Margin = new Padding(3, 4, 3, 4);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Vehicle Weight Measurement System";
            FormClosing += MainForm_FormClosing;
            Load += MainForm_Load;
            grpVehicleInfo.ResumeLayout(false);
            grpVehicleInfo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxVehicle).EndInit();
            grpImage.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picCam1).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvRecords).EndInit();
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
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
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private GroupBox grpImage;
        private PictureBox pictureBoxVehicle;
        private Label lblDetectedPlate;
        private DataGridView dgvRecords;
        private Label lblRecordsTitle;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem monitoringToolStripMenuItem;
        private ToolStripMenuItem reportsToolStripMenuItem;
        private ToolStripMenuItem settingsToolStripMenuItem;
        private ToolStripMenuItem helpToolStripMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripMenuItem startSystemToolStripMenuItem;
        private ToolStripMenuItem stopSystemToolStripMenuItem;
        private ToolStripMenuItem VehicleReportToolStripMenuItem;
        private ToolStripMenuItem serialPortSettingsToolStripMenuItem;
        private ToolStripMenuItem cameraSettingsToolStripMenuItem;
        private ToolStripMenuItem databaseSettingsToolStripMenuItem;
        private ToolStripMenuItem aboutToolStripMenuItem;

        private PictureBox picCam1;
        private Label lblADC4;
        private Label lblADC3;
        private Label lblADC2;
        private Label lblADC1;
        private Label lblAxle34;
        private Label lblAxle23;
        private Label lblAxle12;
        private Label lblAxle56;
        private Label lblAxle45;
        private Microsoft.Data.SqlClient.SqlCommand sqlCommand1;
    }
}
