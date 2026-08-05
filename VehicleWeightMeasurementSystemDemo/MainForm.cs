using Microsoft.Extensions.Configuration;
using Serilog;
using System.ComponentModel;
using System.Net;
using VehicleWeightMeasurementSystemDemo.ApplicationLayer.Abstractions;
using VehicleWeightMeasurementSystemDemo.ApplicationLayer.Configuration;
using VehicleWeightMeasurementSystemDemo.ApplicationLayer.Parsing;
using VehicleWeightMeasurementSystemDemo.Domain.Services;
using VehicleWeightMeasurementSystemDemo.Domain.Weighing;
using VehicleWeightMeasurementSystemDemo.Infrastructure.Cameras;
using VehicleWeightMeasurementSystemDemo.Infrastructure.Persistence;
using VehicleWeightMeasurementSystemDemo.Infrastructure.Serial;
using VehicleWeightMeasurementSystemDemo.Theming;

namespace VehicleWeightMeasurementSystemDemo
{
    public partial class MainForm : Form
    {
        private bool IsDesignMode =>
        LicenseManager.UsageMode == LicenseUsageMode.Designtime;

        private CancellationTokenSource? _cameraCts;

        // وضعیت اجرا / خاموش‌شدن
        private volatile bool _systemRunning;
        private volatile bool _shuttingDown;

        private List<int> lineIds = new();
        private readonly Dictionary<int, Queue<PendingImage>> _imagesByLine = new();
        private readonly object _lock = new();

        // حداکثر عمر مجاز یک عکس در صف (برای جلوگیری از drift دائمی)
        private const int ImageStaleMs = 5000;
        private const int MaxQueuePerLine = 20;

        private SerialPortService _serialService;
        private CameraWatcherService _cameraWatcher;
        private IPlateRecognitionService _plateService;
        private IConfiguration _config;
        private SqlRepository _repo;

        // تنظیمات نگه‌داشته‌شده برای Start/Stop از منو
        private OverviewCameraSettings? _overviewSettings;
        private bool _serialEnabled;
        private bool _snapshotEnabled;

        // ضریب اعمال‌شده روی مقادیر وزن قبل از نمایش (پیش‌فرض 1.5 از تنظیمات)
        private decimal _alpha = 1.5m;

        private sealed class PendingImage
        {
            public string Path { get; init; } = string.Empty;
            public DateTime CapturedAtUtc { get; init; }
        }

        private void ConfigureGrid()
        {
            dgvRecords.AutoGenerateColumns = false;
            dgvRecords.Columns.Clear();

            // PlateNumber
            dgvRecords.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Id",
                HeaderText = "Seq",
                DataPropertyName = "Id",
                FillWeight = 12
            });

