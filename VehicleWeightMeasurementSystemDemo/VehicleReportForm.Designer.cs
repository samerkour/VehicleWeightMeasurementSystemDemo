namespace VehicleWeightMeasurementSystemDemo
{
    partial class VehicleReportForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            grpFilters = new GroupBox();
            maxWeightLabel = new Label();
            minWeightLabel = new Label();
            plateLabel = new Label();
            lineLabel = new Label();
            toLabel = new Label();
            fromLabel = new Label();
            btnExcel = new Button();
            btnSearch = new Button();
            chkOverweight = new CheckBox();
            nudMaxWeight = new NumericUpDown();
            nudMinWeight = new NumericUpDown();
            txtPlate = new TextBox();
            cmbLine = new ComboBox();
            dtTo = new DateTimePicker();
            dtFrom = new DateTimePicker();
            dgvVehicles = new DataGridView();
            grpFilters.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudMaxWeight).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudMinWeight).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvVehicles).BeginInit();
            SuspendLayout();
            // 
            // grpFilters
            // 
            grpFilters.Controls.Add(maxWeightLabel);
            grpFilters.Controls.Add(minWeightLabel);
            grpFilters.Controls.Add(plateLabel);
            grpFilters.Controls.Add(lineLabel);
            grpFilters.Controls.Add(toLabel);
            grpFilters.Controls.Add(fromLabel);
            grpFilters.Controls.Add(btnExcel);
            grpFilters.Controls.Add(btnSearch);
            grpFilters.Controls.Add(chkOverweight);
            grpFilters.Controls.Add(nudMaxWeight);
            grpFilters.Controls.Add(nudMinWeight);
            grpFilters.Controls.Add(txtPlate);
            grpFilters.Controls.Add(cmbLine);
            grpFilters.Controls.Add(dtTo);
            grpFilters.Controls.Add(dtFrom);
            grpFilters.Location = new Point(12, 22);
            grpFilters.Name = "grpFilters";
            grpFilters.Size = new Size(987, 295);
            grpFilters.TabIndex = 0;
            grpFilters.TabStop = false;
            grpFilters.Text = "Filters";
            // 
            // maxWeightLabel
            // 
            maxWeightLabel.AutoSize = true;
            maxWeightLabel.Location = new Point(501, 129);
            maxWeightLabel.Name = "maxWeightLabel";
            maxWeightLabel.Size = new Size(88, 20);
            maxWeightLabel.TabIndex = 14;
            maxWeightLabel.Text = "Max Weight";
            // 
            // minWeightLabel
            // 
            minWeightLabel.AutoSize = true;
            minWeightLabel.Location = new Point(76, 129);
            minWeightLabel.Name = "minWeightLabel";
            minWeightLabel.Size = new Size(85, 20);
            minWeightLabel.TabIndex = 13;
            minWeightLabel.Text = "Min Weight";
            // 
            // plateLabel
            // 
            plateLabel.AutoSize = true;
            plateLabel.Location = new Point(501, 85);
            plateLabel.Name = "plateLabel";
            plateLabel.Size = new Size(42, 20);
            plateLabel.TabIndex = 12;
            plateLabel.Text = "Plate";
            // 
            // lineLabel
            // 
            lineLabel.AutoSize = true;
            lineLabel.Location = new Point(76, 82);
            lineLabel.Name = "lineLabel";
            lineLabel.Size = new Size(36, 20);
            lineLabel.TabIndex = 11;
            lineLabel.Text = "Line";
            // 
            // toLabel
            // 
            toLabel.AutoSize = true;
            toLabel.Location = new Point(501, 42);
            toLabel.Name = "toLabel";
            toLabel.Size = new Size(25, 20);
            toLabel.TabIndex = 10;
            toLabel.Text = "To";
            // 
            // fromLabel
            // 
            fromLabel.AutoSize = true;
            fromLabel.Location = new Point(76, 37);
            fromLabel.Name = "fromLabel";
            fromLabel.Size = new Size(43, 20);
            fromLabel.TabIndex = 9;
            fromLabel.Text = "From";
            // 
            // btnExcel
            // 
            btnExcel.Location = new Point(602, 242);
            btnExcel.Name = "btnExcel";
            btnExcel.Size = new Size(94, 29);
            btnExcel.TabIndex = 8;
            btnExcel.Text = "Export Excel";
            btnExcel.UseVisualStyleBackColor = true;
            btnExcel.Click += btnExcel_Click;
            // 
            // btnSearch
            // 
            btnSearch.Location = new Point(167, 242);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(94, 29);
            btnSearch.TabIndex = 7;
            btnSearch.Text = "Search";
            btnSearch.UseVisualStyleBackColor = true;
            btnSearch.Click += btnSearch_Click;
            // 
            // chkOverweight
            // 
            chkOverweight.AutoSize = true;
            chkOverweight.Location = new Point(148, 193);
            chkOverweight.Name = "chkOverweight";
            chkOverweight.Size = new Size(107, 24);
            chkOverweight.TabIndex = 6;
            chkOverweight.Text = "Overweight";
            chkOverweight.UseVisualStyleBackColor = true;
            // 
            // nudMaxWeight
            // 
            nudMaxWeight.Location = new Point(602, 127);
            nudMaxWeight.Name = "nudMaxWeight";
            nudMaxWeight.Size = new Size(150, 27);
            nudMaxWeight.TabIndex = 5;
            // 
            // nudMinWeight
            // 
            nudMinWeight.Location = new Point(167, 127);
            nudMinWeight.Name = "nudMinWeight";
            nudMinWeight.Size = new Size(150, 27);
            nudMinWeight.TabIndex = 4;
            // 
            // txtPlate
            // 
            txtPlate.Location = new Point(602, 82);
            txtPlate.Name = "txtPlate";
            txtPlate.Size = new Size(125, 27);
            txtPlate.TabIndex = 3;
            // 
            // cmbLine
            // 
            cmbLine.FormattingEnabled = true;
            cmbLine.Location = new Point(167, 82);
            cmbLine.Name = "cmbLine";
            cmbLine.Size = new Size(151, 28);
            cmbLine.TabIndex = 2;
            // 
            // dtTo
            // 
            dtTo.Location = new Point(602, 37);
            dtTo.Name = "dtTo";
            dtTo.Size = new Size(250, 27);
            dtTo.TabIndex = 1;
            // 
            // dtFrom
            // 
            dtFrom.Location = new Point(167, 38);
            dtFrom.Name = "dtFrom";
            dtFrom.Size = new Size(250, 27);
            dtFrom.TabIndex = 0;
            // 
            // dgvVehicles
            // 
            dgvVehicles.AllowUserToOrderColumns = true;
            dgvVehicles.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvVehicles.Location = new Point(12, 334);
            dgvVehicles.Name = "dgvVehicles";
            dgvVehicles.RowHeadersWidth = 51;
            dgvVehicles.Size = new Size(987, 296);
            dgvVehicles.TabIndex = 1;
            dgvVehicles.ColumnHeaderMouseClick += dgvVehicles_ColumnHeaderMouseClick;
            // 
            // VehicleReportForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1011, 706);
            Controls.Add(dgvVehicles);
            Controls.Add(grpFilters);
            Name = "VehicleReportForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "VehicleReportForm";
            grpFilters.ResumeLayout(false);
            grpFilters.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudMaxWeight).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudMinWeight).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvVehicles).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox grpFilters;
        private TextBox txtPlate;
        private ComboBox cmbLine;
        private DateTimePicker dtTo;
        private DateTimePicker dtFrom;
        private CheckBox chkOverweight;
        private NumericUpDown nudMaxWeight;
        private NumericUpDown nudMinWeight;
        private Button btnExcel;
        private Button btnSearch;
        private DataGridView dgvVehicles;
        private Label maxWeightLabel;
        private Label minWeightLabel;
        private Label plateLabel;
        private Label lineLabel;
        private Label toLabel;
        private Label fromLabel;
    }
}