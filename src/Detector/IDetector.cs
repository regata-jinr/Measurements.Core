using System;
using CanberraDeviceAccessLib;

namespace Measurements.Core
{
    public interface IDetector
    {
        string           Name                { get; }
        string           FullFileSpectraName { get; }
        decimal          DeadTime            { get; }
        int              PresetRealTime      { get; }
        int              PresetLiveTime      { get; }
        decimal          ElapsedRealTime     { get; }
        decimal          ElapsedLiveTime     { get; }
        DetectorStatus   Status              { get; }
        bool             IsPaused            { get; }
        bool             IsHV                { get; }
        bool             IsConnected         { get; }
        string           ErrorMessage        { get; }
        MeasurementInfo  CurrentMeasurement  { get; }
        IrradiationInfo  RelatedIrradiation  { get; }
        //TODO: think about count mode should also keeping in db
        AcquisitionModes AcquisitionMode     {get; set;}

        void            FillSampleInformation(MeasurementInfo measurement, IrradiationInfo irradiation);
        void            ConnectAsync();
        void            Connect();
        //void            SetAcquireCountsAndMode(int counts, CanberraDeviceAccessLib.AcquisitionModes mode);
        void            Reconnect();
        void            Save(string fullFileName="");
        void            Disconnect();
        void            Reset();
        void            Start();
        void            Dispose();
        void            Pause();
        void            Stop();
        void            Clear();
        void            AddEfficiencyCalibrationFile(decimal height);
        string          GetParameterValue(ParamCodes parCode);
        void            SetParameterValue<T>(ParamCodes parCode, T val);
     
        event EventHandler StatusChanged;
        event EventHandler<DetectorEventsArgs> AcquiringStatusChanged;
    }
}
