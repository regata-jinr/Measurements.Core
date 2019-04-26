namespace MeasurementsCore
{
    interface IDetector
    {
        string Name { get; set; }
        int CountToRealTime { get; set; }
        int CountToLiveTime { get; set; }
        int CountNormal { get; set; }
        void Connect();
        DetectorStatus DetStatus { get; }
        bool IsHV { get; }
        bool IsConnected { get; }
        string ErrorMessage { get; }
        void Reconnect();
        void Save(string fileName = "");
        void Disconnect();
        void Reset();
        void AOptions(CanberraDeviceAccessLib.AcquisitionModes opt, int param);
        void AStart();
        void AContinue();
        void AStop();
        void AClear();
        void FillInfo(ref Sample sample, string mType, string operatorName, float height);
    }
}
