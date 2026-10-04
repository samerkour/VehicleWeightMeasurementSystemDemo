using System;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Windows.Forms;

namespace SetupLauncher
{
    public partial class MainForm : Form
    {
        string rootPath = AppDomain.CurrentDomain.BaseDirectory;

        public MainForm()
        {
            InitializeComponent();
            //CheckAdmin();
        }

        private void CheckAdmin()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);

            if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                MessageBox.Show("Please run as Administrator", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }
        }

        private void RunProcess(string file, string args = "")
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = file,
                    Arguments = args,
                    UseShellExecute = true,
                    Verb = "runas",
                    WorkingDirectory = rootPath
                };

                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to start process:\n{ex.Message}");
            }
        }

        // 1️⃣ SQL INSTALL
        private void btnSql_Click(object sender, EventArgs e)
        {
            string cmdPath = Path.Combine(rootPath, "setup.cmd");

            if (!File.Exists(cmdPath))
            {
                MessageBox.Show($"setup.cmd not found:\n{cmdPath}");
                return;
            }

            RunProcess(
                "cmd.exe",
                $"/c \"\"{cmdPath}\"\""
            );
        }

        // 2️⃣ Vehicle System
        private void btnVehicle_Click(object sender, EventArgs e)
        {
            string exe = Path.Combine(rootPath, "Apps", "VehicleWeightMeasurementSystemSetup.exe");
            RunProcess(exe);
        }

        // 3️⃣ RmtoSync
        private void btnRmto_Click(object sender, EventArgs e)
        {
            string exe = Path.Combine(rootPath, "Apps", "RmtoSyncSetup.exe");
            RunProcess(exe);
        }

        // 4️⃣ Help
        private void btnHelp_Click(object sender, EventArgs e)
        {
            string help = Path.Combine(rootPath, "Help.pdf");

            if (!File.Exists(help))
            {
                MessageBox.Show("Help file not found.");
                return;
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = help,
                UseShellExecute = true
            });
        }
    }
}