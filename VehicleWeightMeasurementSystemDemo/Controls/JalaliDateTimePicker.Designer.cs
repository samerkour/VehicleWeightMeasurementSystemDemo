namespace VehicleWeightMeasurementSystemDemo.Controls
{
    partial class JalaliDateTimePicker
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnDrop = new Button();
            txtInput = new TextBox();
            SuspendLayout();
            // 
            // btnDrop
            // 
            btnDrop.BackColor = Color.FromArgb(33, 150, 243);
            btnDrop.Dock = DockStyle.Right;
            btnDrop.FlatAppearance.BorderSize = 0;
            btnDrop.FlatStyle = FlatStyle.Flat;
            btnDrop.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnDrop.ForeColor = Color.White;
            btnDrop.Location = new Point(154, 0);
            btnDrop.Margin = new Padding(0);
            btnDrop.Name = "btnDrop";
            btnDrop.Size = new Size(32, 28);
            btnDrop.TabIndex = 0;
            btnDrop.Text = "▼";
            btnDrop.UseVisualStyleBackColor = false;
            btnDrop.Click += btnDrop_Click;
            // 
            // txtInput
            // 
            txtInput.BorderStyle = BorderStyle.None;
            txtInput.Dock = DockStyle.Fill;
            txtInput.Font = new Font("Segoe UI", 9.5F);
            txtInput.Location = new Point(0, 0);
            txtInput.Margin = new Padding(0);
            txtInput.Name = "txtInput";
            txtInput.Size = new Size(154, 28);
            txtInput.TabIndex = 1;
            txtInput.TextAlign = HorizontalAlignment.Right;
            txtInput.TextChanged += txtInput_TextChanged;
            txtInput.KeyPress += txtInput_KeyPress;
            txtInput.Leave += txtInput_Leave;
            // 
            // JalaliDateTimePicker
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            BorderStyle = BorderStyle.FixedSingle;
            Controls.Add(txtInput);
            Controls.Add(btnDrop);
            Margin = new Padding(3, 4, 3, 4);
            MinimumSize = new Size(150, 28);
            Name = "JalaliDateTimePicker";
            RightToLeft = RightToLeft.Yes;
            Size = new Size(186, 28);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnDrop;
        private TextBox txtInput;
    }
}
