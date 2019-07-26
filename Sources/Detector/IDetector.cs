using System;

namespace Measurements.Core
{
    interface IDetector
    {
        string Name { get; set; }
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
        void Save(string fileName = "");
        void Disconnect();
        void Reset();
        void Options(CanberraDeviceAccessLib.AcquisitionModes opt, int param);
        void Start();
        void Continue();
        void Stop();
        void Clear();
        void FillInfo(ref Sample sample, string mType, string operatorName, double height);

        event EventHandler DetectorChangedStatusEvent;
        event EventHandler<DetectorEventsArgs> DetectorMessageEvent;
    }
}
