namespace RmtoSync.Ui;

partial class MainForm
{
    private System.ComponentModel.IContainer? components;
    private System.Windows.Forms.Label titleLabel;
    private System.Windows.Forms.FlowLayoutPanel buttonPanel;
    private System.Windows.Forms.Label _statusLabel;
    private System.Windows.Forms.Label _lastResultLabel;
    private System.Windows.Forms.DataGridView _recentGrid;
    private System.Windows.Forms.DataGridViewTextBoxColumn colTime;
    private System.Windows.Forms.DataGridViewTextBoxColumn colResult;
    private System.Windows.Forms.DataGridViewTextBoxColumn colOperation;
    private System.Windows.Forms.DataGridViewTextBoxColumn colPhotoId;
    private System.Windows.Forms.DataGridViewTextBoxColumn colCode;
    private System.Windows.Forms.DataGridViewTextBoxColumn colSummary;
    private System.Windows.Forms.Button _startStopButton;
    private System.Windows.Forms.Button _lastErrorButton;
    private System.Windows.Forms.Button _logsButton;
    private System.Windows.Forms.Button _exitButton;
    private System.Windows.Forms.Timer _clockTimer;
    private System.Windows.Forms.Timer _queueTimer;

    private void InitializeComponent()
    {
        System.Windows.Forms.DataGridViewCellStyle headerStyle = new System.Windows.Forms.DataGridViewCellStyle();
        System.Windows.Forms.DataGridViewCellStyle cellStyle = new System.Windows.Forms.DataGridViewCellStyle();
        System.Windows.Forms.DataGridViewCellStyle alternatingStyle = new System.Windows.Forms.DataGridViewCellStyle();

        this.components = new System.ComponentModel.Container();
        this.titleLabel = new System.Windows.Forms.Label();
        this.buttonPanel = new System.Windows.Forms.FlowLayoutPanel();
        this._statusLabel = new System.Windows.Forms.Label();
        this._lastResultLabel = new System.Windows.Forms.Label();
        this._recentGrid = new System.Windows.Forms.DataGridView();
        this.colTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.colResult = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.colOperation = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.colPhotoId = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.colCode = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.colSummary = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this._startStopButton = new System.Windows.Forms.Button();
        this._lastErrorButton = new System.Windows.Forms.Button();
        this._logsButton = new System.Windows.Forms.Button();
        this._exitButton = new System.Windows.Forms.Button();
        this._clockTimer = new System.Windows.Forms.Timer(this.components);
        this._queueTimer = new System.Windows.Forms.Timer(this.components);

        this.SuspendLayout();

        //
        // titleLabel
        //
        this.titleLabel.AutoSize = false;
        this.titleLabel.Dock = System.Windows.Forms.DockStyle.Top;
        this.titleLabel.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
        this.titleLabel.Height = 38;
        this.titleLabel.Name = "titleLabel";
        this.titleLabel.Text = "Send records to Rahdari terminal";
        this.titleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

        //
        // buttonPanel
        //
        this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.buttonPanel.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
        this.buttonPanel.Height = 52;
        this.buttonPanel.Name = "buttonPanel";
        this.buttonPanel.Padding = new System.Windows.Forms.Padding(16, 10, 16, 10);
        this.buttonPanel.WrapContents = false;

        //
        // _statusLabel
        //
        this._statusLabel.AutoSize = false;
        this._statusLabel.BackColor = System.Drawing.Color.FromArgb(247, 249, 252);
        this._statusLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this._statusLabel.Dock = System.Windows.Forms.DockStyle.Top;
        this._statusLabel.Font = new System.Drawing.Font("Segoe UI", 9.5F);
        this._statusLabel.ForeColor = System.Drawing.Color.FromArgb(33, 37, 41);
        this._statusLabel.Height = 176;
        this._statusLabel.Name = "_statusLabel";
        this._statusLabel.Padding = new System.Windows.Forms.Padding(10, 8, 10, 8);
        this._statusLabel.Text = "آماده شروع سرویس...";
        this._statusLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;

        //
        // _lastResultLabel
        //
        this._lastResultLabel.AutoSize = false;
        this._lastResultLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this._lastResultLabel.Dock = System.Windows.Forms.DockStyle.Bottom;
        this._lastResultLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        this._lastResultLabel.Height = 64;
        this._lastResultLabel.Name = "_lastResultLabel";
        this._lastResultLabel.Padding = new System.Windows.Forms.Padding(10, 6, 10, 6);
        this._lastResultLabel.Text = "نتیجه قبلی راهداری: نامشخص";
        this._lastResultLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

        //
        // _recentGrid
        //
        headerStyle.BackColor = System.Drawing.Color.FromArgb(33, 37, 41);
        headerStyle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        headerStyle.ForeColor = System.Drawing.Color.White;
        cellStyle.Font = new System.Drawing.Font("Segoe UI", 9F);
        cellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(200, 230, 201);
        cellStyle.SelectionForeColor = System.Drawing.Color.Black;
        alternatingStyle.BackColor = System.Drawing.Color.FromArgb(248, 249, 250);
        this._recentGrid.AllowUserToAddRows = false;
        this._recentGrid.AllowUserToDeleteRows = false;
        this._recentGrid.AllowUserToResizeRows = false;
        this._recentGrid.AlternatingRowsDefaultCellStyle = alternatingStyle;
        this._recentGrid.AutoGenerateColumns = false;
        this._recentGrid.BackgroundColor = System.Drawing.Color.White;
        this._recentGrid.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this._recentGrid.ColumnHeadersDefaultCellStyle = headerStyle;
        this._recentGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        this._recentGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[]
        {
            this.colTime,
            this.colResult,
            this.colOperation,
            this.colPhotoId,
            this.colCode,
            this.colSummary
        });
        this._recentGrid.DefaultCellStyle = cellStyle;
        this._recentGrid.Dock = System.Windows.Forms.DockStyle.Fill;
        this._recentGrid.EnableHeadersVisualStyles = false;
        this._recentGrid.GridColor = System.Drawing.Color.FromArgb(222, 226, 230);
        this._recentGrid.MultiSelect = false;
        this._recentGrid.Name = "_recentGrid";
        this._recentGrid.ReadOnly = true;
        this._recentGrid.RowHeadersVisible = false;
        this._recentGrid.RowTemplate.Height = 26;
        this._recentGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
        this._recentGrid.CellDoubleClick += this.RecentGrid_CellDoubleClick;

