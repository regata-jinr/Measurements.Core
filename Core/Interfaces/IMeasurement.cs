using System;
using System.Threading.Tasks;
using Measurements.Core.Classes;

namespace Measurements.Core.Interfaces
{
    interface IMeasurement
    {
        void Start(int time);
        void Restart();
        void Pause();
        void Continue();
        void Stop();
        void Clear();
        void Save();
        void SaveToFile();
        void SaveToDB();
        void SetInfo(Sample s, string type, string experimentator, string description);

        //ToString
        //Dispose

        event EventHandler Completed;
        event EventHandler Paused;
        event EventHandler Stoped;
        event EventHandler ErrorOccured;
    }
}
