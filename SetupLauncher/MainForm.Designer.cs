namespace SetupLauncher
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
        private System.Windows.Forms.Label btnSql;
        private System.Windows.Forms.Label btnVehicle;
        private System.Windows.Forms.Label btnRmto;
        private System.Windows.Forms.Label btnHelp;

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            btnSql = new Label();
            btnVehicle = new Label();
            btnRmto = new Label();
            btnHelp = new Label();
            SuspendLayout();
            // 
            // btnSql
            // 
            btnSql.BackColor = Color.Transparent;
            btnSql.FlatStyle = FlatStyle.Flat;
            btnSql.Location = new Point(125, 553);
            btnSql.Name = "btnSql";
            btnSql.Size = new Size(172, 94);
            btnSql.TabIndex = 0;
            btnSql.Click += btnSql_Click;
            // 
            // btnVehicle
            // 
            btnVehicle.BackColor = Color.Transparent;
            btnVehicle.FlatStyle = FlatStyle.Flat;
            btnVehicle.Location = new Point(382, 553);
            btnVehicle.Name = "btnVehicle";
            btnVehicle.Size = new Size(172, 94);
            btnVehicle.TabIndex = 1;
            btnVehicle.Click += btnVehicle_Click;
            // 
            // btnRmto
            // 
            btnRmto.BackColor = Color.Transparent;
            btnRmto.FlatStyle = FlatStyle.Flat;
            btnRmto.Location = new Point(655, 553);
            btnRmto.Name = "btnRmto";
            btnRmto.Size = new Size(172, 94);
            btnRmto.TabIndex = 2;
            btnRmto.Click += btnRmto_Click;
            // 
            // btnHelp
            // 
            btnHelp.BackColor = Color.Transparent;
            btnHelp.FlatStyle = FlatStyle.Flat;
            btnHelp.Location = new Point(904, 553);
            btnHelp.Name = "btnHelp";
            btnHelp.Size = new Size(172, 94);
            btnHelp.TabIndex = 3;
            btnHelp.Click += btnHelp_Click;
            // 
            // MainForm
            // 
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            ClientSize = new Size(1232, 703);
            Controls.Add(btnSql);
            Controls.Add(btnVehicle);
            Controls.Add(btnRmto);
            Controls.Add(btnHelp);
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            MaximizeBox = false;
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "System Installer";
            ResumeLayout(false);
        }

        #endregion
    }
}
