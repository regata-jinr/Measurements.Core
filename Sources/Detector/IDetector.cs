using System;

namespace Measurements.Core
{
    public interface IDetector
    {
        string Name { get; }
        int CountToRealTime { get; set; }
        int CountToLiveTime { get; set; }
        int CountNormal { get; set; }
        void ConnectAsync();
        void Connect();
        DetectorStatus DetStatus { get; }
        bool IsHV { get; }
        bool IsConnected { get; }
        string ErrorMessage { get; }
        void Reconnect();
        void Save();
        void Disconnect();
        void Reset();
        void Options(CanberraDeviceAccessLib.AcquisitionModes opt, int param);
        void Start();
        void Continue();
        void Stop();
        void Clear();
        Measurement CurrentMeasurement { get; set; }

        event EventHandler DetectorChangedStatusEvent;
        event EventHandler<DetectorEventsArgs> DetectorMessageEvent;
    }
}
