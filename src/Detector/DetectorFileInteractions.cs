/***************************************************************************
 *                                                                         *
 *                                                                         *
 * Copyright(c) 2017-2020, REGATA Experiment at FLNP|JINR                  *
 * Author: [Boris Rumyantsev](mailto:bdrum@jinr.ru)                        *
 * All rights reserved                                                     *
 *                                                                         *
 *                                                                         *
 ***************************************************************************/

// The code in this file determines interaction with acquiring spectra file. 
// E.g. filling information about sample. Save file.
// Detector class divided by few files:

// ├── DetectorAcquisition.cs      --> Contains methods that allow to manage of spectra acquisition process. 
// |                                    Start, stop, pause, clear acquisition process and also specify acquisition mode.
// ├── DetectorCalibration.cs      --> Contains methods for loading calibration files by energy and height
// ├── DetectorConnection.cs       --> Contains methods for connection, disconnection to the device. Reset connection and so on.
// ├── DetectorFileInteractions.cs --> opened
// ├── DetectorInitialization.cs   --> Contains constructor of type, destructor and additional parameters. Like Status enumeration
// |                                    Events arguments and so on
// ├── DetectorParameters.cs       --> Contains methods for getting and setting any parameters by special code.
// |                                    See codes here CanberraDeviceAccessLib.ParamCodes. 
// |                                    Also some of important parameters wrapped into properties
// ├── DetectorProperties.cs       --> Contains description of basics properties, events, enumerations and additional classes
// └── IDetector.cs                --> Interface of the Detector type

using System;
using System.IO;
using System.Linq;
using CanberraDeviceAccessLib;
using Regata.Measurements.Managers;
using Regata.Measurements.Models;

namespace Regata.Measurements.Devices
{
  public partial class Detector : IDisposable
  {
    /// <summary>
    /// Save current measurement session on the device.
    /// </summary>
    public void Save(string fullFileName = "")
    {
      _nLogger.Info($"Starts saving of current session to file");
      try
      {
        if (!_device.IsConnected || Status == DetectorStatus.off)
          throw new InvalidOperationException();

        if (string.IsNullOrEmpty(fullFileName))
        {
          var _currentDir = $"D:\\Spectra\\{DateTime.Now.Year}\\{DateTime.Now.Month.ToString("D2")}\\{CurrentMeasurement.Type.ToLower()}";
          Directory.CreateDirectory(_currentDir);
          fullFileName = $"{_currentDir}\\{CurrentMeasurement.FileSpectra}.cnf";
        }

        _device.Save($"{fullFileName}", true);
        FullFileSpectraName = fullFileName;

        if (File.Exists(FullFileSpectraName))
          _nLogger.Info($"File '{FullFileSpectraName}' was saved");
        else _nLogger.Error($"Some problems during saving. File {CurrentMeasurement.FileSpectra}.cnf doesn't exist.");
      }
      catch (ArgumentNullException ae)
      {
        NotificationManager.Notify(ae, NotificationLevel.Error, AppManager.Sender);
      }
      catch (InvalidOperationException ie)
      {
        NotificationManager.Notify(ie, NotificationLevel.Error, AppManager.Sender);
      }
      catch (Exception e)
      {
        NotificationManager.Notify(e, NotificationLevel.Error, AppManager.Sender);
      }
    }

    /// <summary>
    /// Load information about planning measurement to the device
    /// </summary>
    /// <param name="measurement"></param>
    /// <param name="irradiation"></param>        
    public void LoadMeasurementInfoToDevice(MeasurementInfo measurement, IrradiationInfo irradiation)
    {
      if (!CheckIrradiationInfo(irradiation) || !CheckMeasurementInfo(measurement))
        return;

      _nLogger.Info($"Set sample {measurement} to detector");

      try
      {
        CurrentMeasurement = measurement;
        RelatedIrradiation = irradiation;
        _device.Param[ParamCodes.CAM_T_STITLE] = measurement.SampleKey;                  // title
        _device.Param[ParamCodes.CAM_T_SCOLLNAME] = measurement.Assistant;                  // operator's name
        _device.Param[ParamCodes.CAM_T_SIDENT] = measurement.SetKey;                     // sample code
        _device.Param[ParamCodes.CAM_F_SQUANT] = (double)irradiation.Weight.Value;       // weight
        _device.Param[ParamCodes.CAM_F_SQUANTERR] = 0;                                      // err = 0
        _device.Param[ParamCodes.CAM_T_SUNITS] = "gram";                                 // units = gram
        _device.Param[ParamCodes.CAM_T_BUILDUPTYPE] = "IRRAD";
        _device.Param[ParamCodes.CAM_X_SDEPOSIT] = irradiation.DateTimeStart.Value;        // irr start date time
        _device.Param[ParamCodes.CAM_X_STIME] = irradiation.DateTimeFinish.Value;       // irr finish date time
        _device.Param[ParamCodes.CAM_F_SSYSERR] = 0;                                      // Random sample error (%)
        _device.Param[ParamCodes.CAM_F_SSYSTERR] = 0;                                      // Non-random sample error (%)
        _device.Param[ParamCodes.CAM_T_STYPE] = measurement.Type;
        _device.Param[ParamCodes.CAM_T_SGEOMTRY] = measurement.Height.Value.ToString("f"); // height

        AddEfficiencyCalibrationFileByHeight(measurement.Height.Value);
        AddEfficiencyCalibrationFileByEnergy();

        DivideString(CurrentMeasurement.Note); //filling description field in file

        Counts = measurement.Duration.Value;

        _device.Save("", true);
      }
      catch (Exception e)
      {
        NotificationManager.Notify(e, NotificationLevel.Error, AppManager.Sender);
      }
    }

