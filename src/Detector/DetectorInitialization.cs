/***************************************************************************
 *                                                                         *
 *                                                                         *
 * Copyright(c) 2017-2020, REGATA Experiment at FLNP|JINR                  *
 * Author: [Boris Rumyantsev](mailto:bdrum@jinr.ru)                        *
 * All rights reserved                                                     *
 *                                                                         *
 *                                                                         *
 ***************************************************************************/

// Contains constructor of type, destructor and additional parameters. Like Status enumeration
// Events arguments and so on
// Detector class divided by few files:

// ├── DetectorAcquisition.cs      --> Contains methods that allow to control spectra acquisition 
// |                                   process.Start, stop, pause, clear acquisition process and
// |                                   also specify acquisition mode.
// ├── DetectorCalibration.cs      --> Contains methods for loading calibration files by energy and /// |                                   height
// ├── DetectorConnection.cs       --> Contains methods for connection, disconnection to the device. // |                                   Reset connection and so on.
// ├── DetectorFileInteractions.cs --> The code in this file determines interaction with acquiring //// |                                   spectra. 
// |                                    E.g. filling information about sample. Save file.
// ├── DetectorInitialization.cs   --> opened
// ├── DetectorParameters.cs       --> Contains methods for getting and setting any parameters by   /// |                                   special code.
// |                                    See codes here CanberraDeviceAccessLib.ParamCodes. 
// |                                    Also some of important parameters wrapped into properties
// ├── DetectorProperties.cs       --> Contains description of basics properties, events, 
// └──                                 enumerations and additional classes

using System;
using Regata.Measurements.Managers;
using Regata.Measurements.Models;
using CanberraDeviceAccessLib;

namespace Regata.Measurements.Devices
{
  /// <summary>
  /// Detector is one of the main class, because detector is the main part of our experiment. It allows to manage real detector and has protection from crashes. You can start, stop and do any basics operations which you have with detector via mvcg.exe. This software based on dlls provided by [Genie2000] (https://www.mirion.com/products/genie-2000-basic-spectroscopy-software) for interactions with [HPGE](https://www.mirion.com/products/standard-high-purity-germanium-detectors) detectors also from [Mirion Tech.](https://www.mirion.com). Personally we are working with [Standard Electrode Coaxial Ge Detectors](https://www.mirion.com/products/sege-standard-electrode-coaxial-ge-detectors)
  /// </summary>
  /// <seealso cref="https://www.mirion.com/products/genie-2000-basic-spectroscopy-software"/>
  public partial class Detector : IDisposable
  {
    /// <summary>Constructor of Detector class.</summary>
    /// <param name="name">Name of detector. Without path.</param>
    /// <param name="option">CanberraDeviceAccessLib.ConnectOptions {aReadWrite, aContinue, aNoVerifyLicense, aReadOnly, aTakeControl, aTakeOver}.By default ConnectOptions is ReadWrite.</param>
    public Detector(string name)
    {
      try
      {
        _nLogger = AppManager.logger.WithProperty("Sender", $"{typeof(Detector).Name} {name}");
        _nLogger.Info($"Initialization of the detector '{name}' with mode {ConnectOptions.aReadWrite} and timeout limit {10} seconds");

        _conOption = ConnectOptions.aReadWrite;
        _isDisposed = false;
        Status = DetectorStatus.off;
        ErrorMessage = "";
        _device = new DeviceAccessClass();
        CurrentMeasurement = new MeasurementInfo();

        if (CheckNameOfDetector(name))
          _name = name;
        else
          throw new Exception($"Detector with name '{name}' doesn't exist in MID Data base");

        _device.DeviceMessages += DeviceMessagesHandler;
        _timeOutLimitSeconds = 10;
        IsPaused = false;

        Connect();

        AcquisitionMode = AcquisitionModes.aCountToRealTime;
      }
      catch (Exception e)
      {
        NotificationManager.Notify(e, NotificationLevel.Error, AppManager.Sender);
      }
    }

    private void CleanUp(bool isDisposing)
    {
      _nLogger.Info($"Cleaning of the detector {Name}");

      if (!_isDisposed)
      {
        if (isDisposing)
        {
          NLog.LogManager.Flush();
        }
        Disconnect();
      }
      _isDisposed = true;
    }

    ~Detector()
    {
      CleanUp(false);
    }

    /// <summary>
    /// Disconnects from detector. Changes status to off. Resets ErrorMessage. Clears the detector.
    /// </summary>
    public void Dispose()
    {
      CleanUp(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Power reset for the detector. 
    /// </summary>
    public void Reset()
    {
      // FIXME: not tested
      try
      {
        _nLogger.Info($"Attempt to reset the detector");
        _device.SendCommand(DeviceCommands.aReset);

        if (Status == DetectorStatus.ready)
          _nLogger.Info($"Resetting was successful");
        else
          throw new Exception($"Something were wrong during reseting of the detector '{Name}'");
      }
      catch (Exception e)
      {
        NotificationManager.Notify(e, NotificationLevel.Error, AppManager.Sender);
      }
    }

  } //  public partial class Detector : IDisposable
} // namespace Regata.Measurements.Devices
