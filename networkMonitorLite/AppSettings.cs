// Author: mcagriaksoy - github.com/mcagriaksoy

using System.Drawing;

namespace NetworkMonitor
{
    public class AppSettings
    {
        // Store colors as ARGB integers for easy JSON serialization
        public int OverlayBackgroundArgb { get; set; }
        public int DownloadColorArgb { get; set; }
        public int UploadColorArgb { get; set; }

        public string FontFamily { get; set; } = "Segoe UI";
        public float FontSize { get; set; } = 8f;
        public int FontStyleInt { get; set; } = (int)FontStyle.Bold;

        public static AppSettings Default()
        {
            return new AppSettings
            {
                OverlayBackgroundArgb = Color.FromArgb(20, 20, 20).ToArgb(),
                DownloadColorArgb = Color.FromArgb(0, 255, 100).ToArgb(),
                UploadColorArgb = Color.FromArgb(255, 180, 0).ToArgb(),
                FontFamily = "Segoe UI",
                FontSize = 8f,
                FontStyleInt = (int)FontStyle.Bold
            };
        }

        public Color OverlayBackgroundColor => Color.FromArgb(OverlayBackgroundArgb);
        public Color DownloadColor => Color.FromArgb(DownloadColorArgb);
        public Color UploadColor => Color.FromArgb(UploadColorArgb);
        public FontStyle OverlayFontStyle => (FontStyle)FontStyleInt;

        public Font CreateOverlayFont()
        {
            try
            {
                return new Font(FontFamily, FontSize, OverlayFontStyle);
            }
            catch
            {
                // Fallback if font not available
                return new Font("Segoe UI", FontSize, OverlayFontStyle);
            }
        }
    }
}
