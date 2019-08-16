using System;

namespace Measurements.Core
{
    public interface IMeasurement
    {
        void Dispose();
        void SaveToDataBase();
        void SaveLocally();
        bool IsDbConnected();


    }
}
