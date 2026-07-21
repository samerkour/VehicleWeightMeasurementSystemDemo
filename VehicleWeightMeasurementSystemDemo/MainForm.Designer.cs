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
            lblAxleTitle = new Label();
            lblTotalWeight = new Label();
            lblAxles = new Label();
            lblLine = new Label();
            dgvAxles = new DataGridView();
            lblSpeed = new Label();
            lblDetectedPlate = new Label();
            pictureBoxVehicle = new PictureBox();
            dataGridViewTextBoxColumn1 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn2 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn3 = new DataGridViewTextBoxColumn();
            backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            grpImage = new GroupBox();
            tableLayoutPanel = new TableLayoutPanel();
            dgvRecords = new DataGridView();
            lblRecordsTitle = new Label();
            menuStrip1 = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            exitToolStripMenuItem = new ToolStripMenuItem();
            monitoringToolStripMenuItem = new ToolStripMenuItem();
            startSystemToolStripMenuItem = new ToolStripMenuItem();
            stopSystemToolStripMenuItem = new ToolStripMenuItem();
            reportsToolStripMenuItem = new ToolStripMenuItem();
            dailyReportToolStripMenuItem = new ToolStripMenuItem();
            monthlyReportToolStripMenuItem = new ToolStripMenuItem();
            overweightVehiclesToolStripMenuItem = new ToolStripMenuItem();
            settingsToolStripMenuItem = new ToolStripMenuItem();
            serialPortSettingsToolStripMenuItem = new ToolStripMenuItem();
            cameraSettingsToolStripMenuItem = new ToolStripMenuItem();
            databaseSettingsToolStripMenuItem = new ToolStripMenuItem();
            helpToolStripMenuItem = new ToolStripMenuItem();
            aboutToolStripMenuItem = new ToolStripMenuItem();
            picCam1 = new PictureBox();
            picCam2 = new PictureBox();
            grpVehicleInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvAxles).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxVehicle).BeginInit();
            grpImage.SuspendLayout();
            tableLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvRecords).BeginInit();
            menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picCam1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picCam2).BeginInit();
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
            grpVehicleInfo.Controls.Add(lblAxleTitle);
            grpVehicleInfo.Controls.Add(lblTotalWeight);
            grpVehicleInfo.Controls.Add(lblAxles);
            grpVehicleInfo.Controls.Add(lblLine);
            grpVehicleInfo.Controls.Add(dgvAxles);
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
            // lblAxleTitle
            // 
            lblAxleTitle.AutoSize = true;
            lblAxleTitle.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblAxleTitle.ForeColor = Color.RoyalBlue;
            lblAxleTitle.Location = new Point(14, 243);
            lblAxleTitle.Name = "lblAxleTitle";
            lblAxleTitle.Size = new Size(120, 28);
            lblAxleTitle.TabIndex = 7;
            lblAxleTitle.Text = "Information";
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
            // dgvAxles
            // 
            dgvAxles.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            dgvAxles.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvAxles.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvAxles.Location = new Point(11, 274);
            dgvAxles.Margin = new Padding(3, 4, 3, 4);
            dgvAxles.Name = "dgvAxles";
            dgvAxles.RowHeadersWidth = 51;
            dgvAxles.Size = new Size(543, 217);
            dgvAxles.TabIndex = 5;


            dgvAxles.AutoGenerateColumns = false;
            dgvAxles.Columns.Clear();

            dgvAxles.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Axle",
                HeaderText = "Axle",
                DataPropertyName = "AxleIndex",
                Width = 30
            });


            // 🔹 ADC Column
            dgvAxles.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ADC",
                HeaderText = "ADC",
                DataPropertyName = "ADCDisplay",
                Width = 120
            });

            // 🔹 Distance Column
            dgvAxles.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Distance",
                HeaderText = "Distance (m)",
                DataPropertyName = "DistanceDisplay",
                Width = 120
            });

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
            // dataGridViewTextBoxColumn1
            // 
            dataGridViewTextBoxColumn1.MinimumWidth = 6;
            dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            dataGridViewTextBoxColumn1.Width = 125;
            // 
            // dataGridViewTextBoxColumn2
            // 
            dataGridViewTextBoxColumn2.MinimumWidth = 6;
            dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            dataGridViewTextBoxColumn2.Width = 125;
            // 
            // dataGridViewTextBoxColumn3
            // 
            dataGridViewTextBoxColumn3.MinimumWidth = 6;
            dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            dataGridViewTextBoxColumn3.Width = 125;
            // 
            // grpImage
            // 
            grpImage.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grpImage.Controls.Add(tableLayoutPanel);
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
            // tableLayoutPanel
            // 
            tableLayoutPanel.ColumnCount = 2;
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel.Controls.Add(picCam1, 0, 0);
            tableLayoutPanel.Controls.Add(picCam2, 1, 0);
            tableLayoutPanel.Dock = DockStyle.Fill;
            tableLayoutPanel.Location = new Point(3, 27);
            tableLayoutPanel.Name = "tableLayoutPanel";
            tableLayoutPanel.RowCount = 1;
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel.Size = new Size(554, 469);
            tableLayoutPanel.TabIndex = 4;
          
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
            reportsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { dailyReportToolStripMenuItem, monthlyReportToolStripMenuItem, overweightVehiclesToolStripMenuItem });
            reportsToolStripMenuItem.Name = "reportsToolStripMenuItem";
            reportsToolStripMenuItem.Size = new Size(74, 24);
            reportsToolStripMenuItem.Text = "Reports";
            // 
            // dailyReportToolStripMenuItem
            // 
            dailyReportToolStripMenuItem.Name = "dailyReportToolStripMenuItem";
            dailyReportToolStripMenuItem.Size = new Size(225, 26);
            dailyReportToolStripMenuItem.Text = "Daily Report";
            dailyReportToolStripMenuItem.Click += dailyReportToolStripMenuItem_Click;
            // 
            // monthlyReportToolStripMenuItem
            // 
            monthlyReportToolStripMenuItem.Name = "monthlyReportToolStripMenuItem";
            monthlyReportToolStripMenuItem.Size = new Size(225, 26);
            monthlyReportToolStripMenuItem.Text = "Monthly Report";
            monthlyReportToolStripMenuItem.Click += monthlyReportToolStripMenuItem_Click;
            // 
            // overweightVehiclesToolStripMenuItem
            // 
            overweightVehiclesToolStripMenuItem.Name = "overweightVehiclesToolStripMenuItem";
            overweightVehiclesToolStripMenuItem.Size = new Size(225, 26);
            overweightVehiclesToolStripMenuItem.Text = "Overweight Vehicles";
            overweightVehiclesToolStripMenuItem.Click += overweightVehiclesToolStripMenuItem_Click;
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
            aboutToolStripMenuItem.Size = new Size(224, 26);
            aboutToolStripMenuItem.Text = "About";
            aboutToolStripMenuItem.Click += aboutToolStripMenuItem_Click;
            // 
            // picCam1
            // 
            picCam1.BorderStyle = BorderStyle.FixedSingle;
            picCam1.Dock = DockStyle.Fill;
            picCam1.Location = new Point(3, 3);
            picCam1.Name = "picCam1";
            picCam1.Size = new Size(271, 463);
            picCam1.SizeMode = PictureBoxSizeMode.StretchImage;
            picCam1.TabIndex = 0;
            picCam1.TabStop = false;
            // 
            // picCam2
            // 
            picCam2.BorderStyle = BorderStyle.FixedSingle;
            picCam2.Dock = DockStyle.Fill;
            picCam2.Location = new Point(280, 3);
            picCam2.Name = "picCam2";
            picCam2.Size = new Size(271, 463);
            picCam2.SizeMode = PictureBoxSizeMode.StretchImage;
            picCam2.TabIndex = 1;
            picCam2.TabStop = false;
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
            Load += MainForm_Load;
            grpVehicleInfo.ResumeLayout(false);
            grpVehicleInfo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvAxles).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxVehicle).EndInit();
            grpImage.ResumeLayout(false);
            tableLayoutPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvRecords).EndInit();
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picCam1).EndInit();
            ((System.ComponentModel.ISupportInitialize)picCam2).EndInit();
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
        private DataGridView dgvAxles;
        private DataGridView dgvRecords;
        private Label lblAxleTitle;
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
        private ToolStripMenuItem dailyReportToolStripMenuItem;
        private ToolStripMenuItem monthlyReportToolStripMenuItem;
        private ToolStripMenuItem overweightVehiclesToolStripMenuItem;
        private ToolStripMenuItem serialPortSettingsToolStripMenuItem;
        private ToolStripMenuItem cameraSettingsToolStripMenuItem;
        private ToolStripMenuItem databaseSettingsToolStripMenuItem;
        private ToolStripMenuItem aboutToolStripMenuItem;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private TableLayoutPanel tableLayoutPanel;
        private PictureBox picCam1;
        private PictureBox picCam2;
    }
}
