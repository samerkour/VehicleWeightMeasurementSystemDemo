namespace VehicleWeightMeasurementSystemDemo
{
    partial class DatabaseSettingsForm
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
            grpDb = new GroupBox();
            lblDatabase = new Label();
            lblServer = new Label();
            lblUsername = new Label();
            lblPassword = new Label();
            chkTrusted = new CheckBox();
            txtServer = new TextBox();
            txtDatabase = new TextBox();
            txtUsername = new TextBox();
            txtPassword = new TextBox();
            chkTrustServerCertificate = new CheckBox();
            lblFull = new Label();
            txtConnectionString = new TextBox();
            lblHint = new Label();
            btnTest = new Button();
            btnSave = new Button();
            btnCancel = new Button();
            grpDb.SuspendLayout();
            SuspendLayout();
            // 
            // grpDb
            // 
            grpDb.Controls.Add(chkTrustServerCertificate);
            grpDb.Controls.Add(txtPassword);
            grpDb.Controls.Add(txtUsername);
            grpDb.Controls.Add(lblPassword);
            grpDb.Controls.Add(lblUsername);
            grpDb.Controls.Add(chkTrusted);
            grpDb.Controls.Add(txtDatabase);
            grpDb.Controls.Add(lblDatabase);
            grpDb.Controls.Add(txtServer);
            grpDb.Controls.Add(lblServer);
            grpDb.Location = new Point(12, 12);
            grpDb.Name = "grpDb";
            grpDb.Padding = new Padding(14);
            grpDb.Size = new Size(480, 280);
            grpDb.TabIndex = 0;
            grpDb.TabStop = false;
            grpDb.Text = "Database Connection";
            // 
            // lblServer
            // 
            lblServer.AutoSize = true;
            lblServer.Location = new Point(22, 52);
            lblServer.Name = "lblServer";
            lblServer.Size = new Size(100, 20);
            lblServer.TabIndex = 0;
            lblServer.Text = "Server";
            // 
            // lblDatabase
            // 
            lblDatabase.AutoSize = true;
            lblDatabase.Location = new Point(22, 94);
            lblDatabase.Name = "lblDatabase";
            lblDatabase.Size = new Size(100, 20);
            lblDatabase.TabIndex = 0;
            lblDatabase.Text = "Database";
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
            // txtServer
            // 
            txtServer.Location = new Point(160, 48);
            txtServer.Name = "txtServer";
            txtServer.Size = new Size(250, 27);
            txtServer.TabIndex = 1;
            // 
            // txtDatabase
            // 
            txtDatabase.Location = new Point(160, 90);
            txtDatabase.Name = "txtDatabase";
            txtDatabase.Size = new Size(250, 27);
            txtDatabase.TabIndex = 2;
            // 
            // txtUsername
            // 
            txtUsername.Location = new Point(160, 132);
            txtUsername.Name = "txtUsername";
            txtUsername.Size = new Size(250, 27);
            txtUsername.TabIndex = 3;
            // 
            // txtPassword
            // 
            txtPassword.Location = new Point(160, 174);
            txtPassword.Name = "txtPassword";
            txtPassword.PasswordChar = '*';
            txtPassword.Size = new Size(250, 27);
            txtPassword.TabIndex = 4;
            // 
            // chkTrusted
            // 
            chkTrusted.AutoSize = true;
            chkTrusted.Location = new Point(62, 216);
            chkTrusted.Name = "chkTrusted";
            chkTrusted.Size = new Size(150, 24);
            chkTrusted.TabIndex = 5;
            chkTrusted.Text = "Integrated Security";
            chkTrusted.UseVisualStyleBackColor = true;
            chkTrusted.CheckedChanged += chkTrusted_CheckedChanged;
            // 
            // chkTrustServerCertificate
            // 
            chkTrustServerCertificate.AutoSize = true;
            chkTrustServerCertificate.Checked = true;
            chkTrustServerCertificate.CheckState = CheckState.Checked;
            chkTrustServerCertificate.Location = new Point(250, 216);
            chkTrustServerCertificate.Name = "chkTrustServerCertificate";
            chkTrustServerCertificate.Size = new Size(190, 24);
            chkTrustServerCertificate.TabIndex = 6;
            chkTrustServerCertificate.Text = "Trust Server Certificate";
            chkTrustServerCertificate.UseVisualStyleBackColor = true;
            // 
            // lblFull
            // 
            lblFull.AutoSize = true;
            lblFull.Location = new Point(12, 300);
            lblFull.Name = "lblFull";
            lblFull.Size = new Size(120, 20);
            lblFull.TabIndex = 7;
            lblFull.Text = "Connection String";
            // 
            // txtConnectionString
            // 
            txtConnectionString.Location = new Point(12, 326);
            txtConnectionString.Multiline = true;
            txtConnectionString.Name = "txtConnectionString";
            txtConnectionString.ReadOnly = true;
            txtConnectionString.ScrollBars = ScrollBars.Vertical;
            txtConnectionString.Size = new Size(480, 66);
            txtConnectionString.TabIndex = 8;
            // 
            // lblHint
            // 
            lblHint.ForeColor = SystemColors.GrayText;
            lblHint.Location = new Point(12, 400);
            lblHint.Name = "lblHint";
            lblHint.Size = new Size(480, 40);
            lblHint.TabIndex = 9;
            lblHint.Text = "Changes take effect after restarting the monitoring system.";
            // 
            // btnTest
            // 
            btnTest.Location = new Point(12, 450);
            btnTest.Name = "btnTest";
            btnTest.Size = new Size(100, 34);
            btnTest.TabIndex = 10;
            btnTest.Text = "Test";
            btnTest.UseVisualStyleBackColor = true;
            btnTest.Click += btnTest_Click;
            // 
            // btnSave
            // 
            btnSave.Location = new Point(286, 450);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(100, 34);
            btnSave.TabIndex = 11;
            btnSave.Text = "Save";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // btnCancel
            // 
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(392, 450);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(100, 34);
            btnCancel.TabIndex = 12;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // DatabaseSettingsForm
            // 
            AcceptButton = btnSave;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(504, 500);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Controls.Add(btnCancel);
            Controls.Add(btnSave);
            Controls.Add(btnTest);
            Controls.Add(lblHint);
            Controls.Add(txtConnectionString);
            Controls.Add(lblFull);
            Controls.Add(grpDb);
            Name = "DatabaseSettingsForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Database Settings";
            grpDb.ResumeLayout(false);
            grpDb.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private GroupBox grpDb;
        private Label lblPassword;
        private Label lblUsername;
        private Label lblDatabase;
        private Label lblServer;
        private CheckBox chkTrusted;
        private TextBox txtPassword;
        private TextBox txtUsername;
        private TextBox txtDatabase;
        private TextBox txtServer;
        private CheckBox chkTrustServerCertificate;
        private Label lblFull;
        private TextBox txtConnectionString;
        private Label lblHint;
        private Button btnTest;
        private Button btnSave;
        private Button btnCancel;
    }
}