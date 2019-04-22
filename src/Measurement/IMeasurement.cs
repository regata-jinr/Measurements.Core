namespace MeasurementsCore
{
    interface IMeasurement
    {
        void StartAsync();
        void Continue();
        void Stop();
        void Clear();
        void ShowDetectorInMvcg();
        void CompleteMeasurement();
        void SaveSpectraToFile();
        //void Restore();
        void Backup();
        void SaveToDB();

        //ToString
        //Dispose

    }
}
