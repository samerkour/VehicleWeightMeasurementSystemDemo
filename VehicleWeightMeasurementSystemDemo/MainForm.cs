using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.ComponentModel;
using System.Net;
using VehicleWeightMeasurementSystemDemo.Camera;
using VehicleWeightMeasurementSystemDemo.Data;
using VehicleWeightMeasurementSystemDemo.Models;
using VehicleWeightMeasurementSystemDemo.Services;
using static VehicleWeightMeasurementSystemDemo.Services.SATPA_API;

namespace VehicleWeightMeasurementSystemDemo
{
    public partial class MainForm : Form
    {
        private System.Windows.Forms.Timer _camTimer;
        private bool IsDesignMode =>
        LicenseManager.UsageMode == LicenseUsageMode.Designtime;


        private List<int> lineIds;

        private readonly Dictionary<int, Queue<VehicleDto>> _vehiclesByLine = new();
        private readonly Dictionary<int, Queue<string>> _imagesByLine = new();
        private readonly object _lock = new();

        private SerialPortService _serialService;
        private CameraWatcherService _cameraWatcher;
        private PlateRecognitionService _plateService;
        private IConfiguration _config;
        private SqlRepository _repo;


        private void ConfigureGrid()
        {
            dgvRecords.AutoGenerateColumns = false;
            dgvRecords.Columns.Clear();

            // PlateNumber
            dgvRecords.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "PlateNumber",
                HeaderText = "Plate",
                DataPropertyName = "PlateNumber",
                FillWeight = 15
            });

