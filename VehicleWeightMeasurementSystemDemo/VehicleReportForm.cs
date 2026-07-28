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

namespace VehicleWeightMeasurementSystemDemo
{
    public partial class VehicleReportForm : Form
    {

        int pageSize = 50;
        int currentPage = 1;

        public SqlRepository _repo { get; }

        public VehicleReportForm(SqlRepository repo)
        {
            InitializeComponent();
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }


        private async void btnSearch_Click(object sender, EventArgs e)
        {

            var data = await _repo.SearchAsync(
                dtFrom.Value,
                dtTo.Value,
                cmbLine.SelectedValue as int?,
                txtPlate.Text,
                (double)nudMinWeight.Value,
                (double)nudMaxWeight.Value,
                chkOverweight.Checked
            );


            dgvVehicles.DataSource = data;

        }

        private void dgvVehicles_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            dgvVehicles.Sort(
                   dgvVehicles.Columns[e.ColumnIndex],
                   ListSortDirection.Descending);
        }

        private async void btnExcel_Click(object sender, EventArgs e)
        {
            var data = await _repo.SearchAsync(
                dtFrom.Value,
                dtTo.Value,
                cmbLine.SelectedValue as int?,
                txtPlate.Text,
                (double)nudMinWeight.Value,
                (double)nudMaxWeight.Value,
                chkOverweight.Checked
            );

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

    }
}
