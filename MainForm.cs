using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.ComponentModel;
using VehicleWeightMeasurementSystemDemo.Camera;
using VehicleWeightMeasurementSystemDemo.Data;
using VehicleWeightMeasurementSystemDemo.Models;
using VehicleWeightMeasurementSystemDemo.Services;

namespace VehicleWeightMeasurementSystemDemo
{
    public partial class MainForm : Form
    {
        private bool IsDesignMode =>
        LicenseManager.UsageMode == LicenseUsageMode.Designtime;

        private VehicleDto _currentVehicle;

        private SerialPortService _serialService;
        private CameraWatcherService _cameraService;
        private PlateRecognitionService _plateService;
        private SqlRepository _repo;

        private void ConfigureGrids()
        {
            dgvAxles.AutoGenerateColumns = false;
            dgvAxles.Columns.Clear();

            dgvAxles.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Axle",
                HeaderText = "Axle",
                DataPropertyName = "Index",
                Width = 60
            });

            dgvAxles.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Weight",
                HeaderText = "Weight (kg)",
                DataPropertyName = "Weight",
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N0" }
            });

            dgvAxles.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Time",
                HeaderText = "Time (ms)",
                DataPropertyName = "DisplayTime"
            });

            dgvAxles.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Distance",
                HeaderText = "Distance (m)",
                DataPropertyName = "DisplayDistance"
            });
        }

        private void ConfigureRTL(DataGridView grid)
        {
            grid.DefaultCellStyle.Font = new Font("B Nazanin", 10);
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("B Nazanin", 10, FontStyle.Bold);
            grid.DefaultCellStyle.Font = new Font("B Nazanin", 10);

            grid.RightToLeft = RightToLeft.Yes;

            // Align headers
            grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            // Align cell content
            grid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            // Optional: better readability
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Prevent weird selection visuals in RTL
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }

        public MainForm()
        {
            InitializeComponent();
        }

        public MainForm(SqlRepository repo,
            PlateRecognitionService plateService
            //,
            //SerialPortService serial,
            //CameraWatcherService camera
            )
            : this()
        {
            _repo = repo;
            _plateService = plateService;
            //_serialService = serial;
            //_cameraService = camera;
        }


        private async void MainForm_Load(object sender, EventArgs e)
        {
            if (IsDesignMode)
                return;

            Style();

            _plateService.AddCamera(pictureBoxVehicle);

            //ConfigureRTL(dgvAxles);
            //ConfigureRTL(dgvRecords);

            // 🔹 1. Load configuration (appsettings.json)
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var serialSettings = config.GetSection("SerialPort").Get<SerialPortSettings>();
            var cameraSettings = config.GetSection("Camera").Get<CameraSettings>();

            //var connectionString = config.GetConnectionString("DefaultConnection");
            //var options = new DbContextOptionsBuilder<AppDbContext>()
            //.UseSqlServer(connectionString)
            //.Options;

            //var dbContext = new AppDbContext(options);

            // 🔹 2. Create services using settings
            _serialService = new SerialPortService(serialSettings);
            _cameraService = new CameraWatcherService(cameraSettings);
            //_plateService = new PlateRecognitionService(new SatpaRecognitionEngine());
            //_repo = new SqlRepository(dbContext);

            // 🔹 3. Wire events
            _serialService.OnConnectionChanged += HandleSerialStatus;
            _serialService.OnDataReceived += HandleSerialData;

            _cameraService.OnStatusChanged += HandleCameraStatus;
            _cameraService.OnImageCaptured += HandleImage;

            // 🔹 4. Start services
            _serialService.Start();
            _cameraService.Start(); // no need to pass path anymore


            await LoadGrid(); // 🔴 load existing data
        }

        private void Style()
        {
            this.BackColor = UITheme.Background;
            this.Font = new Font("Segoe UI", 10);


            lblSerialStatus.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblSerialStatus.ForeColor = UITheme.Success;

            lblCameraStatus.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblCameraStatus.ForeColor = UITheme.Success;

            grpVehicleInfo.BackColor = UITheme.CardBack;
            grpVehicleInfo.ForeColor = Color.Black;

            grpImage.BackColor = UITheme.CardBack;
            grpImage.ForeColor = Color.Black;

            pictureBoxVehicle.BackColor = Color.Black;
            pictureBoxVehicle.Padding = new Padding(5);

            lblDetectedPlate.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            lblDetectedPlate.ForeColor = UITheme.Success;

            StyleGrid(dgvAxles);
            StyleGrid(dgvRecords);

            grpVehicleInfo.Padding = new Padding(10);
            grpImage.Padding = new Padding(10);
        }

        private void StyleGrid(DataGridView grid)
        {
            grid.BackgroundColor = Color.White;
            grid.BorderStyle = BorderStyle.None;
            grid.EnableHeadersVisualStyles = false;

            grid.ColumnHeadersDefaultCellStyle.BackColor = UITheme.HeaderBack;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(200, 230, 201);
            grid.DefaultCellStyle.SelectionForeColor = Color.Black;

            grid.RowTemplate.Height = 30;
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 249, 250);

            grid.GridColor = UITheme.Border;

        }

        private async void HandleSerialData(string raw)
        {
            try
            {
                var vehicle = VehicleParser.Parse(raw);
                CalculationService.CalculateDistances(vehicle);


                _currentVehicle = vehicle; // 🔴 store latest vehicle


                this.Invoke(() =>
                {
                    lblSpeed.Text = $"Speed: {vehicle.Speed} km/h";
                    lblLine.Text = $"Line: {vehicle.Line}";
                    lblAxles.Text = $"Axles No: {vehicle.AxleCount}";
                    lblTotalWeight.Text = $"TotalWeight: {vehicle.TotalWeight}";
                    lblADC1.Text = $"ADC1: {vehicle.ADC1}";
                    lblADC2.Text = $"ADC2: {vehicle.ADC2}";
                    lblADC3.Text = $"ADC3: {vehicle.ADC3}";
                    lblADC4.Text = $"ADC4: {vehicle.ADC4}";

                    dgvAxles.DataSource = null;
                    dgvAxles.DataSource = vehicle.Axles;
                });


                await Task.Delay(500);

                // 🔴 SAVE to DB
                await _repo.SaveAsync(_currentVehicle);

                // 🔴 REFRESH GRID
                await LoadGrid();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error processing serial data: {Raw}", raw);
            }

        }

        private async void HandleImage(string path)
        {
            try
            {
                //if (_currentVehicle == null)
                //    return; // no serial data yet

                PlateResultDto plate = _plateService.Extract(path);

                _currentVehicle.PlateNumber = plate.PlateNumber;

                this.Invoke(() =>
                {
                    pictureBoxVehicle.Image = Image.FromFile(path);
                    lblDetectedPlate.Text = $"Plate Number: {plate.PlateNumber}";
                });

                //// 🔴 SAVE to DB
                //await _repo.SaveAsync(_currentVehicle);

                //// 🔴 REFRESH GRID
                //await LoadGrid();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error processing image: {Path}", path);
            }
        }

        private async Task LoadGrid()
        {
            var data = await _repo.GetAllAsync();

            if (this.InvokeRequired)
            {
                this.Invoke(() =>
                {
                    dgvRecords.DataSource = data;
                });
            }
            else
            {
                dgvRecords.DataSource = data;
            }
        }

        private void dgvRecords_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            foreach (DataGridViewRow row in dgvRecords.Rows)
            {
                if (row.IsNewRow) continue;

                var plateValue = row.Cells["PlateNumber"].Value?.ToString();

                //if (string.IsNullOrWhiteSpace(plateValue) || plateValue == "---")
                //{
                //    row.DefaultCellStyle.BackColor = Color.LightPink;
                //}
                //else
                //{
                //    row.DefaultCellStyle.BackColor = Color.LightGreen;
                //}

                if (string.IsNullOrWhiteSpace(plateValue) || plateValue == "---")
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 235, 238); // soft red
                }
                else
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(232, 245, 233); // soft green
                }
            }
        }



        private void HandleSerialStatus(bool connected)
        {
            this.Invoke(() =>
            {
                if (connected)
                {
                    lblSerialStatus.Text = "Serial Port: ● Connected";
                    lblSerialStatus.ForeColor = Color.Green;
                }
                else
                {
                    lblSerialStatus.Text = "Serial Port: ● Disconnected";
                    lblSerialStatus.ForeColor = Color.Red;
                }
            });
        }

        private void HandleCameraStatus(bool ok)
        {
            if (InvokeRequired)
            {
                Invoke(() => UpdateCameraStatus(ok));
            }
            else
            {
                UpdateCameraStatus(ok);
            }
        }

        private void UpdateCameraStatus(bool ok)
        {
            if (ok)
            {
                lblCameraStatus.Text = "Camera Folder: ● Monitoring";
                lblCameraStatus.ForeColor = Color.Green;
                // optional:
                // lblCameraStatus.BackColor = Color.LightGreen;
            }
            else
            {
                lblCameraStatus.Text = "Camera Folder: ● Not Available";
                lblCameraStatus.ForeColor = Color.Red;
                // optional:
                // lblCameraStatus.BackColor = Color.LightPink;
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void startSystemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _serialService?.Start();
            _cameraService?.Start();
        }

        private void stopSystemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _serialService?.Stop();
            _cameraService?.Stop();
        }

        private void dailyReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowDailyReport();
        }

        private void monthlyReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowMonthlyReport();
        }

        private void ShowMonthlyReport()
        {
            MessageBox.Show("Monthly Report Coming Soon...");
        }

        private void overweightVehiclesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Overweight Vehicles Report Coming Soon...");
        }

        private void ShowDailyReport()
        {
            MessageBox.Show("Daily Report Coming Soon...");
        }


        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Vehicle Weight System v1.0");
        }
    }
}
