using System;
using System.Threading.Tasks;


namespace Measurements.Core.Interfaces
{
    interface IDetector
    {
        void Connect();
        void Disonnect();
        void Reconnect();
        void Reset();
        bool IsConnect();
        Core.Classes.Status GetStatus();
        Task AStart();
        void AStop();
        void AClear();
        
        //Dispose()

        event EventHandler StatusChanged;
        event EventHandler ACompleted;
        event EventHandler AStoped;
        event EventHandler AGotError;
        event EventHandler HVOff;

    }
}
