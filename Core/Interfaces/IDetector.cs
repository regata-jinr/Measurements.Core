using System;
using System.Threading.Tasks;
using CanberraDeviceAccessLib;


namespace Measurements.Core.Interfaces
{
    interface IDetector
    {
        // void Connect(); is unavalible because Detector connects automatically with device which name and connecting options specify in constructor
        void Disconnect();
        void Reconnect();
        void Reset();
        bool IsConnected { get; }
        Classes.Status Status { get; }
        bool IsHV { get; }
        Task AStart();
        void AStop();
        void AClear();
        string ErrorMessage { get; }


        //Dispose()

        event EventHandler StatusChanged;
        event EventHandler ACompleted;
        event EventHandler AStoped;
        event EventHandler AGotError;
        event EventHandler HVOff;

    }
}
