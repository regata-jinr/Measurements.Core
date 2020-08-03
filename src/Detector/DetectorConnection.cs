/***************************************************************************
 *                                                                         *
 *                                                                         *
 * Copyright(c) 2017-2020, REGATA Experiment at FLNP|JINR                  *
 * Author: [Boris Rumyantsev](mailto:bdrum@jinr.ru)                        *
 * All rights reserved                                                     *
 *                                                                         *
 *                                                                         *
 ***************************************************************************/

// Contains methods for connection, disconnection to the device. Reset connection and so on.
// Detector class divided by few files:

// ├── DetectorAcquisition.cs      --> Contains methods that allow to manage of spectra acquisition process 
// |                                    Start, stop, pause, clear acquisition process and also specify acquisition mode
// ├── DetectorCalibration.cs      --> Contains methods for loading calibration files by energy and height
// ├── DetectorConnection.cs       --> opened
// ├── DetectorFileInteractions.cs --> The code in this file determines interaction with acquiring spectra. 
// |                                    E.g. filling information about sample. Save file
// ├── DetectorInitialization.cs   --> Contains constructor of type, destructor and additional parameters. Like Status enumeration
// |                                    Events arguments and so on
// ├── DetectorParameters.cs       --> Contains methods for getting and setting any parameters by special code
// |                                    See codes here CanberraDeviceAccessLib.ParamCodes 
// |                                    Also some of important parameters wrapped into properties
// ├── DetectorProperties.cs       --> Contains description of basics properties, events, enumerations and additional classes
// └── IDetector.cs                --> Interface of the Detector type

using System;
using System.Threading.Tasks;
using Regata.Measurements.Managers;

namespace Regata.Measurements.Devices
{
  public partial class Detector : IDisposable
  {
    /// <summary>
    /// 
    /// </summary>
    /// TODO: find out what happened in case of long connection. How to add timeoutlimit, because device.Connect already async. 
    public void Connect()
    {
      try
      {
        _nLogger.Info($"Starts connecting to the detector");
        ConnectInternal();

        if (_device.IsConnected)
          _nLogger.Info($"Connection to the detector was successful");

      }
      catch (TimeoutException te)
      {
        NotificationManager.Notify(te, NotificationLevel.Warning, AppManager.Sender);
      }
      catch (Exception e)
      {
        NotificationManager.Notify(e, NotificationLevel.Error, AppManager.Sender);
      }
    }

    private void ConnectInternal()
    {
      try
      {
        _nLogger.Debug($"Starts internal connection to the detector {_name}");

        Status = DetectorStatus.off;
        _device.Connect(Name, _conOption);
        Status = DetectorStatus.ready;

        _nLogger.Debug($"Internal connection was successful");
      }
      catch (Exception e)
      {
        NotificationManager.Notify(e, NotificationLevel.Warning, AppManager.Sender);
        if (e.Message.Contains("278e2a")) Status = DetectorStatus.busy;
      }
    }

    /// <summary>
    /// Recconects will trying to ressurect connection with detector. In case detector has status error or ready, 
    /// it will do nothing. In case detector is off it will just call connect.
    /// In case status is busy, it will run recursively before 3 attempts with 5sec pausing.
    public async Task Reconnect()
    {
      _nLogger.Info($"Attempt to reconnect to the detector.");

      var t1 = Task.Run(() => { while (!_device.IsConnected) { Disconnect(); } });
      var t2 = Task.Delay(TimeSpan.FromSeconds(_timeOutLimitSeconds));

      var t3 = await Task.WhenAny(t1, t2);

      if (!_device.IsConnected)
        Connect();

      if (t3 == t1 && _device.IsConnected)
        _nLogger.Info($"Reconnection successful");

      if (t3 == t2)
      {
        _nLogger.Info($"Can not to disconnect from detector {_name}. Exceeded timeout limit.");
        NotificationManager.Notify(new Notification { Level = NotificationLevel.Warning, Title = "Reconnection has exceeded timeout limit" });
      }
    }

    public void Disconnect()
    {
      try
      {
        _nLogger.Info($"Disconnecting from the detector.");
        if (_device.IsConnected)
          _device.Disconnect();
        _nLogger.Info($"Disconnecting was successful.");
        Status = DetectorStatus.off;
        ErrorMessage = "";
      }
      catch (Exception e)
      {
        NotificationManager.Notify(e, NotificationLevel.Error, AppManager.Sender);
      }
    }

    public bool IsConnected => _device.IsConnected;

  } //  public partial class Detector : IDisposable
} // namespace Regata.Measurements.Devices
