using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using VehicleWeightMeasurementSystemDemo.Domain.Weighing;
using VehicleWeightMeasurementSystemDemo.Infrastructure.Persistence;
using VehicleWeightMeasurementSystemDemo.Reports;
using VehicleWeightMeasurementSystemDemo.Theming;

namespace VehicleWeightMeasurementSystemDemo
{
    public partial class VehicleReportForm : Form
    {

        int _pageSize = 50;
        int _currentPage = 1;
        private int _totalCount;
        private bool _loading;
        private readonly BindingSource _gridSource = new();
        private string _sortProperty = "Timestamp";
        private ListSortDirection _sortDirection = ListSortDirection.Descending;

        private int TotalPages => Math.Max(1, (int)Math.Ceiling(_totalCount / (double)_pageSize));


        public SqlRepository _repo { get; }
        private readonly decimal _alpha;

        public VehicleReportForm(SqlRepository repo, decimal alpha = 1.5m)
        {
            InitializeComponent();
            grpFilters.BringToFront();
            _pager.BringToFront();
            dgvVehicles.SendToBack();
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _alpha = alpha;

            _cmbPageSize.SelectedIndex = 0;

            dtFrom.Value = DateTime.Today.AddMonths(-1);
            dtTo.Value = DateTime.Now;

            ConfigureGrid();
            Style();

            _ = LoadLineAsync();
        }

        private async Task LoadLineAsync()
        {
            try
            {
                var lineIds = await _repo.GetActiveLineIdsAsync();

                if (cmbLine.IsDisposed)
                    return;

                // گزینه‌ی اول = همه خطوط (با 0 نشان داده می‌شود)
                cmbLine.Items.Add(0);
                foreach (var id in lineIds)
                    cmbLine.Items.Add(id);

                cmbLine.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Error loading lines: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigureGrid()
        {
            dgvVehicles.AutoGenerateColumns = false;
            dgvVehicles.Columns.Clear();

            dgvVehicles.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Id",
                HeaderText = "Seq",
                DataPropertyName = "Id",
                FillWeight = 12
            });

            dgvVehicles.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "PlateNumber",
                HeaderText = "Plate",
                DataPropertyName = "PlateNumber",
                FillWeight = 20
            });

