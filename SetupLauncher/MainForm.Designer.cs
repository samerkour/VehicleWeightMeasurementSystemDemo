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
        private System.Windows.Forms.Button btnSql;
        private System.Windows.Forms.Button btnVehicle;
        private System.Windows.Forms.Button btnRmto;
        private System.Windows.Forms.Button btnHelp;

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            btnSql = new Button();
            btnVehicle = new Button();
            btnRmto = new Button();
            btnHelp = new Button();
            SuspendLayout();
            // 
            // btnSql
            // 
            btnSql.Location = new Point(125, 607);
            btnSql.Name = "btnSql";
            btnSql.Size = new Size(185, 40);
            btnSql.TabIndex = 0;
            btnSql.Text = "Install SQL Server";
            btnSql.Click += btnSql_Click;
            // 
            // btnVehicle
            // 
            btnVehicle.Location = new Point(382, 607);
            btnVehicle.Name = "btnVehicle";
            btnVehicle.Size = new Size(206, 40);
            btnVehicle.TabIndex = 1;
            btnVehicle.Text = "Install Vehicle System";
            btnVehicle.Click += btnVehicle_Click;
            // 
            // btnRmto
            // 
            btnRmto.Location = new Point(660, 607);
            btnRmto.Name = "btnRmto";
            btnRmto.Size = new Size(172, 40);
            btnRmto.TabIndex = 2;
            btnRmto.Text = "Install RmtoSync";
            btnRmto.Click += btnRmto_Click;
            // 
            // btnHelp
            // 
            btnHelp.Location = new Point(904, 607);
            btnHelp.Name = "btnHelp";
            btnHelp.Size = new Size(172, 40);
            btnHelp.TabIndex = 3;
            btnHelp.Text = "Help";
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
            MaximizeBox = false;
            Name = "MainForm";
            Text = "System Installer";
            ResumeLayout(false);
        }

        #endregion
    }
}
