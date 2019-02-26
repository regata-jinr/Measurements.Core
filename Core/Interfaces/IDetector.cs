using System;

namespace Measurements.Core.Interfaces
{
    public delegate void ChangedStatusDelegate();
    public delegate void AcquiringCompletedDelegate();
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


        event ChangedStatusDelegate ChangedStatusEvent;
        event AcquiringCompletedDelegate AcquiringCompletedEvent;
        event EventHandler AStoped;
        event EventHandler AErrorOccured;
        event EventHandler ErrorOccured;
        event EventHandler HVOff;

    }
}
