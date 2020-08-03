/***************************************************************************
 *                                                                         *
 *                                                                         *
 * Copyright(c) 2017-2020, REGATA Experiment at FLNP|JINR                  *
 * Author: [Boris Rumyantsev](mailto:bdrum@jinr.ru)                        *
 * All rights reserved                                                     *
 *                                                                         *
 *                                                                         *
 ***************************************************************************/

// Contains methods for loading calibration files by energy and height
// Detector class divided by few files:

// ├── DetectorAcquisition.cs      --> Contains methods that allow to manage of spectra acquisition process. 
// |                                    Start, stop, pause, clear acquisition process and also specify acquisition mode.
// ├── DetectorCalibration.cs      --> opened
// ├── DetectorConnection.cs       --> Contains methods for connection, disconnection to the device. Reset connection and so on.
// ├── DetectorFileInteractions.cs --> The code in this file determines interaction with acquiring spectra. 
// |                                    E.g. filling information about sample. Save file.
// ├── DetectorInitialization.cs   --> Contains constructor of type, destructor and additional parameters. Like Status enumeration
// |                                    Events arguments and so on
// ├── DetectorParameters.cs       --> Contains methods for getting and setting any parameters by special code.
// |                                    See codes here CanberraDeviceAccessLib.ParamCodes. 
// |                                    Also some of important parameters wrapped into properties
// ├── DetectorProperties.cs       --> Contains description of basics properties, events, enumerations and additional classes
// └── IDetector.cs                --> Interface of the Detector type

using System;
using System.IO;
using Regata.Measurements.Managers;

namespace Regata.Measurements.Devices
{
  public partial class Detector : IDisposable
  {
    public void AddEfficiencyCalibrationFileByHeight(decimal height)
    {
      try
      {
        // fixme: here some problem with matching decimal values with combobox selected value
        //        conflict is possible in such manner: 20.0 == 20
        if (height == 20) height = 20m;
        if (height == 10) height = 10m;
        if (height == 5) height = 5m;
        if (height == 2.5m) height = 2.5m;

        string effFileName = $"C:\\GENIE2K\\CALFILES\\Efficiency\\{Name}\\{Name}-eff-{height.ToString().Replace('.', ',')}.CAL";

        if (!File.Exists(effFileName))
          throw new FileNotFoundException($"Efficiency file {effFileName} not found!");


        _nLogger.Info($"Efficiency file {effFileName} will add to the detector");
        var effFile = new CanberraDataAccessLib.DataAccess();
        effFile.Open(effFileName);
        effFile.CopyBlock(_device, CanberraDataAccessLib.ClassCodes.CAM_CLS_GEOM);
        effFile.Close();
        _device.Save("", true);
      }
      catch (FileNotFoundException fnfe)
      {
        NotificationManager.Notify(fnfe, NotificationLevel.Warning, AppManager.Sender);
      }
      catch (Exception e)
      {
        NotificationManager.Notify(e, NotificationLevel.Error, AppManager.Sender);
      }
    }


    public void AddEfficiencyCalibrationFileByEnergy()
    {
      try
      {
        string effFileName = $"C:\\GENIE2K\\CALFILES\\Efficiency\\{Name}\\{Name}-energy.CAL";

        if (!File.Exists(effFileName))
          throw new FileNotFoundException($"Efficiency file {effFileName} not found!");

        _nLogger.Info($"Efficiency file {effFileName} will add to the detector");
        var effFile = new CanberraDataAccessLib.DataAccess();
        effFile.Open(effFileName);
        effFile.CopyBlock(_device, CanberraDataAccessLib.ClassCodes.CAM_CLS_SHAPECALRES);
        effFile.CopyBlock(_device, CanberraDataAccessLib.ClassCodes.CAM_CLS_CALRESULTS);
        effFile.Close();
        _device.Save("", true);
      }
      catch (FileNotFoundException fnfe)
      {
        NotificationManager.Notify(fnfe, NotificationLevel.Warning, AppManager.Sender);
      }
      catch (Exception e)
      {
        NotificationManager.Notify(e, NotificationLevel.Error, AppManager.Sender);
      }
    }

  }
}

