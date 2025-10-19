// Author: mcagriaksoy - github.com/mcagriaksoy

namespace NetworkMonitor
{
    public static class Formatting
    {
        public static string Speed(long bytesPerSecond)
        {
            if (bytesPerSecond < 0) bytesPerSecond = 0;
            if (bytesPerSecond < 1024) return $"{bytesPerSecond} B/s";
            if (bytesPerSecond < 1024 * 1024) return $"{bytesPerSecond / 1024.0:F2} KB/s";
            if (bytesPerSecond < 1024L * 1024 * 1024) return $"{bytesPerSecond / (1024.0 * 1024.0):F2} MB/s";
            return $"{bytesPerSecond / (1024.0 * 1024.0 * 1024.0):F2} GB/s";
        }

        public static string Bytes(long bytes)
        {
            if (bytes < 0) bytes = 0;
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F2} KB";
            if (bytes < 1024L * 1024 * 1024) return $"{bytes / (1024.0 * 1024.0):F2} MB";
            return $"{bytes / (1024.0 * 1024.0 * 1024.0):F2} GB";
        }

        public static string Compact(long bytesPerSecond)
        {
            if (bytesPerSecond < 0) bytesPerSecond = 0;
            if (bytesPerSecond < 1024) return $"{bytesPerSecond}B";
            if (bytesPerSecond < 1024 * 1024) return $"{bytesPerSecond / 1024}K";
            if (bytesPerSecond < 1024L * 1024 * 1024) return $"{bytesPerSecond / (1024 * 1024)}M";
            return $"{bytesPerSecond / (1024L * 1024 * 1024)}G";
        }
    }
}
