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
            this.btnSql = new Button();
            this.btnVehicle = new Button();
            this.btnRmto = new Button();
            this.btnHelp = new Button();

            this.SuspendLayout();

            btnSql.Text = "Install SQL Server";
            btnSql.SetBounds(30, 30, 250, 40);
            btnSql.Click += btnSql_Click;

            btnVehicle.Text = "Install Vehicle System";
            btnVehicle.SetBounds(30, 80, 250, 40);
            btnVehicle.Click += btnVehicle_Click;

            btnRmto.Text = "Install RmtoSync";
            btnRmto.SetBounds(30, 130, 250, 40);
            btnRmto.Click += btnRmto_Click;

            btnHelp.Text = "Help";
            btnHelp.SetBounds(30, 180, 250, 40);
            btnHelp.Click += btnHelp_Click;

            this.Controls.AddRange(new Control[] {
                btnSql, btnVehicle, btnRmto, btnHelp
            });

            this.Text = "System Installer";
            this.ClientSize = new System.Drawing.Size(320, 260);

            this.ResumeLayout(false);
        }

        #endregion
    }
}
