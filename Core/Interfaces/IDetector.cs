using System;

namespace Measurements.Core.Interfaces
{
    public delegate void ChangingStatusDelegate();
    public delegate void AcquiringStatusDelegate();
    interface IDetector 
    {
        void Disconnect();
        void Reconnect();
        void Reset();
        bool IsConnected { get; }
        Classes.DetectorStatus DetStatus { get; }
        Classes.AcquringStatus AcqStatus { get; }
        bool IsHV { get; }
        void AStart(int time);
        void AStop();
        void AClear();
        string ErrorMessage { get; }
        void FillSampleInfo(Core.Classes.Sample s);
        void FillMeasurementInfo(Core.Classes.Measurement m);

        event ChangingStatusDelegate StatusChangedEvent;
        event AcquiringStatusDelegate AcquiringStatusChangedEvent;

    }
}
