namespace VehicleWeightMeasurementSystemDemo
{
    partial class OverviewCameraSettingsForm
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
            lblRefresh = new Label();
            lblPassword = new Label();
            lblUsername = new Label();
            lblPictureUrl = new Label();
            lblHost = new Label();
            chkEnabled = new CheckBox();
            txtHost = new TextBox();
            txtPictureUrl = new TextBox();
            txtUsername = new TextBox();
            txtPassword = new TextBox();
            nudRefresh = new NumericUpDown();
            lblHint = new Label();
            btnSave = new Button();
            btnCancel = new Button();
            grpCamera.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudRefresh).BeginInit();
            SuspendLayout();
            // 
            // grpCamera
            // 
            grpCamera.Controls.Add(lblRefresh);
            grpCamera.Controls.Add(lblPassword);
            grpCamera.Controls.Add(lblUsername);
            grpCamera.Controls.Add(lblPictureUrl);
            grpCamera.Controls.Add(lblHost);
            grpCamera.Controls.Add(chkEnabled);
            grpCamera.Controls.Add(txtHost);
            grpCamera.Controls.Add(txtPictureUrl);
            grpCamera.Controls.Add(txtUsername);
            grpCamera.Controls.Add(txtPassword);
            grpCamera.Controls.Add(nudRefresh);
            grpCamera.Location = new Point(12, 12);
            grpCamera.Name = "grpCamera";
            grpCamera.Padding = new Padding(14);
            grpCamera.Size = new Size(480, 330);
            grpCamera.TabIndex = 0;
            grpCamera.TabStop = false;
            grpCamera.Text = "Overview Camera";
            // 
            // lblHost
            // 
            lblHost.AutoSize = true;
            lblHost.Location = new Point(22, 52);
            lblHost.Name = "lblHost";
            lblHost.Size = new Size(100, 20);
            lblHost.TabIndex = 0;
            lblHost.Text = "Host";
            // 
            // lblPictureUrl
            // 
            lblPictureUrl.AutoSize = true;
            lblPictureUrl.Location = new Point(22, 94);
            lblPictureUrl.Name = "lblPictureUrl";
            lblPictureUrl.Size = new Size(100, 20);
            lblPictureUrl.TabIndex = 0;
            lblPictureUrl.Text = "Picture URL";
            // 
            // lblUsername
            // 
            lblUsername.AutoSize = true;
            lblUsername.Location = new Point(22, 136);
            lblUsername.Name = "lblUsername";
            lblUsername.Size = new Size(100, 20);
            lblUsername.TabIndex = 0;
            lblUsername.Text = "Username";
            // 
            // lblPassword
            // 
            lblPassword.AutoSize = true;
            lblPassword.Location = new Point(22, 178);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(100, 20);
            lblPassword.TabIndex = 0;
            lblPassword.Text = "Password";
            // 
            // lblRefresh
            // 
            lblRefresh.AutoSize = true;
            lblRefresh.Location = new Point(22, 220);
            lblRefresh.Name = "lblRefresh";
            lblRefresh.Size = new Size(100, 20);
            lblRefresh.TabIndex = 0;
            lblRefresh.Text = "Refresh (ms)";
            // 
            // txtHost
            // 
            txtHost.Location = new Point(180, 48);
            txtHost.Name = "txtHost";
            txtHost.Size = new Size(250, 27);
            txtHost.TabIndex = 1;
            // 
            // txtPictureUrl
            // 
            txtPictureUrl.Location = new Point(180, 90);
            txtPictureUrl.Name = "txtPictureUrl";
            txtPictureUrl.Size = new Size(250, 27);
            txtPictureUrl.TabIndex = 2;
            // 
            // txtUsername
            // 
            txtUsername.Location = new Point(180, 132);
            txtUsername.Name = "txtUsername";
            txtUsername.Size = new Size(250, 27);
            txtUsername.TabIndex = 3;
            // 
            // txtPassword
            // 
            txtPassword.Location = new Point(180, 174);
            txtPassword.Name = "txtPassword";
            txtPassword.PasswordChar = '*';
            txtPassword.Size = new Size(250, 27);
            txtPassword.TabIndex = 4;
            // 
            // nudRefresh
            // 
            nudRefresh.Location = new Point(180, 216);
            nudRefresh.Maximum = new decimal(new int[] { 60000, 0, 0, 0 });
            nudRefresh.Minimum = new decimal(new int[] { 100, 0, 0, 0 });
            nudRefresh.Name = "nudRefresh";
            nudRefresh.Size = new Size(250, 27);
            nudRefresh.TabIndex = 5;
            nudRefresh.Value = new decimal(new int[] { 700, 0, 0, 0 });
            // 
            // chkEnabled
            // 
            chkEnabled.AutoSize = true;
            chkEnabled.Location = new Point(180, 258);
            chkEnabled.Name = "chkEnabled";
            chkEnabled.Size = new Size(84, 24);
            chkEnabled.TabIndex = 6;
            chkEnabled.Text = "Enabled";
            chkEnabled.UseVisualStyleBackColor = true;
            // 
            // lblHint
            // 
            lblHint.ForeColor = SystemColors.GrayText;
            lblHint.Location = new Point(12, 348);
            lblHint.Name = "lblHint";
            lblHint.Size = new Size(480, 40);
            lblHint.TabIndex = 7;
            lblHint.Text = "Changes take effect after restarting the monitoring system.";
            // 
            // btnSave
            // 
            btnSave.Location = new Point(276, 400);
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
            btnCancel.Location = new Point(386, 400);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(100, 34);
            btnCancel.TabIndex = 9;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // OverviewCameraSettingsForm
            // 
            AcceptButton = btnSave;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(504, 452);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Controls.Add(btnCancel);
            Controls.Add(btnSave);
            Controls.Add(lblHint);
            Controls.Add(grpCamera);
            Name = "OverviewCameraSettingsForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Overview Camera Settings";
            grpCamera.ResumeLayout(false);
            grpCamera.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudRefresh).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox grpCamera;
        private Label lblRefresh;
        private Label lblPassword;
        private Label lblUsername;
        private Label lblPictureUrl;
        private Label lblHost;
        private CheckBox chkEnabled;
        private TextBox txtHost;
        private TextBox txtPictureUrl;
        private TextBox txtUsername;
        private TextBox txtPassword;
        private NumericUpDown nudRefresh;
        private Label lblHint;
        private Button btnSave;
        private Button btnCancel;
    }
}
