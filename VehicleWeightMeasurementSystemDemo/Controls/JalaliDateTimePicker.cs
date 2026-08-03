using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace VehicleWeightMeasurementSystemDemo.Controls
{
    /// <summary>
    /// Fully-featured Jalali (Shamsi) DateTimePicker.
    /// Internal storage is a Gregorian DateTime; all display/calculation uses PersianCalendar.
    /// Dropdown month view (7x6), Persian month/day names, Persian digits, RTL.
    /// </summary>
    [ToolboxItem(true)]
    [DefaultProperty(nameof(Value))]
    [DefaultEvent(nameof(ValueChanged))]
    [DesignerCategory("UserControl")]
    public partial class JalaliDateTimePicker : UserControl
    {
        private DateTime _value = DateTime.Now;
        private DateTime _viewYearMonth;   // used to render the visible month grid
        private bool _suppressTextEvent;
        private bool _nullable;
        private DateTime _minDate = DateTime.MinValue;
        private DateTime _maxDate = DateTime.MaxValue;
        private bool _showTime = true;
        private string _errorText = "";

        private ToolStripDropDown _dropDown;
        private TableLayoutPanel _monthGrid;
        private Label _lblTitle;
        private readonly Button[,] _dayCells = new Button[6, 7];
        private ToolTip _tooltip;

        public event EventHandler ValueChanged;
        public new event EventHandler TextChanged;

        [Browsable(false)]
        [Bindable(BindableSupport.Yes)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DateTime Value
        {
            get => _value;
            set
            {
                var v = Clamp(value);
                if (v == _value)
                    return;
                _value = v;
                UpdateText();
                ValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>Support nullable date</summary>
        [Browsable(true)]
        [Category("Data")]
        [DefaultValue(null)]
        public DateTime? ValueNullable
        {
            get => AllowNull && !string.IsNullOrWhiteSpace(txtInput.Text)
                ? _value
                : (AllowNull ? null : _value);
            set
            {
                if (value.HasValue)
                    Value = value.Value;
                else if (_nullable)
                {
                    _value = DateTime.Now;
                    UpdateText();
                    ValueChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        [Browsable(true)]
        [Category("Data")]
        [DefaultValue(false)]
        public bool AllowNull
        {
            get => _nullable;
            set
            {
                _nullable = value;
                if (!value && _value == DateTime.MinValue)
                    Value = DateTime.Now;
            }
        }

        [Browsable(true)]
        [Category("Behavior")]
        [DefaultValue("0001-01-01T00:00:00")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DateTime MinDate
        {
            get => _minDate;
            set
            {
                _minDate = value;
                if (_value < _minDate)
                    Value = _minDate;
            }
        }

        [Browsable(true)]
        [Category("Behavior")]
        [DefaultValue("9999-12-31T23:59:59")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DateTime MaxDate
        {
            get => _maxDate;
            set
            {
                _maxDate = value;
                if (_value > _maxDate)
                    Value = _maxDate;
            }
        }

        [Browsable(true)]
        [Category("Behavior")]
        [DefaultValue(true)]
        public bool ShowTime
        {
            get => _showTime;
            set
            {
                _showTime = value;
                UpdateText();
            }
        }

        [Browsable(true)]
        [Category("Behavior")]
        [DefaultValue("")]
        public string ErrorText
        {
            get => _errorText;
            set
            {
                _errorText = value ?? "";
                ToolTip = _errorText;
            }
        }

        [Browsable(true)]
        [Category("Behavior")]
        [DefaultValue("")]
        public string ToolTip
        {
            get => _tooltip?.GetToolTip(this) ?? "";
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    return;
                _tooltip ??= new ToolTip { AutoPopDelay = 6000 };
                _tooltip.SetToolTip(this, value);
            }
        }

        public JalaliDateTimePicker()
        {
            InitializeComponent();

            RightToLeft = RightToLeft.Yes;
            _viewYearMonth = _value;

            BuildCalendar();
            UpdateText();
        }

        // ================= UI construction =================

        private void BuildCalendar()
        {
            var panel = new TableLayoutPanel
            {
                AutoSize = false,
                ColumnCount = 7,
                RowCount = 8,
                Padding = new Padding(6),
                BackColor = Color.White,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None,
                RightToLeft = RightToLeft.Yes,
                Size = new Size(308, 252)
            };

            panel.ColumnStyles.Clear();
            for (int c = 0; c < 7; c++)
                panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 42));

            panel.RowStyles.Clear();
            for (int r = 0; r < 8; r++)
                panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));

            // Row 0: title / navigation
            _lblTitle = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                Text = ""
            };

            var btnPrevYear = MakeNavButton("«", -12);
            var btnPrevMonth = MakeNavButton("‹", -1);
            var btnNextMonth = MakeNavButton("›", +1);
            var btnNextYear = MakeNavButton("»", +12);

            var navLeft = new Panel { Dock = DockStyle.Fill };
            navLeft.Controls.Add(btnPrevMonth);
            btnPrevMonth.Dock = DockStyle.Fill;
            navLeft.Width = 60;

            var navRight = new Panel { Dock = DockStyle.Fill };
            navRight.Controls.Add(btnNextMonth);
            btnNextMonth.Dock = DockStyle.Fill;
            navRight.Width = 60;

            panel.Controls.Add(btnPrevYear, 0, 0);
            panel.Controls.Add(navLeft, 1, 0);
            panel.Controls.Add(_lblTitle, 2, 0);
            panel.SetColumnSpan(_lblTitle, 3);
            panel.Controls.Add(navRight, 5, 0);
            panel.Controls.Add(btnNextYear, 6, 0);

            // Row 1: day headers (شنبه .. جمعه)
            for (int c = 0; c < 7; c++)
            {
                panel.Controls.Add(new Label
                {
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(80, 80, 80),
                    Text = PersianCalendarHelper.ShortDayNames[c]
                }, c, 1);
            }

            // Rows 2..7: 6x7 day cells
            for (int r = 0; r < 6; r++)
            {
                for (int c = 0; c < 7; c++)
                {
                    var cell = new Button
                    {
                        Dock = DockStyle.Fill,
                        FlatStyle = FlatStyle.Flat,
                        FlatAppearance = { BorderSize = 0 },
                        BackColor = Color.White,
                        ForeColor = Color.FromArgb(30, 30, 30),
                        Font = new Font("Segoe UI", 9F),
                        Cursor = Cursors.Hand,
                        Tag = null,
                        Margin = new Padding(1)
                    };
                    cell.Click += DayCell_Click;
                    _dayCells[r, c] = cell;
                    panel.Controls.Add(cell, c, r + 2);
                }
            }

            _monthGrid = panel;

            _dropDown = new ToolStripDropDown();
            _dropDown.AutoClose = true;
            _dropDown.Margin = Padding.Empty;
            _dropDown.Padding = Padding.Empty;
            _dropDown.BackColor = Color.White;

            var host = new ToolStripControlHost(panel)
            {
                AutoSize = false,
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                Size = new Size(308, 252)
            };
            _dropDown.Items.Add(host);
        }

        private Button MakeNavButton(string text, int monthDelta)
        {
            var btn = new Button
            {
                Dock = DockStyle.Fill,
                Text = text,
                Width = 30,
                Height = 28,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                BackColor = Color.FromArgb(240, 244, 248),
                ForeColor = Color.FromArgb(30, 30, 30),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btn.Click += (s, e) => NavigateMonth(monthDelta);
            return btn;
        }

        // ================= navigation / rendering =================

        private void NavigateMonth(int monthDelta)
        {
            _viewYearMonth = _viewYearMonth.AddMonths(monthDelta);
            RenderCalendar();
        }

        private void RenderCalendar()
        {
            int year = PersianCalendarHelper.GetYear(_viewYearMonth);
            int month = PersianCalendarHelper.GetMonth(_viewYearMonth);
            int daysInMonth = PersianCalendarHelper.GetDaysInMonth(year, month);
            int offset = PersianCalendarHelper.GetFirstDayOffset(year, month);

            _lblTitle.Text =
                $"{PersianCalendarHelper.MonthNames[month]} " +
                PersianCalendarHelper.ToPersianDigits(year.ToString());

            int todayJYear = PersianCalendarHelper.GetYear(DateTime.Now);
            int todayJMonth = PersianCalendarHelper.GetMonth(DateTime.Now);
            int todayJDay = PersianCalendarHelper.GetDay(DateTime.Now);

            int selJYear = PersianCalendarHelper.GetYear(_value);
            int selJMonth = PersianCalendarHelper.GetMonth(_value);
            int selJDay = PersianCalendarHelper.GetDay(_value);

            for (int r = 0; r < 6; r++)
            {
                for (int c = 0; c < 7; c++)
                {
                    int dayNumber = (r * 7 + c) - offset + 1;
                    var cell = _dayCells[r, c];

                    if (dayNumber < 1 || dayNumber > daysInMonth)
                    {
                        cell.Text = "";
                        cell.Enabled = false;
                        cell.BackColor = Color.White;
                        cell.Tag = null;
                        continue;
                    }

                    cell.Enabled = true;
                    cell.Text = PersianCalendarHelper.ToPersianDigits(dayNumber.ToString());
                    cell.Tag = dayNumber;

                    bool isToday = dayNumber == todayJDay && month == todayJMonth && year == todayJYear;
                    bool isSelected = dayNumber == selJDay && month == selJMonth && year == selJYear;

                    if (isSelected)
                    {
                        cell.BackColor = Color.FromArgb(33, 150, 243);
                        cell.ForeColor = Color.White;
                    }
                    else if (isToday)
                    {
                        cell.BackColor = Color.FromArgb(200, 230, 201);
                        cell.ForeColor = Color.FromArgb(30, 30, 30);
                    }
                    else
                    {
                        cell.BackColor = Color.White;
                        cell.ForeColor = Color.FromArgb(30, 30, 30);
                    }
                }
            }
        }

        private void DayCell_Click(object sender, EventArgs e)
        {
            var cell = (Button)sender;
            if (cell.Tag is not int dayNumber)
                return;

            int year = PersianCalendarHelper.GetYear(_viewYearMonth);
            int month = PersianCalendarHelper.GetMonth(_viewYearMonth);

            var date = PersianCalendarHelper.ToGregorian(
                year, month, dayNumber,
                _showTime ? _value.Hour : 0,
                _showTime ? _value.Minute : 0);

            Value = date;
            _dropDown.Close();
        }

        private void btnDrop_Click(object sender, EventArgs e)
        {
            _viewYearMonth = _value;
            RenderCalendar();

            _dropDown.Show(this, new Point(0, Height));
        }

        // ================= text sync & validation =================

        private void UpdateText()
        {
            _suppressTextEvent = true;
            txtInput.Text = PersianCalendarHelper.FormatJalali(_value, _showTime);
            _suppressTextEvent = false;
        }

        private void txtInput_TextChanged(object sender, EventArgs e)
        {
            if (_suppressTextEvent)
                return;

            TextChanged?.Invoke(this, EventArgs.Empty);
        }

        private void txtInput_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                CommitInput();
                e.Handled = true;
            }
        }

        private void txtInput_Leave(object sender, EventArgs e)
        {
            CommitInput();
        }

        private void CommitInput()
        {
            string text = txtInput.Text.Trim();

            if (string.IsNullOrEmpty(text))
            {
                if (_nullable)
                {
                    _value = DateTime.Now;
                    ErrorText = "";
                    UpdateText();
                    ValueChanged?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    ErrorText = "تاریخ نمی‌تواند خالی باشد.";
                }
                return;
            }

            if (PersianCalendarHelper.TryParseJalali(text, out DateTime parsed))
            {
                var clamped = Clamp(parsed);
                if (clamped != parsed)
                {
                    ErrorText = "تاریخ خارج از محدوده مجاز است.";
                    return;
                }

                ErrorText = "";
                if (clamped != _value)
                {
                    _value = clamped;
                    ValueChanged?.Invoke(this, EventArgs.Empty);
                }
                UpdateText();
            }
            else
            {
                ErrorText = "فرمت تاریخ صحیح نیست (مثال: ۱۴۰۴/۰۵/۱۲).";
            }
        }

        private DateTime Clamp(DateTime value)
        {
            if (value < _minDate)
                return _minDate;
            if (value > _maxDate)
                return _maxDate;
            return value;
        }
    }
}