        //
        // colTime
        //
        this.colTime.HeaderText = "زمان";
        this.colTime.Name = "colTime";
        this.colTime.ReadOnly = true;
        this.colTime.Width = 72;

        //
        // colResult
        //
        this.colResult.HeaderText = "نتیجه";
        this.colResult.Name = "colResult";
        this.colResult.ReadOnly = true;
        this.colResult.Width = 130;

        //
        // colOperation
        //
        this.colOperation.HeaderText = "عملیات";
        this.colOperation.Name = "colOperation";
        this.colOperation.ReadOnly = true;
        this.colOperation.Width = 120;

        //
        // colPhotoId
        //
        this.colPhotoId.HeaderText = "PhotoId";
        this.colPhotoId.Name = "colPhotoId";
        this.colPhotoId.ReadOnly = true;
        this.colPhotoId.Width = 82;

        //
        // colCode
        //
        this.colCode.HeaderText = "کد";
        this.colCode.Name = "colCode";
        this.colCode.ReadOnly = true;
        this.colCode.Width = 55;

        //
        // colSummary
        //
        this.colSummary.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
        this.colSummary.HeaderText = "توضیح";
        this.colSummary.MinimumWidth = 200;
        this.colSummary.Name = "colSummary";
        this.colSummary.ReadOnly = true;

        //
        // _startStopButton
        //
        this._startStopButton.AutoSize = true;
        this._startStopButton.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
        this._startStopButton.MinimumSize = new System.Drawing.Size(150, 32);
        this._startStopButton.Name = "_startStopButton";
        this._startStopButton.Text = "Start service";
        this._startStopButton.UseVisualStyleBackColor = true;
        this._startStopButton.Click += this.StartStopButton_Click;

        //
        // _lastErrorButton
        //
        this._lastErrorButton.AutoSize = true;
        this._lastErrorButton.FlatAppearance.BorderSize = 1;
        this._lastErrorButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this._lastErrorButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        this._lastErrorButton.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
        this._lastErrorButton.MinimumSize = new System.Drawing.Size(150, 32);
        this._lastErrorButton.Name = "_lastErrorButton";
        this._lastErrorButton.Text = "نامشخص";
        this._lastErrorButton.UseVisualStyleBackColor = false;
        this._lastErrorButton.Click += this.LastErrorButton_Click;

        //
        // _logsButton
        //
        this._logsButton.AutoSize = true;
        this._logsButton.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
        this._logsButton.MinimumSize = new System.Drawing.Size(150, 32);
        this._logsButton.Name = "_logsButton";
        this._logsButton.Text = "Logs folder";
        this._logsButton.UseVisualStyleBackColor = true;
        this._logsButton.Click += this.LogsButton_Click;

        //
        // _exitButton
        //
        this._exitButton.AutoSize = true;
        this._exitButton.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
        this._exitButton.MinimumSize = new System.Drawing.Size(150, 32);
        this._exitButton.Name = "_exitButton";
        this._exitButton.Text = "Exit";
        this._exitButton.UseVisualStyleBackColor = true;
        this._exitButton.Click += this.ExitButton_Click;

        //
        // _clockTimer
        //
        this._clockTimer.Interval = 1000;
        this._clockTimer.Tick += this.ClockTimer_Tick;

        //
        // _queueTimer
        //
        this._queueTimer.Interval = 30000;
        this._queueTimer.Tick += this.QueueTimer_Tick;

        //
        // MainForm
        //
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(900, 560);
        this.Controls.Add(this._recentGrid);
        this.Controls.Add(this._lastResultLabel);
        this.Controls.Add(this.buttonPanel);
        this.Controls.Add(this._statusLabel);
        this.Controls.Add(this.titleLabel);
        this.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
        this.MaximizeBox = true;
        this.MinimizeBox = true;
        this.Name = "MainForm";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "Rmto Sync";
        this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
        this.FormClosing += this.MainForm_FormClosing;
        this.Load += this.MainForm_Load;

        this.buttonPanel.Controls.Add(this._startStopButton);
        this.buttonPanel.Controls.Add(this._lastErrorButton);
        this.buttonPanel.Controls.Add(this._logsButton);
        this.buttonPanel.Controls.Add(this._exitButton);

        this.ResumeLayout(false);
    }
}
