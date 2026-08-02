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

        private int TotalPages => Math.Max(1, (int)Math.Ceiling(_totalCount / (double)_pageSize));


        public SqlRepository _repo { get; }

        public VehicleReportForm(SqlRepository repo)
        {
            InitializeComponent();
            grpFilters.BringToFront();
            _pager.BringToFront();
            dgvVehicles.SendToBack();
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }


        private async void btnSearch_Click(object sender, EventArgs e)
        {

            _currentPage = 1;
            await LoadPageAsync();

        }

        private void dgvVehicles_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            dgvVehicles.Sort(
                   dgvVehicles.Columns[e.ColumnIndex],
                   ListSortDirection.Descending);
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
                dtFrom.Value, dtTo.Value, cmbLine.SelectedValue as int?,
                txtPlate.Text, (double)nudMinWeight.Value,
                (double)nudMaxWeight.Value, chkOverweight.Checked);

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

            dt.Columns.Add("Plate");
            dt.Columns.Add("Line");
            dt.Columns.Add("Speed");
            dt.Columns.Add("Axles");
            dt.Columns.Add("TotalWeight");
            dt.Columns.Add("Date");

            foreach (var v in list)
            {
                dt.Rows.Add(
                    v.PlateNumber,
                    v.LineName,
                    v.Speed,
                    v.AxleCount,
                    v.TotalWeight,
                    v.Timestamp
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
                    cmbLine.SelectedValue as int?,
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

                dgvVehicles.DataSource = items;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"خطا در دریافت گزارش: {ex.Message}",
                    "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                : $"Page {_currentPage} از {TotalPages}  |  Total Count: {_totalCount:N0}";

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
