using VehicleWeightMeasurementSystemDemo.Theming;

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
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VehicleReportForm));
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
            dtTo = new VehicleWeightMeasurementSystemDemo.Controls.JalaliDateTimePicker();
            dtFrom = new VehicleWeightMeasurementSystemDemo.Controls.JalaliDateTimePicker();
            dgvVehicles = new DataGridView();
            _btnPrev = new Button();
            _btnFirst = new Button();
            _btnNext = new Button();
            _btnLast = new Button();
            _cmbPageSize = new ComboBox();
            _pager = new FlowLayoutPanel();
            _lblPageInfo = new Label();
            grpFilters.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudMaxWeight).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudMinWeight).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvVehicles).BeginInit();
            _pager.SuspendLayout();
            SuspendLayout();
            // 
            // grpFilters
            // 
            grpFilters.BackColor = Color.White;
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
            grpFilters.Dock = DockStyle.Top;
            grpFilters.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grpFilters.ForeColor = Color.FromArgb(30, 30, 30);
            grpFilters.Location = new Point(12, 12);
            grpFilters.Name = "grpFilters";
            grpFilters.Padding = new Padding(10);
            grpFilters.Size = new Size(987, 300);
            grpFilters.TabIndex = 0;
            grpFilters.TabStop = false;
            grpFilters.Text = "Filters";
            // 
            // maxWeightLabel
            // 
            maxWeightLabel.AutoSize = true;
            maxWeightLabel.Location = new Point(501, 134);
            maxWeightLabel.Name = "maxWeightLabel";
            maxWeightLabel.Size = new Size(94, 20);
            maxWeightLabel.TabIndex = 14;
            maxWeightLabel.Text = "Max Weight";
            // 
            // minWeightLabel
            // 
            minWeightLabel.AutoSize = true;
            minWeightLabel.Location = new Point(64, 148);
            minWeightLabel.Name = "minWeightLabel";
            minWeightLabel.Size = new Size(91, 20);
            minWeightLabel.TabIndex = 13;
            minWeightLabel.Text = "Min Weight";
            // 
            // plateLabel
            // 
            plateLabel.AutoSize = true;
            plateLabel.Location = new Point(501, 85);
            plateLabel.Name = "plateLabel";
            plateLabel.Size = new Size(44, 20);
            plateLabel.TabIndex = 12;
            plateLabel.Text = "Plate";
            // 
            // lineLabel
            // 
            lineLabel.AutoSize = true;
            lineLabel.Location = new Point(64, 85);
            lineLabel.Name = "lineLabel";
            lineLabel.Size = new Size(38, 20);
            lineLabel.TabIndex = 11;
            lineLabel.Text = "Line";
            // 
            // toLabel
            // 
            toLabel.AutoSize = true;
            toLabel.Location = new Point(501, 43);
            toLabel.Name = "toLabel";
            toLabel.Size = new Size(26, 20);
            toLabel.TabIndex = 10;
            toLabel.Text = "To";
            // 
            // fromLabel
            // 
            fromLabel.AutoSize = true;
            fromLabel.Location = new Point(64, 37);
            fromLabel.Name = "fromLabel";
            fromLabel.Size = new Size(46, 20);
            fromLabel.TabIndex = 9;
            fromLabel.Text = "From";
            // 
            // btnExcel
            // 
            btnExcel.Location = new Point(602, 242);
            btnExcel.Name = "btnExcel";
            btnExcel.Size = new Size(137, 29);
            btnExcel.TabIndex = 8;
            btnExcel.Text = "Export Excel";
            btnExcel.UseVisualStyleBackColor = true;
            btnExcel.Click += btnExcel_Click;
            // 
            // btnSearch
            // 
            btnSearch.Location = new Point(167, 242);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(137, 29);
            btnSearch.TabIndex = 7;
            btnSearch.Text = "Search";
            btnSearch.UseVisualStyleBackColor = true;
            btnSearch.Click += btnSearch_Click;
            // 
            // chkOverweight
            // 
            chkOverweight.AutoSize = true;
            chkOverweight.Location = new Point(167, 190);
            chkOverweight.Name = "chkOverweight";
            chkOverweight.Size = new Size(113, 24);
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
            nudMinWeight.Location = new Point(167, 141);
            nudMinWeight.Name = "nudMinWeight";
            nudMinWeight.Size = new Size(150, 27);
            nudMinWeight.TabIndex = 4;
            // 
            // txtPlate
            // 
            txtPlate.Font = new Font("Simplified Arabic", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            txtPlate.Location = new Point(602, 82);
            txtPlate.Name = "txtPlate";
            txtPlate.Size = new Size(150, 32);
            txtPlate.TabIndex = 3;
            txtPlate.TextAlign = HorizontalAlignment.Right;
            // 
            // cmbLine
            // 
            cmbLine.FormattingEnabled = true;
            cmbLine.Location = new Point(167, 89);
            cmbLine.Name = "cmbLine";
            cmbLine.Size = new Size(151, 28);
            cmbLine.TabIndex = 2;
            cmbLine.Tag = "1";
            // 
            // dtTo
            // 
            dtTo.BackColor = Color.White;
            dtTo.BorderStyle = BorderStyle.FixedSingle;
            dtTo.Location = new Point(602, 38);
            dtTo.Margin = new Padding(3, 4, 3, 4);
            dtTo.MinimumSize = new Size(150, 28);
            dtTo.Name = "dtTo";
            dtTo.RightToLeft = RightToLeft.Yes;
            dtTo.Size = new Size(250, 29);
            dtTo.TabIndex = 1;
            dtTo.ValueNullable = new DateTime(2026, 8, 5, 13, 54, 35, 72);
            // 
            // dtFrom
            // 
            dtFrom.BackColor = Color.White;
            dtFrom.BorderStyle = BorderStyle.FixedSingle;
            dtFrom.Location = new Point(167, 38);
            dtFrom.Margin = new Padding(3, 4, 3, 4);
            dtFrom.MinimumSize = new Size(150, 28);
            dtFrom.Name = "dtFrom";
            dtFrom.RightToLeft = RightToLeft.Yes;
            dtFrom.Size = new Size(250, 29);
            dtFrom.TabIndex = 0;
            dtFrom.ValueNullable = new DateTime(2026, 8, 5, 13, 54, 35, 82);
            // 
            // dgvVehicles
            // 
            dgvVehicles.AllowUserToAddRows = false;
            dgvVehicles.AllowUserToDeleteRows = false;
            dgvVehicles.AllowUserToOrderColumns = true;
            dgvVehicles.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = Color.FromArgb(248, 249, 251);
            dgvVehicles.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            dgvVehicles.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvVehicles.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvVehicles.BackgroundColor = Color.White;
            dgvVehicles.BorderStyle = BorderStyle.None;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = Color.FromArgb(30, 30, 30);
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            dataGridViewCellStyle2.ForeColor = Color.White;
            dataGridViewCellStyle2.Padding = new Padding(6, 0, 6, 0);
            dataGridViewCellStyle2.SelectionBackColor = Color.FromArgb(30, 30, 30);
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            dgvVehicles.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            dgvVehicles.ColumnHeadersHeight = 36;
            dgvVehicles.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = Color.White;
            dataGridViewCellStyle3.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle3.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle3.Padding = new Padding(6, 2, 6, 2);
            dataGridViewCellStyle3.SelectionBackColor = Color.FromArgb(0, 120, 215);
            dataGridViewCellStyle3.SelectionForeColor = Color.White;
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.False;
            dgvVehicles.DefaultCellStyle = dataGridViewCellStyle3;
            dgvVehicles.EnableHeadersVisualStyles = false;
            dgvVehicles.GridColor = Color.FromArgb(220, 220, 220);
            dgvVehicles.Location = new Point(12, 312);
            dgvVehicles.Margin = new Padding(0, 8, 0, 0);
            dgvVehicles.MultiSelect = false;
            dgvVehicles.Name = "dgvVehicles";
            dgvVehicles.ReadOnly = true;
            dgvVehicles.RowHeadersVisible = false;
            dgvVehicles.RowHeadersWidth = 51;
            dgvVehicles.RowTemplate.Height = 30;
            dgvVehicles.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvVehicles.Size = new Size(987, 334);
            dgvVehicles.TabIndex = 1;
            dgvVehicles.ColumnHeaderMouseClick += dgvVehicles_ColumnHeaderMouseClick;
            dgvVehicles.DataBindingComplete += dgvVehicles_DataBindingComplete;
            // 
            // _btnPrev
            // 
            _btnPrev.Location = new Point(103, 3);
            _btnPrev.Name = "_btnPrev";
            _btnPrev.Size = new Size(94, 28);
            _btnPrev.TabIndex = 2;
            _btnPrev.Text = "Prev";
            _btnPrev.UseVisualStyleBackColor = true;
            _btnPrev.Click += _btnPrev_Click;
            // 
            // _btnFirst
            // 
            _btnFirst.Location = new Point(3, 3);
            _btnFirst.Name = "_btnFirst";
            _btnFirst.Size = new Size(94, 28);
            _btnFirst.TabIndex = 3;
            _btnFirst.Text = "First";
            _btnFirst.UseVisualStyleBackColor = true;
            _btnFirst.Click += _btnFirst_Click;
            // 
            // _btnNext
            // 
            _btnNext.Location = new Point(375, 3);
            _btnNext.Name = "_btnNext";
            _btnNext.Size = new Size(94, 28);
            _btnNext.TabIndex = 4;
            _btnNext.Text = "Next";
            _btnNext.UseVisualStyleBackColor = true;
            _btnNext.Click += _btnNext_Click;
            // 
            // _btnLast
            // 
            _btnLast.Location = new Point(475, 3);
            _btnLast.Name = "_btnLast";
            _btnLast.Size = new Size(94, 28);
            _btnLast.TabIndex = 5;
            _btnLast.Text = "Last";
            _btnLast.UseVisualStyleBackColor = true;
            _btnLast.Click += _btnLast_Click;
            // 
            // _cmbPageSize
            // 
            _cmbPageSize.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbPageSize.FormattingEnabled = true;
            _cmbPageSize.Items.AddRange(new object[] { "50", "100", "150", "200", "300", "400", "500", "1000", "10000" });
            _cmbPageSize.Location = new Point(289, 3);
            _cmbPageSize.Name = "_cmbPageSize";
            _cmbPageSize.Size = new Size(80, 28);
            _cmbPageSize.TabIndex = 6;
            _cmbPageSize.SelectedIndexChanged += _cmbPageSize_SelectedIndexChanged;
            // 
            // _pager
            // 
            _pager.Controls.Add(_btnFirst);
            _pager.Controls.Add(_btnPrev);
            _pager.Controls.Add(_lblPageInfo);
            _pager.Controls.Add(_cmbPageSize);
            _pager.Controls.Add(_btnNext);
            _pager.Controls.Add(_btnLast);
            _pager.Dock = DockStyle.Bottom;
            _pager.Location = new Point(12, 654);
            _pager.Name = "_pager";
            _pager.Size = new Size(987, 40);
            _pager.TabIndex = 7;
            // 
            // _lblPageInfo
            // 
            _lblPageInfo.AutoSize = true;
            _lblPageInfo.Location = new Point(207, 7);
            _lblPageInfo.Margin = new Padding(7);
            _lblPageInfo.Name = "_lblPageInfo";
            _lblPageInfo.Size = new Size(72, 20);
            _lblPageInfo.TabIndex = 7;
            _lblPageInfo.Text = "Page Size";
            // 
            // VehicleReportForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1011, 706);
            Controls.Add(_pager);
            Controls.Add(dgvVehicles);
            Controls.Add(grpFilters);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MinimumSize = new Size(900, 600);
            Name = "VehicleReportForm";
            Padding = new Padding(12);
            StartPosition = FormStartPosition.CenterParent;
            Text = "Vehicle Report";
            grpFilters.ResumeLayout(false);
            grpFilters.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudMaxWeight).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudMinWeight).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvVehicles).EndInit();
            _pager.ResumeLayout(false);
            _pager.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox grpFilters;
        private TextBox txtPlate;
        private ComboBox cmbLine;
        private VehicleWeightMeasurementSystemDemo.Controls.JalaliDateTimePicker dtTo;
        private VehicleWeightMeasurementSystemDemo.Controls.JalaliDateTimePicker dtFrom;
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
        private Button _btnPrev;
        private Button _btnFirst;
        private Button _btnNext;
        private Button _btnLast;
        private ComboBox _cmbPageSize;
        private FlowLayoutPanel _pager;
        private Label _lblPageInfo;
    }
}