            dgvVehicles.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Speed",
                HeaderText = "Speed",
                DataPropertyName = "Speed",
                FillWeight = 10
            });

            dgvVehicles.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "LineName",
                HeaderText = "Line",
                DataPropertyName = "LineName",
                FillWeight = 10
            });

            dgvVehicles.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "AxleCount",
                HeaderText = "Axle Count",
                DataPropertyName = "AxleCount",
                FillWeight = 10
            });

            dgvVehicles.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "W1",
                HeaderText = "W1(kg)",
                DataPropertyName = "AxleWeight1",
                FillWeight = 15
            });

            dgvVehicles.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "W2",
                HeaderText = "W2(kg)",
                DataPropertyName = "AxleWeight2",
                FillWeight = 15
            });

            dgvVehicles.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "W3",
                HeaderText = "W3(kg)",
                DataPropertyName = "AxleWeight3",
                FillWeight = 15
            });

            dgvVehicles.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "W4",
                HeaderText = "W4(kg)",
                DataPropertyName = "AxleWeight4",
                FillWeight = 15
            });

            dgvVehicles.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "W5",
                HeaderText = "W5(kg)",
                DataPropertyName = "AxleWeight5",
                FillWeight = 15
            });

            dgvVehicles.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "W6",
                HeaderText = "W6(kg)",
                DataPropertyName = "AxleWeight6",
                FillWeight = 15
            });

            dgvVehicles.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "TotalWeight",
                HeaderText = "TotalWeight",
                DataPropertyName = "TotalWeight",
                FillWeight = 15
            });

            dgvVehicles.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Axle12",
                HeaderText = "Axle12(m)",
                DataPropertyName = "Axle12",
                DefaultCellStyle = new DataGridViewCellStyle { Format = "F2" },
                FillWeight = 15
            });

            dgvVehicles.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Axle23",
                HeaderText = "Axle23(m)",
                DataPropertyName = "Axle23",
                DefaultCellStyle = new DataGridViewCellStyle { Format = "F2" },
                FillWeight = 15
            });

            dgvVehicles.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Axle34",
                HeaderText = "Axle34(m)",
                DataPropertyName = "Axle34",
                DefaultCellStyle = new DataGridViewCellStyle { Format = "F2" },
                FillWeight = 15
            });

            dgvVehicles.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Axle45",
                HeaderText = "Axle45(m)",
                DataPropertyName = "Axle45",
                DefaultCellStyle = new DataGridViewCellStyle { Format = "F2" },
                FillWeight = 15
            });

            dgvVehicles.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Axle56",
                HeaderText = "Axle56(m)",
                DataPropertyName = "Axle56",
                DefaultCellStyle = new DataGridViewCellStyle { Format = "F2" },
                FillWeight = 15
            });

            dgvVehicles.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Timestamp",
                HeaderText = "Time",
                DataPropertyName = "Timestamp",
                FillWeight = 15,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "yyyy/MM/dd HH:mm:ss",
                    FormatProvider = PersianCulture
                }
            });
        }

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

        private void ScaleTotalWeights(List<VehicleReportDto> items)
        {
            if (items == null) return;

            foreach (var item in items)
            {
                if (item.TotalWeight.HasValue)
                    item.TotalWeight = (double?)((decimal)item.TotalWeight.Value * _alpha);

                item.AxleWeight1 = ScaleWeightNullable(item.AxleWeight1);
                item.AxleWeight2 = ScaleWeightNullable(item.AxleWeight2);
                item.AxleWeight3 = ScaleWeightNullable(item.AxleWeight3);
                item.AxleWeight4 = ScaleWeightNullable(item.AxleWeight4);
                item.AxleWeight5 = ScaleWeightNullable(item.AxleWeight5);
                item.AxleWeight6 = ScaleWeightNullable(item.AxleWeight6);
            }
        }

        private double? ScaleWeightNullable(double? weight)
        {
            return weight.HasValue ? (double?)((decimal)weight.Value * _alpha) : null;
        }

        private void Style()
        {
            this.BackColor = UITheme.Background;
            this.Font = new Font("Segoe UI", 10);
            this.Text = "Vehicle Report";

            grpFilters.BackColor = UITheme.CardBack;
            grpFilters.ForeColor = Color.Black;
            grpFilters.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            grpFilters.Padding = new Padding(14);

            StyleLabel(fromLabel);
            StyleLabel(toLabel);
            StyleLabel(lineLabel);
            StyleLabel(plateLabel);
            StyleLabel(minWeightLabel);
            StyleLabel(maxWeightLabel);

            StyleButton(btnSearch, UITheme.Success);
            StyleButton(btnExcel, Color.FromArgb(33, 150, 243));

            var pagerText = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            _lblPageInfo.Font = pagerText;
            _lblPageInfo.ForeColor = Color.FromArgb(60, 60, 60);
            _lblPageInfo.TextAlign = ContentAlignment.MiddleCenter;

            _pager.BackColor = Color.White;

            _pager.Resize += (s, e) => CenterPagerContent();
            CenterPagerContent();

            grpFilters.Resize += (s, e) => CenterFilterControls();
            CenterFilterControls();
        }

        private void CenterFilterControls()
        {
            if (grpFilters == null || grpFilters.Width <= 0)
                return;

            Control[] items =
            {
                fromLabel, dtFrom,
                toLabel, dtTo,
                lineLabel, cmbLine,
                plateLabel, txtPlate,
                minWeightLabel, nudMinWeight,
                maxWeightLabel, nudMaxWeight,
                chkOverweight,
                btnSearch, btnExcel
            };

            int minX = int.MaxValue;
            int maxX = int.MinValue;

            foreach (var item in items)
            {
                minX = Math.Min(minX, item.Left);
                maxX = Math.Max(maxX, item.Right);
            }

            int contentWidth = maxX - minX;
            int offset = (grpFilters.ClientSize.Width - contentWidth) / 2 - minX;

            foreach (var item in items)
            {
                item.Left += offset;
            }
        }

        private void CenterPagerContent()
        {
            if (_pager == null || _pager.Width <= 0)
                return;

            Control[] items = { _btnFirst, _btnPrev, _lblPageInfo, _cmbPageSize, _btnNext, _btnLast };

            int total = 0;
            foreach (var item in items)
            {
                total += item.Width;
                if (item != items[^1])
                    total += item.Margin.Left + item.Margin.Right;
            }

            int left = Math.Max(0, (_pager.Width - total) / 2);
            _pager.Padding = new Padding(left, _pager.Padding.Top, _pager.Padding.Right, _pager.Padding.Bottom);
        }

        private void StyleLabel(Label label)
        {
            label.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            label.ForeColor = Color.FromArgb(30, 30, 30);
            label.TextAlign = ContentAlignment.MiddleLeft;
            label.AutoSize = false;
            label.Width = 100;
        }

        private void StyleButton(Button button, Color back)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.BackColor = back;
            button.ForeColor = Color.White;
            button.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            button.Cursor = Cursors.Hand;
        }


        private async void btnSearch_Click(object sender, EventArgs e)
        {

            _currentPage = 1;
            await LoadPageAsync();

        }

        private void dgvVehicles_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            var column = dgvVehicles.Columns[e.ColumnIndex];
            if (string.IsNullOrEmpty(column.DataPropertyName))
                return;

            // اگر روی همان ستون کلیک شد، جهت عوض می‌شود؛ در غیر این صورت نزولی
            if (_sortProperty == column.DataPropertyName)
                _sortDirection = _sortDirection == ListSortDirection.Ascending
                    ? ListSortDirection.Descending
                    : ListSortDirection.Ascending;
            else
            {
                _sortProperty = column.DataPropertyName;
                _sortDirection = ListSortDirection.Descending;
            }

            ApplySort();
        }

        private void ApplySort()
        {
            _gridSource.Sort = $"{_sortProperty} {(_sortDirection == ListSortDirection.Ascending ? "ASC" : "DESC")}";
        }

        private void dgvVehicles_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            var timestampColumn = dgvVehicles.Columns["Timestamp"];
            if (timestampColumn != null)
            {
                timestampColumn.HeaderText = "Time";
                timestampColumn.DefaultCellStyle.FormatProvider =
                    new System.Globalization.CultureInfo("fa-IR");
                timestampColumn.DefaultCellStyle.Format = "yyyy/MM/dd HH:mm:ss";
            }
        }

        private async void btnExcel_Click(object sender, EventArgs e)
        {

            if (_loading) return;
            _loading = true;
            btnExcel.Enabled = false;
            UseWaitCursor = true;

            try
            {
                var data = await _repo.SearchAsync(
                dtFrom.Value, dtTo.Value, cmbLine.SelectedItem as int?,
                txtPlate.Text, (double)nudMinWeight.Value,
                (double)nudMaxWeight.Value, chkOverweight.Checked);

                ScaleTotalWeights(data);

                if (data.Count == 0)
                {
                    MessageBox.Show(this, "No Record Found.", "Info",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var dt = ToDataTable(data); // your converter

                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.Filter = "Excel Files (*.xlsx)|*.xlsx";
                    sfd.FileName = $"VehicleReport_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        string filePath = sfd.FileName;

                        using (var wb = new XLWorkbook())
                        {
                            wb.Worksheets.Add(dt, "Vehicles");
                            wb.SaveAs(filePath);
                        }

                        MessageBox.Show("Export completed successfully.", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Generationg excel file error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _loading = false;
                btnExcel.Enabled = true;
                UseWaitCursor = false;
            }
        }

        public DataTable ToDataTable(List<VehicleReportDto> list)
        {
            var dt = new DataTable();

            dt.Columns.Add("Seq");
            dt.Columns.Add("Plate");
            dt.Columns.Add("Speed");
            dt.Columns.Add("Line");
            dt.Columns.Add("Axle Count");
            dt.Columns.Add("W1(kg)");
            dt.Columns.Add("W2(kg)");
            dt.Columns.Add("W3(kg)");
            dt.Columns.Add("W4(kg)");
            dt.Columns.Add("W5(kg)");
            dt.Columns.Add("W6(kg)");
            dt.Columns.Add("TotalWeight");
            dt.Columns.Add("Axle12(m)");
            dt.Columns.Add("Axle23(m)");
            dt.Columns.Add("Axle34(m)");
            dt.Columns.Add("Axle45(m)");
            dt.Columns.Add("Axle56(m)");
            dt.Columns.Add("Date");

            foreach (var v in list)
            {
                dt.Rows.Add(
                    v.Id,
                    v.PlateNumber,
                    v.Speed,
                    v.LineName,
                    v.AxleCount,
                    v.AxleWeight1,
                    v.AxleWeight2,
                    v.AxleWeight3,
                    v.AxleWeight4,
                    v.AxleWeight5,
                    v.AxleWeight6,
                    v.TotalWeight,
                    v.Axle12,
                    v.Axle23,
                    v.Axle34,
                    v.Axle45,
                    v.Axle56,
                    v.Timestamp.ToString("yyyy/MM/dd HH:mm:ss", PersianCulture)
                );
            }

            return dt;
        }

        private async void GoToPage(int page)
        {
            if (_loading) return;

            page = Math.Clamp(page, 1, TotalPages);
            if (page == _currentPage) return;

            _currentPage = page;
            await LoadPageAsync();
        }

        private async Task LoadPageAsync()
        {
            if (_loading) return;
            _loading = true;
            //SetPagerEnabled(false);
            UseWaitCursor = true;

            try
            {
                var (items, total) = await _repo.SearchPagedAsync(
                    dtFrom.Value,
                    dtTo.Value,
                    cmbLine.SelectedItem as int?,
                    txtPlate.Text,
                    (double)nudMinWeight.Value,
                    (double)nudMaxWeight.Value,
                    chkOverweight.Checked,
                    _currentPage,
                    _pageSize);

                _totalCount = total;

                // اگر صفحه فعلی بعد از تغییر فیلتر خارج از محدوده شد
                if (items.Count == 0 && _currentPage > TotalPages)
                {
                    _currentPage = TotalPages;
                    _loading = false;
                    await LoadPageAsync();
                    return;
                }

                ScaleTotalWeights(items);

                dgvVehicles.DataSource = _gridSource;
                _gridSource.DataSource = items;
                ApplySort();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Error loading report: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _loading = false;
                UseWaitCursor = false;
                UpdatePagerState();
            }
        }

        private void UpdatePagerState()
        {
            _lblPageInfo.Text = _totalCount == 0
                ? "No Record Found"
                : $"Page {_currentPage} From {TotalPages}  |  Total Count: {_totalCount:N0}";

            _btnFirst.Enabled = _btnPrev.Enabled = _currentPage > 1;
            _btnNext.Enabled = _btnLast.Enabled = _currentPage < TotalPages;
            _cmbPageSize.Enabled = true;
        }

        private void _btnFirst_Click(object sender, EventArgs e)
        {
            GoToPage(1);
        }

        private void _btnPrev_Click(object sender, EventArgs e)
        {
            GoToPage(_currentPage - 1);
        }

        private void _btnNext_Click(object sender, EventArgs e)
        {
            GoToPage(_currentPage + 1);
        }

        private void _btnLast_Click(object sender, EventArgs e)
        {
            GoToPage(TotalPages);
        }

        private  async void _cmbPageSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_cmbPageSize.SelectedItem is int size)
            {
                _pageSize = size;
                _currentPage = 1;
                await LoadPageAsync();
            }
        }



    }
}
