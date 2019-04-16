namespace MeasurementsCore
{
    interface IMeasurement
    {
        void StartAsync();
        void Continue();
        void Stop();
        void Clear();
        void ShowMvcg();
        //void Restore();
        //void Backup();
        //void SaveToDB();

        //ToString
        //Dispose

    }
}
