using System;
using System.Threading.Tasks;

namespace Measurements.Core.Interfaces
{
    interface IMeasurement
    {
        Task Start();
        void Restart();
        void Pause();
        void Continue();
        void Stop();
        void Clear();
        Task Save();

        //ToString
        //Dispose

        event EventHandler Completed;
        event EventHandler Paused;
        event EventHandler Stoped;
        event EventHandler ErrorOccured;
    }
}
