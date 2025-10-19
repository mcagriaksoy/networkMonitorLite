// Author: mcagriaksoy - github.com/mcagriaksoy

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Net.NetworkInformation;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace NetworkMonitor
{
    public class MainForm : Form
    {
        private Label lblDownload = null!;
        private Label lblUpload = null!;
        private Label lblTotalDownload = null!;
        private Label lblTotalUpload = null!;
        private System.Windows.Forms.Timer updateTimer = null!;
        private NetworkInterface[] networkInterfaces = null!;
    private DateTime lastUpdate;
        private ComboBox cmbInterfaces = null!;
        private NetworkInterface? selectedInterface;
    private NetworkStatsTracker? statsTracker;
        private NotifyIcon notifyIcon = null!;
        private ContextMenuStrip contextMenu = null!;
        private string currentDownloadSpeed = "0";
        private string currentUploadSpeed = "0";
    private Form? taskbarOverlay;
    private Label? taskbarDownloadLabel;
    private Label? taskbarUploadLabel;
        private bool showTaskbarOverlay = true;
    private AppSettings settings = null!;

        public MainForm()
        {
            settings = SettingsService.Load();
            InitializeComponent();
            LoadNetworkInterfaces();
            lastUpdate = DateTime.Now;

        }

        // Main Window Initialization
        private void InitializeComponent()
        {
            this.Text = "Network Monitor Lite™";
            this.Width = 450;
            this.Height = 300;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(20, 20, 20);

            // Setup system tray icon
            contextMenu = new ContextMenuStrip();

            var showMenuItem = new ToolStripMenuItem("Show Window");
            showMenuItem.Click += (s, e) => ShowWindow();
            contextMenu.Items.Add(showMenuItem);

            contextMenu.Items.Add(new ToolStripSeparator());

            var settingsItem = new ToolStripMenuItem("Settings...");
            settingsItem.Click += (s, e) => OpenSettings();
            contextMenu.Items.Add(settingsItem);

            var toggleOverlayMenuItem = new ToolStripMenuItem("Show Taskbar Widget")
            {
                Checked = showTaskbarOverlay,
                CheckOnClick = true
            };
            toggleOverlayMenuItem.CheckedChanged += (s, e) =>
            {
                showTaskbarOverlay = toggleOverlayMenuItem.Checked;
                if (showTaskbarOverlay)
                    CreateTaskbarOverlay();
                else
                    HideTaskbarOverlay();
            };
            contextMenu.Items.Add(toggleOverlayMenuItem);

            contextMenu.Items.Add(new ToolStripSeparator());

            var exitMenuItem = new ToolStripMenuItem("Exit");
            exitMenuItem.Click += (s, e) =>
            {
                notifyIcon.Visible = false;
                HideTaskbarOverlay();
                Application.Exit();
            };
            contextMenu.Items.Add(exitMenuItem);

            notifyIcon = new NotifyIcon
            {
                Icon = CreateTrayIcon("0", "0"),
                Visible = true,
                Text = "Network Monitor\n↓ 0 KB/s\n↑ 0 KB/s",
                ContextMenuStrip = contextMenu
            };
            notifyIcon.DoubleClick += (s, e) => ShowWindow();

            // Create taskbar overlay if enabled
            if (showTaskbarOverlay)
            {
                CreateTaskbarOverlay();
            }

            // Interface selector
            Label lblInterface = new Label
            {
                Text = "Network Interface:",
                Location = new Point(20, 20),
                Size = new Size(120, 20),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };
            this.Controls.Add(lblInterface);

            cmbInterfaces = new ComboBox
            {
                Location = new Point(150, 18),
                Size = new Size(260, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F)
            };
            cmbInterfaces.SelectedIndexChanged += CmbInterfaces_SelectedIndexChanged;
            this.Controls.Add(cmbInterfaces);

            // Download speed label
            Label lblDownloadTitle = new Label
            {
                Text = "Download Speed:",
                Location = new Point(20, 70),
                Size = new Size(150, 25),
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            this.Controls.Add(lblDownloadTitle);

            lblDownload = new Label
            {
                Text = "0.00 KB/s",
                Location = new Point(180, 70),
                Size = new Size(230, 25),
                ForeColor = Color.FromArgb(0, 200, 83),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleRight
            };
            this.Controls.Add(lblDownload);

            // Upload speed label
            Label lblUploadTitle = new Label
            {
                Text = "Upload Speed:",
                Location = new Point(20, 105),
                Size = new Size(150, 25),
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            this.Controls.Add(lblUploadTitle);

            lblUpload = new Label
            {
                Text = "0.00 KB/s",
                Location = new Point(180, 105),
                Size = new Size(230, 25),
                ForeColor = Color.FromArgb(255, 185, 0),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleRight
            };
            this.Controls.Add(lblUpload);

            // Separator line
            Panel separator = new Panel
            {
                Location = new Point(20, 145),
                Size = new Size(390, 2),
                BackColor = Color.Gray
            };
            this.Controls.Add(separator);

            // Total download label
            Label lblTotalDownloadTitle = new Label
            {
                Text = "Total Downloaded:",
                Location = new Point(20, 165),
                Size = new Size(150, 25),
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };
            this.Controls.Add(lblTotalDownloadTitle);

            lblTotalDownload = new Label
            {
                Text = "0.00 MB",
                Location = new Point(180, 165),
                Size = new Size(230, 25),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleRight
            };
            this.Controls.Add(lblTotalDownload);

            // Total upload label
            Label lblTotalUploadTitle = new Label
            {
                Text = "Total Uploaded:",
                Location = new Point(20, 195),
                Size = new Size(150, 25),
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };
            this.Controls.Add(lblTotalUploadTitle);

            lblTotalUpload = new Label
            {
                Text = "0.00 MB",
                Location = new Point(180, 195),
                Size = new Size(230, 25),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleRight
            };
            this.Controls.Add(lblTotalUpload);

            // Timer for updates
            updateTimer = new System.Windows.Forms.Timer
            {
                Interval = 1000 // Update every second
            };
            updateTimer.Tick += UpdateTimer_Tick;
            updateTimer.Start();

            // Add Author information (bottom, multi-line link)
            LinkLabel lblAuthor = new LinkLabel
            {
                Text = "networkMonitorLite™ by mcagriaksoy - 2025\nFor support, visit: github.com/mcagriaksoy/NetworkMonitorLite",
                Dock = DockStyle.Bottom,
                Height = 36,
                ForeColor = Color.Gray,
                LinkColor = Color.Silver,
                ActiveLinkColor = Color.White,
                Font = new Font("Segoe UI", 8F, FontStyle.Italic),
                TextAlign = ContentAlignment.MiddleRight,
                AutoSize = false,
                Padding = new Padding(0, 2, 8, 6)
            };
            lblAuthor.LinkClicked += (s, e) =>
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "https://github.com/mcagriaksoy/NetworkMonitorLite",
                    UseShellExecute = true
                });
            };

            this.Controls.Add(lblAuthor);
        }

        private void ShowWindow()
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.BringToFront();
        }

        private void CreateTaskbarOverlay()
        {
            if (taskbarOverlay != null)
            {
                taskbarOverlay.Close();
                taskbarOverlay.Dispose();
            }

            taskbarOverlay = new Form
            {
                FormBorderStyle = FormBorderStyle.None,
                StartPosition = FormStartPosition.Manual,
                TopMost = true,
                ShowInTaskbar = false,
                BackColor = settings.OverlayBackgroundColor,
                Width = 60,
                Height = 40,
                Opacity = 0.85
            };

            // Position the overlay near the system tray (bottom-right of screen)
            PositionTaskbarOverlay();

            // Download label with arrow
            Label downArrow = new Label
            {
                Text = "↓",
                Location = new Point(5, 3),
                Size = new Size(15, 15),
                ForeColor = settings.DownloadColor,
                Font = settings.CreateOverlayFont()
            };
            taskbarOverlay.Controls.Add(downArrow);

            taskbarDownloadLabel = new Label
            {
                Text = "0.00 KB/s",
                Location = new Point(22, 3),
                Size = new Size(95, 15),
                ForeColor = settings.DownloadColor,
                Font = settings.CreateOverlayFont(),
                TextAlign = ContentAlignment.MiddleLeft
            };
            taskbarOverlay.Controls.Add(taskbarDownloadLabel);

            // Upload label with arrow
            Label upArrow = new Label
            {
                Text = "↑",
                Location = new Point(5, 21),
                Size = new Size(15, 15),
                ForeColor = settings.UploadColor,
                Font = settings.CreateOverlayFont()
            };
            taskbarOverlay.Controls.Add(upArrow);

            taskbarUploadLabel = new Label
            {
                Text = "0.00 KB/s",
                Location = new Point(22, 21),
                Size = new Size(95, 15),
                ForeColor = settings.UploadColor,
                Font = settings.CreateOverlayFont(),
                TextAlign = ContentAlignment.MiddleLeft
            };
            taskbarOverlay.Controls.Add(taskbarUploadLabel);

            // Allow dragging the overlay
            taskbarOverlay.MouseDown += TaskbarOverlay_MouseDown;
            taskbarOverlay.MouseMove += TaskbarOverlay_MouseMove;
            taskbarOverlay.MouseUp += TaskbarOverlay_MouseUp;

            // Add border
            taskbarOverlay.Paint += (s, e) =>
            {
                ControlPaint.DrawBorder(e.Graphics, taskbarOverlay.ClientRectangle,
                    Color.FromArgb(60, 60, 60), ButtonBorderStyle.Solid);
            };

            // Double-click to show main window
            taskbarOverlay.DoubleClick += (s, e) => ShowWindow();

            taskbarOverlay.Show();
            EnsureOverlayTopMost();
        }

        private void OpenSettings()
        {
            using var dlg = new SettingsForm(settings);
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                settings = dlg.Settings;
                SettingsService.Save(settings);
                // Re-apply on overlay
                if (taskbarOverlay != null && !taskbarOverlay.IsDisposed)
                {
                    CreateTaskbarOverlay();
                }
            }
        }

        private Point dragOffset;
        private bool isDragging = false;

        private void TaskbarOverlay_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                dragOffset = new Point(e.X, e.Y);
            }
        }

        private void TaskbarOverlay_MouseMove(object? sender, MouseEventArgs e)
        {
            if (isDragging && taskbarOverlay != null)
            {
                Point newLocation = taskbarOverlay.PointToScreen(new Point(e.X, e.Y));
                newLocation.Offset(-dragOffset.X, -dragOffset.Y);
                taskbarOverlay.Location = newLocation;
            }
        }

        private void TaskbarOverlay_MouseUp(object? sender, MouseEventArgs e)
        {
            isDragging = false;
            EnsureOverlayTopMost();
        }

        private void PositionTaskbarOverlay()
        {
            if (taskbarOverlay == null) return;

            // Get the working area (screen minus taskbar)
            var primary = Screen.PrimaryScreen ?? Screen.AllScreens.FirstOrDefault() ?? Screen.FromPoint(Cursor.Position);
            Rectangle workingArea = primary.WorkingArea;
            Rectangle screenBounds = primary.Bounds;

            // Determine taskbar position
            int taskbarHeight = screenBounds.Height - workingArea.Height;
            int taskbarWidth = screenBounds.Width - workingArea.Width;

            // Position based on taskbar location
            if (workingArea.Bottom < screenBounds.Bottom) // Taskbar at bottom
            {
                taskbarOverlay.Left = screenBounds.Right - taskbarOverlay.Width - 350; // 350px from right edge (space for tray)
                taskbarOverlay.Top = screenBounds.Bottom - taskbarHeight + 3;
            }
            else if (workingArea.Right < screenBounds.Right) // Taskbar on right
            {
                taskbarOverlay.Left = screenBounds.Right - taskbarWidth - taskbarOverlay.Width - 5;
                taskbarOverlay.Top = screenBounds.Bottom - taskbarOverlay.Height - 50;
            }
            else if (workingArea.Top > screenBounds.Top) // Taskbar on top
            {
                taskbarOverlay.Left = screenBounds.Right - taskbarOverlay.Width - 350;
                taskbarOverlay.Top = screenBounds.Top + 3;
            }
            else // Taskbar on left
            {
                taskbarOverlay.Left = workingArea.Left + 5;
                taskbarOverlay.Top = screenBounds.Bottom - taskbarOverlay.Height - 50;
            }
        }

        private void HideTaskbarOverlay()
        {
            if (taskbarOverlay != null)
            {
                taskbarOverlay.Close();
                taskbarOverlay.Dispose();
                taskbarOverlay = null;
                taskbarDownloadLabel = null;
                taskbarUploadLabel = null;
            }
        }

        private Icon CreateTrayIcon(string downloadSpeed, string uploadSpeed)
        {
            // Create a 16x16 bitmap for the tray icon
            Bitmap bmp = new Bitmap(16, 16);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;

                // Draw background
                using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(220, 30, 30, 30)))
                {
                    g.FillRectangle(bgBrush, 0, 0, 16, 16);
                }

                // Draw download speed (top half) - green
                using (Font font = new Font("Arial", 6, FontStyle.Bold))
                using (SolidBrush downBrush = new SolidBrush(Color.FromArgb(0, 255, 100)))
                {
                    StringFormat sf = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Near
                    };
                    g.DrawString(downloadSpeed, font, downBrush, new RectangleF(0, 0, 16, 8), sf);
                }

                // Draw upload speed (bottom half) - orange
                using (Font font = new Font("Arial", 6, FontStyle.Bold))
                using (SolidBrush upBrush = new SolidBrush(Color.FromArgb(255, 180, 0)))
                {
                    StringFormat sf = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Far
                    };
                    g.DrawString(uploadSpeed, font, upBrush, new RectangleF(0, 8, 16, 8), sf);
                }
            }
            IntPtr hIcon = bmp.GetHicon();
            // Clone to a managed icon, then destroy the original handle to avoid GDI leaks
            Icon managed;
            using (var tmp = Icon.FromHandle(hIcon))
            {
                managed = (Icon)tmp.Clone();
            }
            DestroyIcon(hIcon);
            bmp.Dispose();
            return managed;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool DestroyIcon(IntPtr handle);

        private string FormatSpeedForIcon(long bytesPerSecond)
        {
            if (bytesPerSecond < 0)
                bytesPerSecond = 0;

            if (bytesPerSecond < 1024)
                return $"{bytesPerSecond}B";
            else if (bytesPerSecond < 1024 * 1024)
                return $"{bytesPerSecond / 1024}K";
            else if (bytesPerSecond < 1024 * 1024 * 1024)
                return $"{bytesPerSecond / (1024 * 1024)}M";
            else
                return $"{bytesPerSecond / (1024 * 1024 * 1024)}G";
        }

        private void LoadNetworkInterfaces()
        {
            networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            cmbInterfaces.Items.Clear();

            foreach (var netInterface in networkInterfaces)
            {
                if (netInterface.OperationalStatus == OperationalStatus.Up &&
                    (netInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet ||
                     netInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ||
                     netInterface.NetworkInterfaceType == NetworkInterfaceType.GigabitEthernet))
                {
                    cmbInterfaces.Items.Add(netInterface.Name);
                }
            }

            if (cmbInterfaces.Items.Count > 0)
            {
                cmbInterfaces.SelectedIndex = 0;
            }
        }

        private void CmbInterfaces_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cmbInterfaces.SelectedIndex >= 0)
            {
                string selectedName = cmbInterfaces.SelectedItem?.ToString() ?? "";
                foreach (var netInterface in networkInterfaces)
                {
                    if (netInterface.Name == selectedName)
                    {
                        selectedInterface = netInterface;
                        ResetCounters();
                        break;
                    }
                }
            }
        }

        private void ResetCounters()
        {
            if (selectedInterface != null)
            {
                statsTracker = new NetworkStatsTracker(selectedInterface);
                lastUpdate = DateTime.Now;
            }
        }

    private void UpdateTimer_Tick(object? sender, EventArgs e)
        {
            if (selectedInterface == null || statsTracker == null)
                return;

            try
            {
                // Update stats
                if (!statsTracker.TryUpdate(out var res))
                {
                    lblDownload.Text = "0.00 KB/s";
                    lblUpload.Text = "0.00 KB/s";
                    return;
                }
                string formattedDownload = Formatting.Speed(res.DownloadBps);
                string formattedUpload = Formatting.Speed(res.UploadBps);

                lblDownload.Text = formattedDownload;
                lblUpload.Text = formattedUpload;
                lblTotalDownload.Text = Formatting.Bytes(res.TotalReceived);
                lblTotalUpload.Text = Formatting.Bytes(res.TotalSent);

                // Update system tray icon
                currentDownloadSpeed = Formatting.Compact(res.DownloadBps);
                currentUploadSpeed = Formatting.Compact(res.UploadBps);

                notifyIcon.Icon = CreateTrayIcon(currentDownloadSpeed, currentUploadSpeed);
                notifyIcon.Text = $"Network Monitor\n\u2193 {formattedDownload}\n\u2191 {formattedUpload}";

                // Update taskbar overlay
                if (taskbarOverlay != null && taskbarOverlay.Visible && taskbarDownloadLabel != null && taskbarUploadLabel != null)
                {
                    taskbarDownloadLabel.Text = formattedDownload;
                    taskbarUploadLabel.Text = formattedUpload;
                    taskbarDownloadLabel.ForeColor = settings.DownloadColor;
                    taskbarUploadLabel.ForeColor = settings.UploadColor;
                    taskbarOverlay.BackColor = settings.OverlayBackgroundColor;
                    EnsureOverlayTopMost();
                }
            }
            catch (Exception ex)
            {
                lblDownload.Text = "Error";
                lblUpload.Text = "Error";
                MessageBox.Show($"Error updating network stats: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EnsureOverlayTopMost()
        {
            if (taskbarOverlay == null || taskbarOverlay.IsDisposed) return;
            const uint SWP_NOSIZE = 0x0001;
            const uint SWP_NOMOVE = 0x0002;
            const uint SWP_NOACTIVATE = 0x0010;
            SetWindowPos(taskbarOverlay.Handle, HWND_TOPMOST, 0, 0, 0, 0,
                SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
        }

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                updateTimer?.Stop();
                updateTimer?.Dispose();
                if (notifyIcon != null)
                {
                    notifyIcon.Icon?.Dispose();
                    notifyIcon.Dispose();
                }
                contextMenu?.Dispose();
                HideTaskbarOverlay();
            }
            base.Dispose(disposing);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.WindowState = FormWindowState.Minimized;
            }
            base.OnFormClosing(e);
        }
    }
}