            // Speed
            dgvRecords.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Speed",
                HeaderText = "Speed",
                DataPropertyName = "Speed",
                FillWeight = 10
            });

            // LineName (NOT LineId)
            dgvRecords.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "LineName",
                HeaderText = "Line",
                DataPropertyName = "LineId",
                FillWeight = 10
            });

            // AxleCount
            dgvRecords.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "AxleCount",
                HeaderText = "AxleCount",
                DataPropertyName = "AxleCount",
                FillWeight = 10
            });

            // AxlesSummary 🔥 (25%)
            dgvRecords.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "AxlesSummary",
                HeaderText = "Axles Detail",
                DataPropertyName = "AxlesSummary",
                FillWeight = 25
            });

            // TotalWeight
            dgvRecords.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "TotalWeight",
                HeaderText = "TotalWeight",
                DataPropertyName = "TotalWeight",
                FillWeight = 15
            });

            // Timestamp
            dgvRecords.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Timestamp",
                HeaderText = "Time",
                DataPropertyName = "Timestamp",
                FillWeight = 15,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "yyyy-MM-dd HH:mm:ss"
                }
            });
        }

        public MainForm()
        {
            InitializeComponent();
        }

        public MainForm(IConfiguration config,
            SqlRepository repo,
            PlateRecognitionService plateService,
            SerialPortService serialService,
            CameraWatcherService cameraWatcher
            )
            : this()
        {
            _config = config;
            _repo = repo;

            _plateService = plateService;

            _serialService = serialService;
            _serialService.OnConnectionChanged += HandleSerialStatus;
            _serialService.OnDataReceived += async (raw) => await HandleSerialData(raw);



            _cameraWatcher = cameraWatcher;
            _cameraWatcher.OnImageCaptured += async (lineId, path) => await HandleImage(lineId, path);
            _cameraWatcher.OnStatusChanged += HandleCameraStatus;
        }


        private async void MainForm_Load(object sender, EventArgs e)
        {
            if (IsDesignMode)
                return;

          

            ConfigureGrid();
            Style();

            // 🔹 1. Load configurations
            var serialSettings = _config.GetSection("SerialPort").Get<SerialPortSettings>();
            var snapshotCameraSettings = _config.GetSection("SnapshotCamera").Get<SnapshotCameraSettings>();
            var ovarviewCamera = _config.GetSection("OverviewCamera").Get<OverviewCameraSettings>();


            // 🔹 3. Wire events
            if (serialSettings.Enabled)
            {
                // Start services
                _serialService.Start();
            }


            if (snapshotCameraSettings.Enabled)
            {

                 lineIds = await _repo.GetActiveLineIdsAsync();
                _cameraWatcher.Start(lineIds); // no need to pass path anymore


                _plateService.AddCamera(pictureBoxVehicle);
            }

 
            if (ovarviewCamera.Enabled)
            {

                _camTimer = new System.Windows.Forms.Timer();
                _camTimer.Interval = ovarviewCamera.RefreshIntervalMs;
                _camTimer.Tick += async (s, e) =>
                {
                    await LoadImage(
                     picCam1,
                     $"http://{ovarviewCamera.Host}{ovarviewCamera.PictureUrl}",
                     ovarviewCamera.Username,
                     ovarviewCamera.Password
                 );
                };
                _camTimer.Start();
            }

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


        //private async void HandleSerialData(string raw)
        //{
        //    try
        //    {
        //        var vehicle = VehicleParser.Parse(raw);
        //        CalculationService.CalculateDistances(vehicle);

        //        //_currentVehicle = vehicle; // 🔴 store latest vehicle


        //        lock (_lock)
        //        {
        //            if (!_vehiclesByLine.ContainsKey(vehicle.LineId))
        //                _vehiclesByLine[vehicle.LineId] = new Queue<VehicleDto>();

        //            _vehiclesByLine[vehicle.LineId].Enqueue(vehicle);
        //        }



        //        this.Invoke(() =>
        //        {
        //            lblSpeed.Text = $"Speed: {vehicle.Speed} km/h";
        //            lblLine.Text = $"Line: {vehicle.LineId}";
        //            lblAxles.Text = $"Axles No: {vehicle.AxleCount}";
        //            lblTotalWeight.Text = $"TotalWeight: {vehicle.TotalWeight}";
        //            lblADC1.Text = $"ADC1: {vehicle.ADC1}";
        //            lblADC2.Text = $"ADC2: {vehicle.ADC2}";
        //            lblADC3.Text = $"ADC3: {vehicle.ADC3}";
        //            lblADC4.Text = $"ADC4: {vehicle.ADC4}";

        //            dgvAxles.DataSource = null;
        //            dgvAxles.DataSource = vehicle.Axles;
        //        });

        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error(ex, "Error processing serial data: {Raw}", raw);
        //    }

        //}

        private async Task LoadImage(PictureBox pic, string url, string user, string pass)
        {
            try
            {
                var handler = new HttpClientHandler
                {
                    Credentials = new NetworkCredential(user, pass)
                };

                using var client = new HttpClient(handler);

                var bytes = await client.GetByteArrayAsync(url);

                using var ms = new MemoryStream(bytes);
                var img = Image.FromStream(ms);

                pic.Invoke(() =>
                {
                    pic.Image?.Dispose(); // 🔥 مهم
                    pic.Image = new Bitmap(img);
                });
            }
            catch (Exception ex)
            {
                // اگر قطع شد، تصویر سیاه کن
                pic.Invoke(() =>
                {
                    pic.BackColor = Color.DarkRed;
                });

                Log.Warning("Camera error: {Message}", ex.Message);
            }
        }

        private void UpdateVehicleUI(VehicleDto vehicle)
        {
            lblSpeed.Text = $"Speed: {vehicle.Speed} km/h";
            lblLine.Text = $"Line: {vehicle.LineId}";
            lblAxles.Text = $"Axles No: {vehicle.AxleCount}";
            lblTotalWeight.Text = $"TotalWeight: {vehicle.TotalWeight}";

            dgvAxles.DataSource = null;
            dgvAxles.DataSource = vehicle.Axles;
        }

        private void UpdateImageUI(string imagePath, PlateResultDto plate)
        {
            using (var fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var ms = new MemoryStream())
            {
                fs.CopyTo(ms);
                ms.Position = 0;

                var img = Image.FromStream(ms);

                pictureBoxVehicle.Image?.Dispose();
                pictureBoxVehicle.Image = new Bitmap(img);
            }

            lblDetectedPlate.Text = $"Detected Plate No: {plate?.PlateNumber}";
        }

        private async Task HandleSerialData(string raw)
        {
            try
            {
                var vehicle = VehicleParser.Parse(raw);
                CalculationService.CalculateDistances(vehicle);

                // 🔹 Map ADC → Axles
                var adcList = new List<string?>
                {
                    vehicle.ADC1,
                    vehicle.ADC2,
                    vehicle.ADC3,
                    vehicle.ADC4
                };

                for (int i = 0; i < vehicle.Axles.Count; i++)
                {
                    var axle = vehicle.Axles[i];

                    axle.ADCDisplay =
                        i < adcList.Count && !string.IsNullOrWhiteSpace(adcList[i])
                        ? adcList[i]
                        : "-";
                }

                // 🔹 UI Update (SAFE)
                if (InvokeRequired)
                {
                    await InvokeAsync(() => UpdateVehicleUI(vehicle));
                }
                else
                {
                    UpdateVehicleUI(vehicle);
                }

                string imagePath = await WaitForImageAsync(vehicle.LineId);

                if (imagePath == null)
                {
                    Log.Warning("❌ No image found for Line {LineId}", vehicle.LineId);
                }

                var plate = _plateService.Extract(imagePath);

                await _repo.SaveAsync(vehicle, imagePath, plate);
                await LoadGrid();

                if (!string.IsNullOrEmpty(imagePath))
                {
                    if (InvokeRequired)
                    {
                        await InvokeAsync(() => UpdateImageUI(imagePath, plate));
                    }
                    else
                    {
                        UpdateImageUI(imagePath, plate);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error processing serial data: {Raw}", raw);
            }
        }


        private async Task HandleImage(int lineId, string path)
        {
            try
            {
                lock (_lock)
                {
                    if (!_imagesByLine.ContainsKey(lineId))
                        _imagesByLine[lineId] = new Queue<string>();

                    _imagesByLine[lineId].Enqueue(path);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error queueing image: {Path}", path);
            }
        }

        private async Task<string?> WaitForImageAsync(int lineId, int timeoutMs = 2000)
        {
            var start = DateTime.Now;

            while ((DateTime.Now - start).TotalMilliseconds < timeoutMs)
            {
                lock (_lock)
                {
                    if (_imagesByLine.ContainsKey(lineId) &&
                        _imagesByLine[lineId].Count > 0)
                    {
                        return _imagesByLine[lineId].Dequeue();
                    }
                }

                await Task.Delay(50);
            }

            return null;
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
            _cameraWatcher?.Start(lineIds);
        }

        private void stopSystemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _serialService?.Stop();
            _cameraWatcher?.Stop();
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
