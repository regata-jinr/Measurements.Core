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
using System.Text.Json;
using Regata.Measurements.Devices;
using Regata.Measurements.Models;
using Regata.Measurements.Managers;
using System.IO;

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
    // TODO: Change to SqlConnection.State
    /// <summary>
    /// Checks if connection to db is available.
    /// In case of success save current measurement on detector to db, in the other case use
    /// serialization for save CurrentMeasurement from detector to the local storage. By default path is 'D:\LocalData'
    /// </summary>
    /// <param name="det"></param>
    public void SaveMeasurement(ref Detector det)
    {
      // if (SessionControllerSingleton.TestDBConnection())
      //   SaveRemotely(det);
      // else
      //   SaveLocally(ref det);
    }

    /// <summary>
    /// Saves information about current measurement to the data base. <seealso cref="MeasurementInfo"/>
    /// </summary>
    /// <paramref name="det">Reference to the instance of detector class</>
    private void SaveRemotely(Detector det)
    {
      if (det.CurrentMeasurement == null || det == null) return;
      try
      {
        if (AppManager.DbContext.Measurements.Where(m => m.Id == det.CurrentMeasurement.Id && string.IsNullOrEmpty(m.FileSpectra)).Any())
        {
          _nLogger.Info($"Information about measurement of current sample {det.CurrentMeasurement} from detector '{det.Name}' will be save to the data base");
          AppManager.DbContext.Measurements.Update(det.CurrentMeasurement);
        }
        else
        {
          AppManager.DbContext.Measurements.Add(new MeasurementInfo
          {
            IrradiationId = det.CurrentMeasurement.IrradiationId,
            CountryCode = det.CurrentMeasurement.CountryCode,
            ClientNumber = det.CurrentMeasurement.ClientNumber,
            Year = det.CurrentMeasurement.Year,
            SetNumber = det.CurrentMeasurement.SetNumber,
            SetIndex = det.CurrentMeasurement.SetIndex,
            SampleNumber = det.CurrentMeasurement.SampleNumber,
            Type = det.CurrentMeasurement.Type,
            Height = det.CurrentMeasurement.Height,
            DateTimeStart = det.CurrentMeasurement.DateTimeStart,
            Duration = det.CurrentMeasurement.Duration,
            DateTimeFinish = det.CurrentMeasurement.DateTimeFinish,
            FileSpectra = det.CurrentMeasurement.FileSpectra,
            Detector = det.CurrentMeasurement.Detector,
            Assistant = det.CurrentMeasurement.Assistant,
            Note = det.CurrentMeasurement.Note
          }
          );
        }
        AppManager.DbContext.SaveChanges();
      }
      catch (Exception dbe)
      {
        NotificationManager.Notify(dbe.InnerException, NotificationLevel.Error, AppManager.Sender);
        //SaveLocally(ref det);
      }
    }

  } // public partial class Session : IDisposable
}   // namespace Regata.Measurements
