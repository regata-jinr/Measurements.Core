/***************************************************************************
 *                                                                         *
 *                                                                         *
 * Copyright(c) 2020, REGATA Experiment at FLNP|JINR                       *
 * Author: [Boris Rumyantsev](mailto:bdrum@jinr.ru)                        *
 * All rights reserved                                                     *
 *                                                                         *
 *                                                                         *
 ***************************************************************************/

using System.Collections.Generic;
using CanberraDeviceAccessLib;
using System;
using Regata.Measurements.Devices;

namespace Regata.Measurements.Managers
{
  public static partial class AppManager
  {

    /// <summary>
    /// The list of MCA devices and Xemoes that available for usage. This list forms via MCA databases provided by Canberra DeviceAccessClass. Each xemo device linked with concrete detector.
    /// </summary>
    public static IReadOnlyList<string> AvailableDevices
    {
      get
      {
        var devs = new List<string>();
        try
        {
          logger.Info("Get list of available detectors");

          var _device = new DeviceAccessClass();
          var detNames = (IEnumerable<object>)_device.ListSpectroscopyDevices;
          foreach (var n in detNames)
          {
            if (Detector.IsDetectorAvailable(n.ToString()))
              devs.Add(n.ToString());
          }
          logger.Info($"{devs.Count} detectors are available for connection");
          return devs;
        }
        catch (Exception e)
        {
          NotificationManager.Notify(e, NotificationLevel.Error, Sender);
          return devs;
        }
      }
    }

  } // public static partial class AppManager
} // namespace Regata.Measurements.Managers
