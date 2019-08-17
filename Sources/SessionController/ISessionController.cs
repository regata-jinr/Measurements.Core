using System;
using System.Collections.Generic;
using CanberraDeviceAccessLib;


namespace Measurements.Core
{
    public interface ISessionController
    {
        ISession Create(string sName);
        ISession Load(string sName);
        string Assistant { get; }
        List<IDetector> AvailableDetectors { get;  }
        List<IDetector> AvailableIrradiationJournals { get; }


    }
}
