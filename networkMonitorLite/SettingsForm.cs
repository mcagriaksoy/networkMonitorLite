// Author: mcagriaksoy - github.com/mcagriaksoy

using System;
using System.Drawing;
using System.Windows.Forms;

namespace NetworkMonitor
{
    public class SettingsForm : Form
    {
        private readonly Button btnBgColor;
        private readonly Button btnDownColor;
        private readonly Button btnUpColor;
        private readonly Button btnFont;
        private readonly Panel previewPanel;
        private readonly Label previewDown;
        private readonly Label previewUp;
        private readonly Button btnOk;
        private readonly Button btnCancel;

        public AppSettings Settings { get; private set; }

        public SettingsForm(AppSettings current)
        {
            this.Text = "Settings";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            // Open the window at center of screen
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Width = 420;
            this.Height = 260;
            this.BackColor = Color.FromArgb(20, 20, 20);

            Settings = new AppSettings
            {
                OverlayBackgroundArgb = current.OverlayBackgroundArgb,
                DownloadColorArgb = current.DownloadColorArgb,
                UploadColorArgb = current.UploadColorArgb,
                FontFamily = current.FontFamily,
                FontSize = current.FontSize,
                FontStyleInt = current.FontStyleInt
            };

            var lblBg = new Label { Text = "Overlay Background:", ForeColor = Color.White, Location = new Point(20, 20), AutoSize = true };
            btnBgColor = new Button { Text = "Select", Location = new Point(170, 16), Width = 80, BackColor = Color.FromArgb(255, 255, 255) };
            btnBgColor.Click += (s, e) => SelectColor(c => Settings.OverlayBackgroundArgb = c.ToArgb());

            var lblDown = new Label { Text = "Download Color:", ForeColor = Color.White, Location = new Point(20, 55), AutoSize = true };
            btnDownColor = new Button { Text = "Select", Location = new Point(170, 51), Width = 80, BackColor = Color.FromArgb(255, 255, 255) };
            btnDownColor.Click += (s, e) => SelectColor(c => Settings.DownloadColorArgb = c.ToArgb());

            var lblUp = new Label { Text = "Upload Color:", ForeColor = Color.White, Location = new Point(20, 90), AutoSize = true };
            btnUpColor = new Button { Text = "Select", Location = new Point(170, 86), Width = 80, BackColor = Color.FromArgb(255, 255, 255) };
            btnUpColor.Click += (s, e) => SelectColor(c => Settings.UploadColorArgb = c.ToArgb());

            var lblFont = new Label { Text = "Overlay Font:", ForeColor = Color.White, Location = new Point(20, 125), AutoSize = true };
            btnFont = new Button { Text = "Choose", Location = new Point(170, 121), Width = 80, BackColor = Color.FromArgb(255, 255, 255) };
            btnFont.Click += (s, e) => SelectFont();

            previewPanel = new Panel { Location = new Point(270, 16), Size = new Size(120, 80), BackColor = Color.FromArgb(30,30,30) };
            previewPanel.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, previewPanel.ClientRectangle, Color.Gray, ButtonBorderStyle.Solid);

            var lblDownArrow = new Label { Text = "\u2193", Location = new Point(5, 8), Size = new Size(15, 20), ForeColor = Color.LightGreen };
            var lblUpArrow = new Label { Text = "\u2191", Location = new Point(5, 45), Size = new Size(15, 20), ForeColor = Color.Orange };
            previewDown = new Label { Text = "0.00 KB/s", Location = new Point(25, 8), Size = new Size(90, 20), ForeColor = Color.LightGreen };
            previewUp = new Label { Text = "0.00 KB/s", Location = new Point(25, 45), Size = new Size(90, 20), ForeColor = Color.Orange };
            previewPanel.Controls.Add(lblDownArrow);
            previewPanel.Controls.Add(lblUpArrow);
            previewPanel.Controls.Add(previewDown);
            previewPanel.Controls.Add(previewUp);

            btnOk = new Button { Text = "Save", DialogResult = DialogResult.OK, Location = new Point(230, 170), Width = 75, BackColor = Color.FromArgb(255, 255, 255) };
            btnCancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Location = new Point(315, 170), Width = 75 , BackColor = Color.FromArgb(255, 255, 255) };

            this.Controls.AddRange(new Control[]
            {
                lblBg, btnBgColor,
                lblDown, btnDownColor,
                lblUp, btnUpColor,
                lblFont, btnFont,
                previewPanel,
                btnOk, btnCancel
            });

            UpdatePreview();
        }

        private void SelectColor(Action<Color> setter)
        {
            using var dlg = new ColorDialog();
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                setter(dlg.Color);
                UpdatePreview();
            }
        }

        private void SelectFont()
        {
            using var dlg = new FontDialog();
            dlg.Font = Settings.CreateOverlayFont();
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                Settings.FontFamily = dlg.Font.FontFamily.Name;
                Settings.FontSize = dlg.Font.Size;
                Settings.FontStyleInt = (int)dlg.Font.Style;
                UpdatePreview();
            }
        }

        private void UpdatePreview()
        {
            previewPanel.BackColor = Color.FromArgb(40, Color.FromArgb(Settings.OverlayBackgroundArgb));
            var font = Settings.CreateOverlayFont();
            previewDown.Font = font;
            previewUp.Font = font;
            previewDown.ForeColor = Color.FromArgb(Settings.DownloadColorArgb);
            previewUp.ForeColor = Color.FromArgb(Settings.UploadColorArgb);
        }
    }
}
