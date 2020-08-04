/***************************************************************************
 *                                                                         *
 *                                                                         *
 * Copyright(c) 2017-2020, REGATA Experiment at FLNP|JINR                  *
 * Author: [Boris Rumyantsev](mailto:bdrum@jinr.ru)                        *
 * All rights reserved                                                     *
 *                                                                         *
 *                                                                         *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using Regata.Measurements.Managers;
using Regata.Measurements.Models;
using Regata.Measurements.Devices;


namespace Regata.Measurements
{
  // this file contains general fields and methods of the Session class.
  // Session class divided by few files:
  // ├── ISession.cs                - interface of Session class
  // ├── SessionDetectorsControl.cs - contains methods that related with managing of detector
  // ├── SessionInfo.cs             - contains fields of Session for EF core interaction
  // └── SessionMain.cs --> opened

  public partial class Session : IDisposable
  {

    /// <summary>
    /// This event will occur after all detectors complete measurements of all samples
    /// </summary>
    // public event Action SessionComplete;
    // public event Action<MeasurementInfo> MeasurementOfSampleDone;

    /// <summary>
    /// This event will occur after one of managed detector by the session complete measurements of all samples
    /// </summary>
    // public event Action<string> MeasurementDone;

    /// <summary>
    /// Sets duration of measurement and the mode of acqusition for each detector controlled by the session. <seealso cref="CanberraDeviceAccessLib.AcquisitionModes"/>
    /// </summary>
    /// <param name="duration">Characterize number of counts for certain mode of acquisition</param>
    /// <param name="acqm">Characterize mode of spectra acquisition. By default is aCountToRealTime</param>
    //private void SetAcquireDurationAndMode(int duration, CanberraDeviceAccessLib.AcquisitionModes acqm)
    //{
    //    Counts = duration;
    //    foreach (var d in ManagedDetectors)
    //        d.SetAcqureCountsAndMode(Counts, CountMode);
    //}

    /// <summary>
    /// Allows user to get chosen acqusition mode specified via <seealso cref="SetAcquireDurationAndMode(int, CanberraDeviceAccessLib.AcquisitionModes)"/>
    /// </summary>
    private CanberraDeviceAccessLib.AcquisitionModes _countMode;
    public CanberraDeviceAccessLib.AcquisitionModes CountMode
    {
      get { return _countMode; }
      set
      {
        var AvailableAcquisitionModes = new CanberraDeviceAccessLib.AcquisitionModes[]
                                            {
                                                        CanberraDeviceAccessLib.AcquisitionModes.aCountToRealTime,
                                                        CanberraDeviceAccessLib.AcquisitionModes.aCountToLiveTime
                                            };

        if (!AvailableAcquisitionModes.Contains(value))
        {
          _nLogger.Info($"Acquisition mode could be chosen only from this modes: {string.Join(", ", AvailableAcquisitionModes)}. aCountToRealTime will be set");
          return;
        }
        _nLogger.Info($"Acquisition mode of measurements is set to {value}");
        _countMode = value;
        foreach (var d in ManagedDetectors)
          d.AcquisitionMode = value;
      }
    }

    /// <summary>
    /// List of detectors that controlled by the session
    /// </summary>
    public List<Detector> ManagedDetectors { get; private set; }

  }   // public partial class Session : IDisposable
}     // namespace Regata.Measurements
