using System;
using CanberraDeviceAccessLib;

namespace Measurements.Core
{
    public interface IDetector
    {
        string Name { get; }
        string FullFileSpectraName { get; }
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
        void Save();
        void Disconnect();
        void Reset();
        void Start();
        void Dispose();
        void Pause();
        void Stop();
        void Clear();
        string GetParameterValue(ParamCodes parCode);
        MeasurementInfo CurrentMeasurement { get; }
        IrradiationInfo CurrentSample { get; set; }

        event EventHandler StatusChanged;
        event EventHandler<DetectorEventsArgs> AcquiringStatusChanged;
    }
}
