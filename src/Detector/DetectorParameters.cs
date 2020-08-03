/***************************************************************************
 *                                                                         *
 *                                                                         *
 * Copyright(c) 2017-2020, REGATA Experiment at FLNP|JINR                  *
 * Author: [Boris Rumyantsev](mailto:bdrum@jinr.ru)                        *
 * All rights reserved                                                     *
 *                                                                         *
 *                                                                         *
 ***************************************************************************/

// Contains methods for getting and setting any parameters by special code.
// See codes here CanberraDeviceAccessLib.ParamCodes. 
// Also some of important parameters wrapped into properties
// Detector class divided by few files:

// ├── DetectorAcquisition.cs      --> Contains methods that allow to manage of spectra acquisition process. 
// |                                    Start, stop, pause, clear acquisition process and also specify acquisition mode.
// ├── DetectorCalibration.cs      --> Contains methods for loading calibration files by energy and height
// ├── DetectorConnection.cs       --> Contains methods for connection, disconnection to the device. Reset connection and so on.
// ├── DetectorFileInteractions.cs --> The code in this file determines interaction with acquiring spectra. 
// |                                    E.g. filling information about sample. Save file. 
// ├── DetectorInitialization.cs   --> Contains constructor of type, destructor and additional parameters. Like Status enumeration
// |                                    Events arguments and so on
// ├── DetectorParameters.cs       --> opened
// ├── DetectorProperties.cs       --> Contains description of basics properties, events, enumerations and additional classes
// └── IDetector.cs                --> Interface of the Detector type

using System;
using CanberraDeviceAccessLib;
using Regata.Measurements.Managers;

namespace Regata.Measurements.Devices
{
  public partial class Detector : IDisposable
  {
    public string GetParameterValue(ParamCodes parCode)
    {
      try
      {
        return _device.Param[parCode].ToString();
      }
      catch (Exception e)
      {
        NotificationManager.Notify(new Exception($"A problem with getting information from device. {parCode} doesn't exist.{Environment.NewLine}{e.Message}"), NotificationLevel.Warning, AppManager.Sender);
        return string.Empty;
      }
    }

    public void SetParameterValue<T>(ParamCodes parCode, T val)
    {
      try
      {
        if (val == null)
          throw new ArgumentNullException($"{parCode} can't be a null");

        _device.Param[parCode] = val;
        _device.Save("", true);
      }
      catch (ArgumentNullException ae)
      {
        NotificationManager.Notify(ae, NotificationLevel.Warning, AppManager.Sender);
      }
      catch (Exception e)
      {
        NotificationManager.Notify(new Exception($"A problem with saving information to file. {parCode} can't has a value {val}"), NotificationLevel.Warning, AppManager.Sender);
      }
    }

    public bool IsHV => _device.HighVoltage.On;
    public int PresetRealTime => int.Parse(GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_X_PREAL));
    public int PresetLiveTime => int.Parse(GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_X_PLIVE));
    public decimal ElapsedRealTime => decimal.Parse(GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_X_EREAL));
    public decimal ElapsedLiveTime => decimal.Parse(GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_X_ELIVE));

    public decimal DeadTime
    {
      get
      {
        if (ElapsedRealTime == 0)
          return 0;
        else
          return Math.Round(100 * (1 - ElapsedLiveTime / ElapsedRealTime), 2);
      }
    }
  }
}

