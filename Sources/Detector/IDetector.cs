using System;
using CanberraDeviceAccessLib;

namespace Measurements.Core
{
    public interface IDetector
    {
        string Name { get; }
        string FullFileSpectraName { get; }
        decimal DeadTime { get; }
        int CountToRealTime { get; }
        int CountToLiveTime { get; }
        void ConnectAsync();
        void Connect();
        DetectorStatus Status { get; }
        bool IsPaused { get; }
        bool IsHV { get; }
        bool IsConnected { get; }
        string ErrorMessage { get; }
        void SetAcqureCountsAndMode(int counts, CanberraDeviceAccessLib.AcquisitionModes mode);
        void Reconnect();
        void Save(string fullFileName="");
        void Disconnect();
        void Reset();
        void Start();
        void Dispose();
        void Pause();
        void Stop();
        void Clear();
        string GetParameterValue(ParamCodes parCode);
        void SetParameterValue<T>(ParamCodes parCode, T val);
        void FillFileInfo();
        MeasurementInfo CurrentMeasurement { get; set; }
        IrradiationInfo CurrentSample { get; set; }

        event EventHandler StatusChanged;
        event EventHandler<DetectorEventsArgs> AcquiringStatusChanged;
    }
}
