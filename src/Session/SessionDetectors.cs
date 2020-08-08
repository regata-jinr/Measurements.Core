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
using System.IO;
using Microsoft.Data.SqlClient;
using Regata.Measurements.Devices;
using Regata.Measurements.Managers;

namespace Regata.Measurements
{
    // this file contains methods that related with managing of detector
    // Session class divided by few files:
    // ├── ISession.cs                - interface of Session class
    // ├── SessionDetectorsControl.cs --> opened
    // ├── SessionInfo.cs             - contains fields of Session for EF core interaction
    // └── SessionMain.cs             - contains general fields and methods of the Session class.

    public partial class Session : IDisposable
    {

        /// <summary>
        /// Attach detector defined by the name to the session
        /// Chosen detector will remove from available detectors list that SessionControllerSingleton controled
        /// </summary>
        /// <param name="dName">Name of detector</param>
        public void AttachDetector(string dName)
        {
            _nLogger.Info($"Session takes a control for the detector '{dName}'");
            try
            {
                if (!AppManager.AvailableDevices.Contains(dName))
                    throw new InvalidOperationException("Detector doesn't available for connection");

                var det = new Detector(dName);
                if (det.IsConnected)
                {
                    ManagedDetectors[dName] = det;
                    det.AcquiringStatusChanged += ProcessAcquiringMessage;
                    _nLogger.Info($"Successfully attached detector '{det.Name}' to the Session '{Name}'");
                }
                else
                    throw new ArgumentNullException($"{dName}. The most probably you are already use this detector");
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
        /// Remove detector given by name from list of managed detectors by the session
        /// Such detector will add to the list of available detectors that controlled by SessionControllerSingleton
        /// </summary>
        /// <param name="dName">Name of detector</param>
        public void DetachDetector(string dName)
        {
            _nLogger.Info($"Session leaves a control for the detector '{dName}'");
            try
            {
                if (!ManagedDetectors.ContainsKey(dName)) return;

                ManagedDetectors[dName].Disconnect();
                ManagedDetectors[dName].AcquiringStatusChanged -= ProcessAcquiringMessage;
                ManagedDetectors.Remove(dName);
                _nLogger.Info($"Successfuly detached the detector '{dName}'");
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
        /// This is the handler of MeasurementDone events. In case of detector has done measurements process, 
        /// this method add counter <see cref="_countOfDetectorsWichDone"/> and will check it with number of managed detectors.
        /// In case of matching <see cref="SessionComplete"/> will invoke.
        /// </summary>
        /// <param name="det">Detector that generated event of measurements has done</param>
        /// <param name="eventArgs"></param>
        // private void MeasurementDoneHandler(string detName)
        // {
        //   _nLogger.Info($"Detector {detName} has done measurement process");
        //   _countOfDetectorsWichDone++;

        //   if (_countOfDetectorsWichDone == ManagedDetectors.Count)
        //   {
        //     _nLogger.Info($"All detectors [{(string.Join(",", ManagedDetectors.Select(d => d.Name).ToArray()))}] has done measurement process");
        //     _countOfDetectorsWichDone = 0;
        //     SessionComplete?.Invoke();
        //   }
        // }

        /// <summary>
        /// This internal method process message from the detector. <see cref="Detector.ProcessDeviceMessages(int, int, int)"/>
        /// </summary>
        /// <param name="o">Boxed detector</param>
        /// <param name="args"><see cref="DetectorEventsArgs"/></param>
        private void ProcessAcquiringMessage(object o, DetectorEventsArgs args)
        {
            try
            {
                var d = o as Detector;
                if (d == null) return;

                if (d.Status == DetectorStatus.ready && args.AcquireMessageParam == (int)CanberraDeviceAccessLib.AdviseMessageMasks.amAcquireDone && !d.IsPaused)
                {
                    //SaveSpectraOnDetectorToFileAndDataBase(ref d);
                    MeasurementOfSampleDone?.Invoke(d.CurrentMeasurement);
                }
            }
            catch (ArgumentException ae)
            {
                NotificationManager.Notify(ae, NotificationLevel.Error, AppManager.Sender);
            }
            catch (Exception e)
            {
                NotificationManager.Notify(e, NotificationLevel.Error, AppManager.Sender);
            }
        }

    } // public partial class Session : IDisposable
}    //namespace Regata.Measurements