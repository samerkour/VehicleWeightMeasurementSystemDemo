namespace VehicleWeightMeasurementSystemDemo
{
    partial class SnapshotCameraSettingsForm
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
            grpCamera = new GroupBox();
            lblWaitTimeout = new Label();
            lblLookback = new Label();
            lblFilter = new Label();
            lblWatchRoot = new Label();
            chkIncludeSubfolders = new CheckBox();
            chkEnabled = new CheckBox();
            txtWatchRoot = new TextBox();
            txtFilter = new TextBox();
            nudLookback = new NumericUpDown();
            nudWaitTimeout = new NumericUpDown();
            btnBrowse = new Button();
            lblHint = new Label();
            btnSave = new Button();
            btnCancel = new Button();
            grpCamera.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudLookback).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudWaitTimeout).BeginInit();
            SuspendLayout();
            // 
            // grpCamera
            // 
            grpCamera.Controls.Add(lblWaitTimeout);
            grpCamera.Controls.Add(lblLookback);
            grpCamera.Controls.Add(lblFilter);
            grpCamera.Controls.Add(lblWatchRoot);
            grpCamera.Controls.Add(chkIncludeSubfolders);
            grpCamera.Controls.Add(chkEnabled);
            grpCamera.Controls.Add(txtWatchRoot);
            grpCamera.Controls.Add(txtFilter);
            grpCamera.Controls.Add(nudLookback);
            grpCamera.Controls.Add(nudWaitTimeout);
            grpCamera.Controls.Add(btnBrowse);
            grpCamera.Location = new Point(12, 12);
            grpCamera.Name = "grpCamera";
            grpCamera.Padding = new Padding(14);
            grpCamera.Size = new Size(460, 300);
            grpCamera.TabIndex = 0;
            grpCamera.TabStop = false;
            grpCamera.Text = "Snapshot Camera";
            // 
            // lblWatchRoot
            // 
            lblWatchRoot.AutoSize = true;
            lblWatchRoot.Location = new Point(22, 50);
            lblWatchRoot.Name = "lblWatchRoot";
            lblWatchRoot.Size = new Size(100, 20);
            lblWatchRoot.TabIndex = 0;
            lblWatchRoot.Text = "Watch Root";
            // 
            // lblFilter
            // 
            lblFilter.AutoSize = true;
            lblFilter.Location = new Point(22, 92);
            lblFilter.Name = "lblFilter";
            lblFilter.Size = new Size(100, 20);
            lblFilter.TabIndex = 0;
            lblFilter.Text = "Filter";
            // 
            // lblLookback
            // 
            lblLookback.AutoSize = true;
            lblLookback.Location = new Point(22, 134);
            lblLookback.Name = "lblLookback";
            lblLookback.Size = new Size(100, 20);
            lblLookback.TabIndex = 0;
            lblLookback.Text = "Lookback (ms)";
            // 
            // lblWaitTimeout
            // 
            lblWaitTimeout.AutoSize = true;
            lblWaitTimeout.Location = new Point(22, 176);
            lblWaitTimeout.Name = "lblWaitTimeout";
            lblWaitTimeout.Size = new Size(100, 20);
            lblWaitTimeout.TabIndex = 0;
            lblWaitTimeout.Text = "Wait (ms)";
            // 
            // txtWatchRoot
            // 
            txtWatchRoot.Location = new Point(170, 46);
            txtWatchRoot.Name = "txtWatchRoot";
            txtWatchRoot.Size = new Size(220, 27);
            txtWatchRoot.TabIndex = 1;
            // 
            // btnBrowse
            // 
            btnBrowse.Location = new Point(396, 45);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Size = new Size(42, 29);
            btnBrowse.TabIndex = 2;
            btnBrowse.Text = "...";
            btnBrowse.UseVisualStyleBackColor = true;
            btnBrowse.Click += btnBrowse_Click;
            // 
            // txtFilter
            // 
            txtFilter.Location = new Point(170, 88);
            txtFilter.Name = "txtFilter";
            txtFilter.Size = new Size(220, 27);
            txtFilter.TabIndex = 3;
            // 
            // nudLookback
            // 
            nudLookback.Location = new Point(170, 130);
            nudLookback.Maximum = new decimal(new int[] { 60000, 0, 0, 0 });
            nudLookback.Name = "nudLookback";
            nudLookback.Size = new Size(220, 27);
            nudLookback.TabIndex = 4;
            // 
            // nudWaitTimeout
            // 
            nudWaitTimeout.Location = new Point(170, 172);
            nudWaitTimeout.Maximum = new decimal(new int[] { 60000, 0, 0, 0 });
            nudWaitTimeout.Name = "nudWaitTimeout";
            nudWaitTimeout.Size = new Size(220, 27);
            nudWaitTimeout.TabIndex = 5;
            // 
            // chkIncludeSubfolders
            // 
            chkIncludeSubfolders.AutoSize = true;
            chkIncludeSubfolders.Location = new Point(170, 214);
            chkIncludeSubfolders.Name = "chkIncludeSubfolders";
            chkIncludeSubfolders.Size = new Size(150, 24);
            chkIncludeSubfolders.TabIndex = 6;
            chkIncludeSubfolders.Text = "Include Subfolders";
            chkIncludeSubfolders.UseVisualStyleBackColor = true;
            // 
            // chkEnabled
            // 
            chkEnabled.AutoSize = true;
            chkEnabled.Location = new Point(170, 254);
            chkEnabled.Name = "chkEnabled";
            chkEnabled.Size = new Size(84, 24);
            chkEnabled.TabIndex = 7;
            chkEnabled.Text = "Enabled";
            chkEnabled.UseVisualStyleBackColor = true;
            // 
            // lblHint
            // 
            lblHint.ForeColor = SystemColors.GrayText;
            lblHint.Location = new Point(12, 318);
            lblHint.Name = "lblHint";
            lblHint.Size = new Size(460, 40);
            lblHint.TabIndex = 8;
            lblHint.Text = "Changes take effect after restarting the monitoring system.";
            // 
            // btnSave
            // 
            btnSave.Location = new Point(256, 370);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(100, 34);
            btnSave.TabIndex = 9;
            btnSave.Text = "Save";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // btnCancel
            // 
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(366, 370);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(100, 34);
            btnCancel.TabIndex = 10;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // SnapshotCameraSettingsForm
            // 
            AcceptButton = btnSave;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(484, 422);
            Controls.Add(btnCancel);
            Controls.Add(btnSave);
            Controls.Add(lblHint);
            Controls.Add(grpCamera);
            Name = "SnapshotCameraSettingsForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Snapshot Camera Settings";
            grpCamera.ResumeLayout(false);
            grpCamera.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudLookback).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudWaitTimeout).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox grpCamera;
        private Label lblWaitTimeout;
        private Label lblLookback;
        private Label lblFilter;
        private Label lblWatchRoot;
        private CheckBox chkIncludeSubfolders;
        private CheckBox chkEnabled;
        private TextBox txtWatchRoot;
        private TextBox txtFilter;
        private NumericUpDown nudLookback;
        private NumericUpDown nudWaitTimeout;
        private Button btnBrowse;
        private Label lblHint;
        private Button btnSave;
        private Button btnCancel;
    }
}