            // PlateNumber
            dgvRecords.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "PlateNumber",
                HeaderText = "Plate",
                DataPropertyName = "PlateNumber",
                FillWeight = 20
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
                HeaderText = "Axle Count",
                DataPropertyName = "AxleCount",
                FillWeight = 10
            });

            dgvRecords.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "W1",
                HeaderText = "W1(kg)",
                DataPropertyName = "AxleWeight1",
                FillWeight = 15
            });

            dgvRecords.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "W2",
                HeaderText = "W2(kg)",
                DataPropertyName = "AxleWeight2",
                FillWeight = 15
            });

            dgvRecords.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "W3",
                HeaderText = "W3(kg)",
                DataPropertyName = "AxleWeight3",
                FillWeight = 15
            });

            dgvRecords.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "W4",
                HeaderText = "W4(kg)",
                DataPropertyName = "AxleWeight4",
                FillWeight = 15
            });

            dgvRecords.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "W5",
                HeaderText = "W5(kg)",
                DataPropertyName = "AxleWeight5",
                FillWeight = 15
            });

            dgvRecords.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "W6",
                HeaderText = "W6(kg)",
                DataPropertyName = "AxleWeight6",
                FillWeight = 15
            });

            // TotalWeight
            dgvRecords.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "TotalWeight",
                HeaderText = "TotalWeight",
                DataPropertyName = "TotalWeight",
                FillWeight = 15
            });

            dgvRecords.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Axle12",
                HeaderText = "Axle12(m)",
                DataPropertyName = "Axle12",
                DefaultCellStyle = new DataGridViewCellStyle { Format = "F2" },
                FillWeight = 15
            });

            dgvRecords.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Axle23",
                HeaderText = "Axle23(m)",
                DataPropertyName = "Axle23",
                DefaultCellStyle = new DataGridViewCellStyle { Format = "F2" },
                FillWeight = 15
            });

            dgvRecords.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Axle34",
                HeaderText = "Axle34(m)",
                DataPropertyName = "Axle34",
                DefaultCellStyle = new DataGridViewCellStyle { Format = "F2" },
                FillWeight = 15
            });

            dgvRecords.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Axle45",
                HeaderText = "Axle45(m)",
                DataPropertyName = "Axle45",
                DefaultCellStyle = new DataGridViewCellStyle { Format = "F2" },
                FillWeight = 15
            });

            dgvRecords.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Axle56",
                HeaderText = "Axle56(m)",
                DataPropertyName = "Axle56",
                DefaultCellStyle = new DataGridViewCellStyle { Format = "F2" },
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
                    Format = "HH:mm:ss",
                    FormatProvider = PersianCulture
                }
            });
        }

        // فرهنگ فارسی با تقویم جلالی برای نمایش تاریخ شمسی
        private static System.Globalization.CultureInfo PersianCulture
        {
            get
            {
                var culture = new System.Globalization.CultureInfo("fa-IR");
                culture.DateTimeFormat.Calendar = new System.Globalization.PersianCalendar();
                culture.DateTimeFormat.DateSeparator = "/";
                return culture;
            }
        }

        public MainForm()
        {
            InitializeComponent();
        }

        public MainForm(IConfiguration config,
            SqlRepository repo,
            IPlateRecognitionService plateService,
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
            _cameraWatcher.OnImageCaptured += HandleImage;
            _cameraWatcher.OnStatusChanged += HandleCameraStatus;
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            if (IsDesignMode)
                return;

            try
            {
                ConfigureGrid();
                Style();

                // 🔹 1. Load configurations
                var serialSettings = _config.GetSection("SerialPort").Get<SerialPortSettings>();
                var snapshotCameraSettings = _config.GetSection("SnapshotCamera").Get<SnapshotCameraSettings>();
                var ovarviewCamera = _config.GetSection("OverviewCamera").Get<OverviewCameraSettings>();
                _alpha = _config.GetSection("WeightSettings").Get<WeightSettings>()?.Alpha ?? 1.5m;

                _serialEnabled = serialSettings?.Enabled == true;
                _snapshotEnabled = snapshotCameraSettings?.Enabled == true;
                _overviewSettings = ovarviewCamera;

                // 🔹 3. Wire events
                if (_serialEnabled)
                {
                    // Start services
                    _serialService.Start();
                }

                if (_snapshotEnabled)
                {
                    lineIds = await _repo.GetActiveLineIdsAsync() ?? new List<int>();
                    _cameraWatcher.Start(lineIds); // no need to pass path anymore

                    _plateService.AddCamera(pictureBoxVehicle);
                }

                StartOverviewCamera(ovarviewCamera);

                _systemRunning = _serialEnabled || _snapshotEnabled;
                UpdateSystemMenuState();

                await LoadGrid(); // 🔴 load existing data
            }
            catch (Exception ex)
            {
                Log.Error(ex, "MainForm_Load failed");
                MessageBox.Show($"خطا در راه‌اندازی اولیه: {ex.Message}",
                    "Startup", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

        private void StartOverviewCamera(OverviewCameraSettings? ovarviewCamera)
        {
            if (ovarviewCamera == null || !ovarviewCamera.Enabled)
                return;

            // جلوگیری از راه‌اندازی چندبارهٔ حلقه
            if (_cameraCts != null)
                return;

            var cts = new CancellationTokenSource();
            _cameraCts = cts;
            var token = cts.Token;

            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        await LoadImage(
                            picCam1,
                            $"http://{ovarviewCamera.Host}{ovarviewCamera.PictureUrl}",
                            ovarviewCamera.Username,
                            ovarviewCamera.Password
                        );

                        await Task.Delay(
                            ovarviewCamera.RefreshIntervalMs,
                            token
                        );
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        Log.Warning("Camera stream error: {Message}", ex.Message);
                    }
                }
            }, token);
        }

        private void StopOverviewCamera()
        {
            var cts = _cameraCts;
            _cameraCts = null;

            if (cts == null)
                return;

            try
            {
                if (!cts.IsCancellationRequested)
                    cts.Cancel();
            }
            catch (ObjectDisposedException) { }

            cts.Dispose();
        }

        private async Task LoadImage(
             PictureBox pic,
             string url,
             string user,
             string pass)
        {
            try
            {
                using var handler = new HttpClientHandler
                {
                    Credentials = new NetworkCredential(user, pass)
                };

                using var client = new HttpClient(handler)
                {
                    Timeout = TimeSpan.FromSeconds(5)
                };

                var bytes = await client.GetByteArrayAsync(url);

                using var ms = new MemoryStream(bytes);
                using var tempImage = Image.FromStream(ms);

                var newImage = new Bitmap(tempImage);

                if (_shuttingDown || pic.IsDisposed || !pic.IsHandleCreated)
                {
                    newImage.Dispose();
                    return;
                }

                try
                {
                    pic.Invoke(() =>
                    {
                        var old = pic.Image;
                        pic.Image = newImage;
                        old?.Dispose();
                    });
                }
                catch (ObjectDisposedException) { newImage.Dispose(); }
                catch (InvalidOperationException) { newImage.Dispose(); }
            }
            catch (Exception ex)
            {
                Log.Warning("Camera error: {Message}", ex.Message);
            }
        }

        private void UpdateVehicleUI(VehicleDto vehicle)
        {
            if (vehicle == null)
                return;

            lblSpeed.Text = $"Speed: {vehicle.Speed} km/h";
            lblLine.Text = $"Line: {vehicle.LineId}";
            lblAxles.Text = $"Axles No: {vehicle.AxleCount}";
            lblTotalWeight.Text = $"TotalWeight: {ScaleWeight(vehicle.TotalWeight)}";
            lblADC1.Text = $"ADC1:\n\n {vehicle.ADC1}";
            lblADC2.Text = $"ADC2:\n\n {vehicle.ADC2}";
            lblADC3.Text = $"ADC3:\n\n {vehicle.ADC3}";
            lblADC4.Text = $"ADC4:\n\n {vehicle.ADC4}";

            // دسترسی ایمن به محورها (قبلاً برای خودروی با کمتر از ۶ محور exception می‌داد)
            lblAxle12.Text = $"Axle12:\n\n {AxleDistance(vehicle, 0)}";
            lblAxle23.Text = $"Axle23:\n\n {AxleDistance(vehicle, 1)}";
            lblAxle34.Text = $"Axle34:\n\n {AxleDistance(vehicle, 3)}";
            lblAxle45.Text = $"Axle45:\n\n {AxleDistance(vehicle, 4)}";
            lblAxle56.Text = $"Axle56:\n\n {AxleDistance(vehicle, 5)}";
        }

        private static string AxleDistance(VehicleDto vehicle, int index)
        {
            var axles = vehicle.Axles;
            if (axles == null || index < 0 || index >= axles.Count)
                return "-";

            return axles[index]?.DistanceDisplay ?? "-";
        }

        private decimal ScaleWeight(decimal? weight)
        {
            return (weight ?? 0) * _alpha;
        }

        private decimal ScaleWeight(double? weight)
        {
            return (decimal)(weight ?? 0) * _alpha;
        }

        private void UpdateImageUI(string? imagePath, PlateResultDto? plate)
        {
            lblDetectedPlate.Text = $"Detected Plate No: {plate?.PlateNumber}";

            if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
            {
                Log.Warning("UpdateImageUI skipped, image unavailable: {Path}", imagePath);
                return;
            }

            try
            {
                using var fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var ms = new MemoryStream();

                fs.CopyTo(ms);
                ms.Position = 0;

                using var img = Image.FromStream(ms);
                var bitmap = new Bitmap(img);

                var old = pictureBoxVehicle.Image;
                pictureBoxVehicle.Image = bitmap;
                old?.Dispose();
            }
            catch (Exception ex)
            {
                Log.Warning("Failed to display image {Path}: {Message}", imagePath, ex.Message);
            }

            UpdatePlateImageUI(plate?.PlateImage);
        }

        private void UpdatePlateImageUI(Bitmap? plateImage)
        {
            try
            {
                if (InvokeRequired)
                {
                    BeginInvoke(new Action(() => UpdatePlateImageUI(plateImage)));
                    return;
                }

                var old = pictureBoxPlate.Image;
                pictureBoxPlate.Image = plateImage;
                old?.Dispose();

                var oldCam = pictureBoxPlateCam.Image;
                pictureBoxPlateCam.Image = plateImage;
                oldCam?.Dispose();
            }
            catch (Exception ex)
            {
                Log.Warning("Failed to display plate image: {Message}", ex.Message);
            }
        }

        private async Task HandleSerialData(string raw)
        {
            if (_shuttingDown)
                return;

            try
            {
                var vehicle = VehicleParser.Parse(raw);
                if (vehicle == null)
                {
                    Log.Warning("Unparsable serial frame: {Raw}", raw);
                    return;
                }

                DistanceCalculator.CalculateDistances(vehicle);

                // 🔹 UI
                await RunOnUiAsync(() => UpdateVehicleUI(vehicle));

                // 🔥 1. صبر برای دریافت مسیر عکس
                var imagePath = await WaitForImageReadyAsync(vehicle.LineId);

                PlateResultDto plate = new PlateResultDto();

                if (string.IsNullOrEmpty(imagePath))
                {
                    Log.Warning("❌ No image found for Line {LineId}", vehicle.LineId);
                }
                else if (await WaitForFileReadySafe(imagePath))
                {
                    // 🔥 2. فایل آماده است → استخراج پلاک
                    plate = _plateService.Extract(imagePath);
                }
                else
                {
                    Log.Warning("⚠️ Image not ready: {Path}", imagePath);
                }

                // رکورد حتی بدون عکس هم ذخیره می‌شود تا داده وزن گم نشود
                await _repo.SaveAsync(vehicle, imagePath, plate);

                await LoadGrid();

                // 🔥 5. UI Image
                await RunOnUiAsync(() => UpdateImageUI(imagePath, plate));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error processing serial data: {Raw}", raw);
            }
        }

        /// <summary>
        /// اجرای ایمن یک اکشن روی ترد UI، با محافظت در زمان بسته‌شدن فرم.
        /// </summary>
        private async Task RunOnUiAsync(Action action)
        {
            if (_shuttingDown || IsDisposed || Disposing)
                return;

            try
            {
                if (InvokeRequired)
                {
                    if (!IsHandleCreated)
                        return;

                    await InvokeAsync(action);
                }
                else
                {
                    action();
                }
            }
            catch (ObjectDisposedException) { }
            catch (InvalidOperationException ex)
            {
                Log.Warning("UI marshalling skipped: {Message}", ex.Message);
            }
        }

        private void HandleImage(int lineId, string path, long capturedAtTicks)
        {
            if (_shuttingDown)
                return;

            try
            {
                var entry = new PendingImage
                {
                    Path = path,
                    CapturedAtUtc = TicksToUtc(capturedAtTicks)
                };

                lock (_lock)
                {
                    if (!_imagesByLine.TryGetValue(lineId, out var queue))
                    {
                        queue = new Queue<PendingImage>();
                        _imagesByLine[lineId] = queue;
                    }

                    queue.Enqueue(entry);

                    while (queue.Count > MaxQueuePerLine)
                    {
                        var dropped = queue.Dequeue();
                        Log.Warning("Dropped overflow image for Line {LineId}: {Path}", lineId, dropped.Path);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error queueing image: {Path}", path);
            }
        }

        private static DateTime TicksToUtc(long ticks)
        {
            var now = DateTime.UtcNow;

            if (ticks <= 0 || ticks > DateTime.MaxValue.Ticks)
                return now;

            var candidate = new DateTime(ticks, DateTimeKind.Utc);

            // اگر مبنای ticks زمان تقویمی نباشد (مثل Stopwatch)، مقدار بی‌معنا می‌شود
            if (Math.Abs((now - candidate).TotalDays) > 1)
                return now;

            return candidate;
        }


        private async Task<bool> WaitForFileReadySafe(string path, int timeoutMs = 150)
        {
            for (int i = 0; i < 15; i++)
            {
                if (_shuttingDown)
                    return false;

                try
                {
                    if (!File.Exists(path))
                    {
                        await Task.Delay(timeoutMs);
                        continue;
                    }

                    using var stream = new FileStream(
                        path,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.ReadWrite);

                    if (stream.Length > 0)
                        return true;
                }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }

                await Task.Delay(timeoutMs);
            }

            return false;
        }

        private async Task<string?> WaitForImageReadyAsync(int lineId, int timeoutMs = 1000)
        {
            var start = DateTime.UtcNow;

            while ((DateTime.UtcNow - start).TotalMilliseconds < timeoutMs)
            {
                lock (_lock)
                {
                    if (_imagesByLine.TryGetValue(lineId, out var queue))
                    {
                        // عکس‌های کهنه را دور بریز تا به خودروی بعدی نچسبند
                        while (queue.Count > 0 &&
                               (DateTime.UtcNow - queue.Peek().CapturedAtUtc).TotalMilliseconds > ImageStaleMs)
                        {
                            var stale = queue.Dequeue();
                            Log.Warning("Discarded stale image for Line {LineId}: {Path}", lineId, stale.Path);
                        }

                        if (queue.Count > 0)
                            return queue.Dequeue().Path;
                    }
                }

                await Task.Delay(50);
            }

            return null;
        }

        private void ScaleWeightList(List<VehicleDto> data)
        {
            if (data == null) return;

            foreach (var v in data)
            {
                v.AxleWeight1 = ScaleWeightNullable(v.AxleWeight1);
                v.AxleWeight2 = ScaleWeightNullable(v.AxleWeight2);
                v.AxleWeight3 = ScaleWeightNullable(v.AxleWeight3);
                v.AxleWeight4 = ScaleWeightNullable(v.AxleWeight4);
                v.AxleWeight5 = ScaleWeightNullable(v.AxleWeight5);
                v.AxleWeight6 = ScaleWeightNullable(v.AxleWeight6);
                v.TotalWeight = ScaleWeightNullable(v.TotalWeight);
            }
        }

        private double? ScaleWeightNullable(double? weight)
        {
            return weight.HasValue ? (double?)((decimal)weight.Value * _alpha) : null;
        }

        private async Task LoadGrid()
        {
            if (_shuttingDown)
                return;

            try
            {
                var (hasPlate, total) = await _repo.GetRecordCountsAsync();

                if (_shuttingDown || lblRecordCounts.IsDisposed || !lblRecordCounts.IsHandleCreated)
                    return;

                if (lblRecordCounts.InvokeRequired)
                {
                    lblRecordCounts.Invoke(() =>
                    {
                        lblRecordCounts.Text = $"(HasPlate {hasPlate} / Total {total})";
                    });
                }
                else
                {
                    lblRecordCounts.Text = $"(HasPlate {hasPlate} / Total {total})";
                }

                var data = await _repo.GetAllAsync();
                ScaleWeightList(data);

                if (dgvRecords.IsDisposed || !dgvRecords.IsHandleCreated)
                    return;

                if (dgvRecords.InvokeRequired)
                {
                    dgvRecords.Invoke(() =>
                    {
                        dgvRecords.DataSource = data;
                    });
                }
                else
                {
                    dgvRecords.DataSource = data;
                }
            }
            catch (ObjectDisposedException) { }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading grid");
            }
        }

        private void dgvRecords_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            foreach (DataGridViewRow row in dgvRecords.Rows)
            {
                if (row.IsNewRow) continue;

                var plateValue = row.Cells["PlateNumber"].Value?.ToString();

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

        private void dgvRecords_SelectionChanged(object sender, EventArgs e)
        {
            flpSelectedRecord.SuspendLayout();
            flpSelectedRecord.Controls.Clear();

            if (dgvRecords.CurrentRow == null || dgvRecords.CurrentRow.IsNewRow)
            {
                flpSelectedRecord.Controls.Add(BuildCell("No record selected", Color.FromArgb(108, 117, 125)));
                flpSelectedRecord.ResumeLayout();
                return;
            }

            var row = dgvRecords.CurrentRow;

            var plateValue = row.Cells["PlateNumber"].Value?.ToString();
            var backColor =
                string.IsNullOrWhiteSpace(plateValue) || plateValue == "---"
                    ? UITheme.Danger
                    : UITheme.Success;

            foreach (DataGridViewColumn col in dgvRecords.Columns)
            {
                if (!col.Visible)
                    continue;

                var value = row.Cells[col.Index].Value;
                flpSelectedRecord.Controls.Add(BuildCell($"{col.HeaderText}: {value}", backColor));
            }

            flpSelectedRecord.ResumeLayout();
        }

        private static Label BuildCell(string text, Color backColor)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = backColor,
                Margin = new Padding(2),
                Padding = new Padding(6, 2, 6, 2),
                TextAlign = ContentAlignment.MiddleLeft
            };
        }

        private void HandleSerialStatus(bool connected)
        {
            if (_shuttingDown || IsDisposed || Disposing || !IsHandleCreated)
                return;

            try
            {
                if (InvokeRequired)
                    Invoke(() => UpdateSerialStatus(connected));
                else
                    UpdateSerialStatus(connected);
            }
            catch (ObjectDisposedException) { }
            catch (InvalidOperationException) { }
        }

        private void UpdateSerialStatus(bool connected)
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
        }

        private void HandleCameraStatus(bool ok)
        {
            if (_shuttingDown || IsDisposed || Disposing || !IsHandleCreated)
                return;

            try
            {
                if (InvokeRequired)
                    Invoke(() => UpdateCameraStatus(ok));
                else
                    UpdateCameraStatus(ok);
            }
            catch (ObjectDisposedException) { }
            catch (InvalidOperationException) { }
        }

        private void UpdateCameraStatus(bool ok)
        {
            if (ok)
            {
                lblCameraStatus.Text = "Camera Folder: ● Monitoring";
                lblCameraStatus.ForeColor = Color.Green;
            }
            else
            {
                lblCameraStatus.Text = "Camera Folder: ● Not Available";
                lblCameraStatus.ForeColor = Color.Red;
            }
        }

        private void UpdateSystemMenuState()
        {
            if (IsDisposed || Disposing)
                return;

            try
            {
                startSystemToolStripMenuItem.Enabled = !_systemRunning;
                stopSystemToolStripMenuItem.Enabled = _systemRunning;
            }
            catch (Exception ex)
            {
                Log.Warning("Menu state update failed: {Message}", ex.Message);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close(); // به‌جای Application.Exit تا FormClosing اجرا شود
        }

        private async void startSystemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_systemRunning)
            {
                Log.Information("Start ignored, system already running");
                return;
            }

            try
            {
                if (_serialService != null)
                    _serialService.Start();

                if (_cameraWatcher != null)
                {
                    if (lineIds == null || lineIds.Count == 0)
                        lineIds = await _repo.GetActiveLineIdsAsync() ?? new List<int>();

                    _cameraWatcher.Start(lineIds);
                }

                StartOverviewCamera(_overviewSettings);

                _systemRunning = true;
            }
            catch (Exception ex)
            {
                _systemRunning = false;
                Log.Error(ex, "Failed to start system");
                MessageBox.Show($"خطا در راه‌اندازی سیستم: {ex.Message}",
                    "Start", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                UpdateSystemMenuState();
            }
        }

        private void stopSystemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!_systemRunning)
            {
                Log.Information("Stop ignored, system already stopped");
                return;
            }

            StopAllServices();
            _systemRunning = false;
            UpdateSystemMenuState();
        }

        private void StopAllServices()
        {
            try { StopOverviewCamera(); }
            catch (Exception ex) { Log.Warning("Overview stop failed: {Message}", ex.Message); }

            try { _cameraWatcher?.Stop(); }
            catch (Exception ex) { Log.Warning("Camera watcher stop failed: {Message}", ex.Message); }

            try { _serialService?.Stop(); }
            catch (Exception ex) { Log.Warning("Serial stop failed: {Message}", ex.Message); }

            lock (_lock)
            {
                _imagesByLine.Clear();
            }
        }

        private void serialPortSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var current = _config.GetSection("SerialPort").Get<SerialPortSettings>();
            if (current == null)
            {
                MessageBox.Show("SerialPort settings not found in configuration.",
                    "Settings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var form = new SerialPortSettingsForm(current);
            if (form.ShowDialog(this) != DialogResult.OK)
                return;

            var updated = form.Settings;
            if (!SaveSerialSettings(updated))
            {
                MessageBox.Show("Failed to save serial port settings.",
                    "Settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ApplySerialSettings(updated);
        }

        private bool SaveSerialSettings(SerialPortSettings settings)
        {
            var section = new System.Text.Json.Nodes.JsonObject
            {
                ["PortName"] = settings.PortName,
                ["BaudRate"] = settings.BaudRate,
                ["Parity"] = settings.Parity,
                ["DataBits"] = settings.DataBits,
                ["StopBits"] = settings.StopBits,
                ["Enabled"] = settings.Enabled
            };

            return SaveConfigSection("SerialPort", section);
        }

        private bool SaveConfigSection(string sectionName, System.Text.Json.Nodes.JsonObject section)
        {
            try
            {
                var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
                var json = File.ReadAllText(path);

                using var doc = System.Text.Json.JsonDocument.Parse(json);
                var root = new System.Text.Json.Nodes.JsonObject();

                foreach (var prop in doc.RootElement.EnumerateObject())
                {
                    if (prop.Name.Equals(sectionName, StringComparison.OrdinalIgnoreCase))
                        continue;

                    root[prop.Name] = System.Text.Json.Nodes.JsonNode.Parse(prop.Value.GetRawText());
                }

                root[sectionName] = section;

                File.WriteAllText(path, root.ToJsonString(
                    new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to save config section {Section}", sectionName);
                return false;
            }
        }

        private async void cameraSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var current = _config.GetSection("SnapshotCamera").Get<SnapshotCameraSettings>();
            if (current == null)
            {
                MessageBox.Show("SnapshotCamera settings not found in configuration.",
                    "Settings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var form = new SnapshotCameraSettingsForm(current);
            if (form.ShowDialog(this) != DialogResult.OK)
                return;

            var updated = form.Settings;

            var section = new System.Text.Json.Nodes.JsonObject
            {
                ["WatchRootPath"] = updated.WatchRootPath,
                ["Filter"] = updated.Filter,
                ["IncludeSubfolders"] = updated.IncludeSubfolders,
                ["Enabled"] = updated.Enabled,
                ["ImageLookbackMs"] = updated.ImageLookbackMs,
                ["ImageWaitTimeoutMs"] = updated.ImageWaitTimeoutMs
            };

            if (!SaveConfigSection("SnapshotCamera", section))
            {
                MessageBox.Show("Failed to save snapshot camera settings.",
                    "Settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _snapshotEnabled = updated.Enabled;

            if (_systemRunning)
            {
                _cameraWatcher?.Stop();

                if (updated.Enabled)
                {
                    lineIds = await _repo.GetActiveLineIdsAsync() ?? new List<int>();
                    _cameraWatcher.Start(lineIds);
                }

                UpdateCameraStatus(_cameraWatcher?.IsRunning == true);
            }
        }

        private void overviewCameraSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var current = _config.GetSection("OverviewCamera").Get<OverviewCameraSettings>();
            if (current == null)
            {
                MessageBox.Show("OverviewCamera settings not found in configuration.",
                    "Settings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var form = new OverviewCameraSettingsForm(current);
            if (form.ShowDialog(this) != DialogResult.OK)
                return;

            var updated = form.Settings;

            var section = new System.Text.Json.Nodes.JsonObject
            {
                ["Host"] = updated.Host,
                ["PictureUrl"] = updated.PictureUrl,
                ["Username"] = updated.Username,
                ["Password"] = updated.Password,
                ["Enabled"] = updated.Enabled,
                ["RefreshIntervalMs"] = updated.RefreshIntervalMs
            };

            if (!SaveConfigSection("OverviewCamera", section))
            {
                MessageBox.Show("Failed to save overview camera settings.",
                    "Settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _overviewSettings = updated;

            if (_systemRunning)
            {
                StopOverviewCamera();
                StartOverviewCamera(_overviewSettings);
            }
        }

        private void databaseSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var current = _config.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(current))
            {
                MessageBox.Show("DefaultConnection not found in configuration.",
                    "Settings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var form = new DatabaseSettingsForm(current);
            if (form.ShowDialog(this) != DialogResult.OK)
                return;

            if (!SaveConfigSection("ConnectionStrings",
                new System.Text.Json.Nodes.JsonObject { ["DefaultConnection"] = form.ConnectionString }))
            {
                MessageBox.Show("Failed to save database settings.",
                    "Settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MessageBox.Show(this,
                "Database settings saved. Restart the application for changes to take effect.",
                "Database", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void weightSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var current = _config.GetSection("WeightSettings").Get<WeightSettings>();
            if (current == null)
            {
                MessageBox.Show("WeightSettings not found in configuration.",
                    "Settings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var form = new WeightSettingsForm(current);
            if (form.ShowDialog(this) != DialogResult.OK)
                return;

            if (!SaveConfigSection("WeightSettings",
                new System.Text.Json.Nodes.JsonObject { ["Alpha"] = form.Alpha }))
            {
                MessageBox.Show("Failed to save weight settings.",
                    "Settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _alpha = form.Alpha;

            MessageBox.Show(this,
                "Weight settings saved. Values are now multiplied by Alpha.",
                "Weight Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ApplySerialSettings(SerialPortSettings settings)
        {
            try
            {
                _serialEnabled = settings.Enabled;

                if (_systemRunning)
                {
                    _serialService?.Stop();

                    if (settings.Enabled)
                        _serialService?.Start();
                }

                UpdateSerialStatus(_serialService?.IsRunning == true);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to apply serial port settings");
            }
        }

        private void VehicleReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var form = new VehicleReportForm(_repo, _alpha))
            {
                form.ShowDialog(this);
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Vehicle Weight System v1.0\n\nCopyright © Farasoo Towzin Co.\nTehran, Iran.");
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_shuttingDown)
                return;

            _shuttingDown = true;

            // قطع رویدادها تا callback روی فرم در حال بسته‌شدن اجرا نشود
            try
            {
                if (_serialService != null)
                    _serialService.OnConnectionChanged -= HandleSerialStatus;

                if (_cameraWatcher != null)
                {
                    _cameraWatcher.OnImageCaptured -= HandleImage;
                    _cameraWatcher.OnStatusChanged -= HandleCameraStatus;
                }
            }
            catch (Exception ex)
            {
                Log.Warning("Event unsubscribe failed: {Message}", ex.Message);
            }

            StopAllServices();
            _systemRunning = false;

            try
            {
                pictureBoxVehicle.Image?.Dispose();
                pictureBoxVehicle.Image = null;
                picCam1.Image?.Dispose();
                picCam1.Image = null;
            }
            catch { }
        }
    }
}