    private bool CheckIrradiationInfo(IrradiationInfo irradiation)
    {
      bool isCorrect = true;
      try
      {
        if (irradiation == null)
          throw new ArgumentException("Irradiated sample has not chosen");

        var type = typeof(IrradiationInfo);
        var neededProps = new string[] { "DateTimeStart", "Duration", "Weight" };

        foreach (var pi in type.GetProperties())
        {
          if (neededProps.Contains(pi.Name) && pi.GetValue(irradiation) == null)
            throw new ArgumentException($"{pi.Name} should not be null");
        }

        if (!irradiation.DateTimeFinish.HasValue)
          irradiation.DateTimeFinish = irradiation.DateTimeStart.Value.AddSeconds(irradiation.Duration.Value);

        if (irradiation.DateTimeFinish.Value.TimeOfDay.TotalSeconds == 0)
          irradiation.DateTimeFinish = irradiation.DateTimeStart.Value.AddSeconds(irradiation.Duration.Value);
      }
      catch (ArgumentException ae)
      {
        NotificationManager.Notify(ae, NotificationLevel.Warning, AppManager.Sender);
        isCorrect = false;
      }
      catch (Exception e)
      {
        NotificationManager.Notify(e, NotificationLevel.Error, AppManager.Sender);
        isCorrect = false;
      }
      return isCorrect;
    }

    private bool CheckMeasurementInfo(MeasurementInfo measurement)
    {
      bool isCorrect = true;
      try
      {
        if (measurement == null)
          throw new ArgumentException("Sample for measurement should not be null");

        var type = typeof(MeasurementInfo);
        var neededProps = new string[] { "Type", "Duration", "Height" };

        foreach (var pi in type.GetProperties())
        {
          if (neededProps.Contains(pi.Name) && pi.GetValue(measurement) == null)
            throw new ArgumentException($"{pi.Name} should not be null");
        }

        if (string.IsNullOrEmpty(measurement.Detector))
          measurement.Detector = Name;

        if (measurement.Detector != Name)
          throw new ArgumentException("Name of detector in db doesn't correspond to current detector for the sample");

        if (measurement.Duration.Value == 0)
          throw new ArgumentException("Duration of the measurement process should not be equal to zero");

      }
      catch (ArgumentException ae)
      {
        NotificationManager.Notify(ae, NotificationLevel.Warning, AppManager.Sender);
        isCorrect = false;
      }
      catch (Exception e)
      {
        NotificationManager.Notify(e, NotificationLevel.Error, AppManager.Sender);
        isCorrect = false;
      }
      return isCorrect;
    }

    /// <summary>
    /// In spectra file we have four row for notes, Each row allows to keep 64 charatcer.
    /// This method divide a big string to these rows
    /// </summary>
    /// <param name="iStr"></param>
    private void DivideString(string iStr)
    {
      if (string.IsNullOrEmpty(iStr)) return;
      int descriptionsCount = iStr.Length / 65;

      switch (descriptionsCount)
      {
        case 0:
          _device.Param[ParamCodes.CAM_T_SDESC1] = iStr;
          break;
        case 1:
          _device.Param[ParamCodes.CAM_T_SDESC1] = iStr.Substring(0, 65);
          _device.Param[ParamCodes.CAM_T_SDESC2] = iStr.Substring(66);
          break;
        case 2:
          _device.Param[ParamCodes.CAM_T_SDESC1] = iStr.Substring(0, 65);
          _device.Param[ParamCodes.CAM_T_SDESC2] = iStr.Substring(66, 65);
          _device.Param[ParamCodes.CAM_T_SDESC3] = iStr.Substring(132);
          break;
        case 3:
          _device.Param[ParamCodes.CAM_T_SDESC1] = iStr.Substring(0, 65);
          _device.Param[ParamCodes.CAM_T_SDESC2] = iStr.Substring(66, 65);
          _device.Param[ParamCodes.CAM_T_SDESC3] = iStr.Substring(132, 65);
          _device.Param[ParamCodes.CAM_T_SDESC4] = iStr.Substring(198);
          break;
        default:
          _device.Param[ParamCodes.CAM_T_SDESC1] = iStr.Substring(0, 65);
          _device.Param[ParamCodes.CAM_T_SDESC2] = iStr.Substring(66, 65);
          _device.Param[ParamCodes.CAM_T_SDESC3] = iStr.Substring(132, 65);
          _device.Param[ParamCodes.CAM_T_SDESC4] = iStr.Substring(198, 65);
          break;
      }
    }

  }
}

