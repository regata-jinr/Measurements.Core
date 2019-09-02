using System;
using CanberraDeviceAccessLib;

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
        DetectorStatus Status { get; }
        bool IsPaused { get; }
        bool IsHV { get; }
        bool IsConnected { get; }
        string ErrorMessage { get; }
        void Options(CanberraDeviceAccessLib.AcquisitionModes opt, int param);
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
        event EventHandler AcquiringStatusChanged;
    }
}
