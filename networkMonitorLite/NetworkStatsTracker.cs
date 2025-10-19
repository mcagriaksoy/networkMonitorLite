// Author: mcagriaksoy - github.com/mcagriaksoy

using System;
using System.Linq;
using System.Net.NetworkInformation;

namespace NetworkMonitor
{
    public class NetworkStatsTracker
    {
        private string _interfaceName;
        private NetworkInterface? _currentInterface;
        private long _prevBytesRecv;
        private long _prevBytesSent;
        private long _totalRecv;
        private long _totalSent;
        private DateTime _lastUpdate;

        public NetworkStatsTracker(NetworkInterface netIf)
        {
            _interfaceName = netIf.Name;
            _currentInterface = netIf;
            Reset();
        }

        public void SetInterface(NetworkInterface netIf)
        {
            _interfaceName = netIf.Name;
            _currentInterface = netIf;
            Reset();
        }

        public void Reset()
        {
            if (_currentInterface == null)
                return;
            var stats = _currentInterface.GetIPv4Statistics();
            _prevBytesRecv = stats.BytesReceived;
            _prevBytesSent = stats.BytesSent;
            _totalRecv = 0;
            _totalSent = 0;
            _lastUpdate = DateTime.Now;
        }

        public bool TryUpdate(out StatsResult result)
        {
            result = default;

            // Refresh interface by name each tick to ensure stats are current
            _currentInterface = NetworkInterface.GetAllNetworkInterfaces()
                .FirstOrDefault(ni => ni.Name == _interfaceName);

            if (_currentInterface == null || _currentInterface.OperationalStatus != OperationalStatus.Up)
                return false;

            var stats = _currentInterface.GetIPv4Statistics();
            DateTime now = DateTime.Now;
            double seconds = Math.Max(0.001, (now - _lastUpdate).TotalSeconds);

            long curRecv = stats.BytesReceived;
            long curSent = stats.BytesSent;

            long downBps = (long)((curRecv - _prevBytesRecv) / seconds);
            long upBps = (long)((curSent - _prevBytesSent) / seconds);

            _totalRecv += Math.Max(0, curRecv - _prevBytesRecv);
            _totalSent += Math.Max(0, curSent - _prevBytesSent);

            _prevBytesRecv = curRecv;
            _prevBytesSent = curSent;
            _lastUpdate = now;

            result = new StatsResult(downBps, upBps, _totalRecv, _totalSent);
            return true;
        }
    }

    public readonly struct StatsResult
    {
        public long DownloadBps { get; }
        public long UploadBps { get; }
        public long TotalReceived { get; }
        public long TotalSent { get; }

        public StatsResult(long downloadBps, long uploadBps, long totalReceived, long totalSent)
        {
            DownloadBps = downloadBps;
            UploadBps = uploadBps;
            TotalReceived = totalReceived;
            TotalSent = totalSent;
        }
    }
}
