/***************************************************************************
 *                                                                         *
 *                                                                         *
 * Copyright(c) 2017-2020, REGATA Experiment at FLNP|JINR                  *
 * Author: [Boris Rumyantsev](mailto:bdrum@jinr.ru)                        *
 * All rights reserved                                                     *
 *                                                                         *
 *                                                                         *
 ***************************************************************************/

// Contains description of basics properties, events, enumerations and additional classes
// Detector class divided by few files:

// ├── DetectorAcquisition.cs      --> Contains methods that allow to manage of spectra acquisition process. 
// |                                    Start, stop, pause, clear acquisition process and also specify acquisition mode.
// ├── DetectorCalibration.cs      --> Contains methods for loading calibration files by energy and height
// ├── DetectorConnection.cs       --> Contains methods for connection, disconnection to the device. Reset connection and so on.
// ├── DetectorFileInteractions.cs --> The code in this file determines interaction with acquiring spectra. 
// |                                    E.g. filling information about sample. Save file. 
// ├── DetectorInitialization.cs   --> Contains constructor of type, destructor and additional parameters. Like Status enumeration
// |                                    Events arguments and so on
// ├── DetectorParameters.cs       --> Contains methods for getting and setting any parameters by special code.
// |                                    See codes here CanberraDeviceAccessLib.ParamCodes. 
// |                                    Also some of important parameters wrapped into properties
// ├── DetectorProperties.cs       --> opened
// └── IDetector.cs                --> Interface of the Detector type


using System;
using System.Linq;
using System.Collections.Generic;
using CanberraDeviceAccessLib;
using Regata.Measurements.Managers;
using Regata.Measurements.Models;

namespace Regata.Measurements.Devices
{
  /// <summary>
  ///  Enumeration of possible detector's working statuses
  ///  ready - Detector is enabled and ready for acquiring
  ///  off   - Detector is disabled
  ///  busy  - Detector is acquiring spectrum
  ///  error - Detector has porblems
  /// </summary>
  public enum DetectorStatus { off, ready, busy, error }

  public partial class Detector : IDisposable
  {
    private readonly DeviceAccessClass _device;
    private readonly string _name;
    private readonly ConnectOptions _conOption;
    private readonly NLog.Logger _nLogger;
    private int _timeOutLimitSeconds;
    private bool _isDisposed;
    private DetectorStatus _status;

    public MeasurementInfo CurrentMeasurement { get; private set; }
    public IrradiationInfo RelatedIrradiation { get; private set; }

    public string Name { get { return _name; } }

    public event EventHandler StatusChanged;
    public string FullFileSpectraName { get; private set; }
    public event EventHandler<DetectorEventsArgs> AcquiringStatusChanged;

    private bool CheckNameOfDetector(string name)
    {
      try
      {
        var detsList = (IEnumerable<object>)_device.ListSpectroscopyDevices;
        if (detsList.Contains(name))
        {
          _nLogger.Info($"Detector with name '{name}' was found in the MID wizard list and will be used");
          return true;
        }
        else
        {
          Status = DetectorStatus.error;
          ErrorMessage = $"Detector with name '{name}' wasn't find in the MID wizard list. Status will change to 'error'";
          NotificationManager.Notify(new ArgumentException(ErrorMessage), NotificationLevel.Error, AppManager.Sender);
          return false;
        }
      }
      catch (Exception e)
      {
        NotificationManager.Notify(e, NotificationLevel.Error, AppManager.Sender);
        return false;
      }
    }

    /// <summary> Returns status of detector. {ready, off, busy, error}. </summary>
    /// <seealso cref="Enum Status"/>
    public DetectorStatus Status
    {
      get { return _status; }

      private set
      {
        if (_status != value)
        {
          _nLogger.Info($"The detector status changed from {_status} to {value}");
          _status = value;
          StatusChanged?.Invoke(this, EventArgs.Empty);
        }
      }
    }

    /// <summary>
    /// Returns error message.
    /// </summary>
    public string ErrorMessage { get; private set; }

  } //  public partial class Detector : IDisposable

  /// <summary>
  /// This class shared information about events occured on the detector between callers.
  /// </summary>
  public class DetectorEventsArgs : EventArgs
  {
    public string Name;
    public DetectorStatus Status;
    public int AcquireMessageParam;
    public string Message;
  }

} // namespace Regata.Measurements.Devices
