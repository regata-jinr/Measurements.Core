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
    public static Dictionary<string, Detector> AvailableDevices { get; private set; }

    /// <summary>
    /// This internal method add detector to the list of available detectors for usage
    /// with all checkings
    /// </summary>
    /// <param name="name">Name of the detector. You can see detectors name in MCA Input Defenition Editor</param>
    private static void AddDevicesToList(string name)
    {
      Detector d = null;
      try
      {
        d = new Detector(name);
        if (d.IsConnected)
        {
          AvailableDevices.Add(d.Name, d);
          // TODO: perhaps such usage will not alow to load detector by hand
          d.Disconnect();
          // TODO: also check xemo connection here
        }

      }
      catch (Exception e)
      {
        // split detector and xemo exception
        NotificationManager.Notify(e, NotificationLevel.Warning, Sender);
        d?.Dispose();
      }
    }

    /// <summary>
    /// Add available detectors to list using internal MCA database
    /// </summary>
    public static void GetDevicesList()
    {
      try
      {
        logger.Info("Initialization of detectors:");

        var _device = new DeviceAccessClass();
        var detNames = (IEnumerable<object>)_device.ListSpectroscopyDevices;

        foreach (var n in detNames)
          AddDevicesToList(n.ToString());

      }
      catch (Exception e)
      {
        NotificationManager.Notify(e, NotificationLevel.Error, Sender);
      }
    }

  } // public static partial class AppManager
} // namespace Regata.Measurements.Managers
