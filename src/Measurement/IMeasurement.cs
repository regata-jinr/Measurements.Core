using System;
using System.Threading.Tasks;
using MeasurementsCore;

namespace MeasurementsCore
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
        //void SaveToFile();
        //void SaveToDB();
        void SetInfo(Sample s, string type, string experimentator, string description);

        //ToString
        //Dispose

    }
}
