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
using Regata.Measurements.Managers;
using Regata.Measurements.Models;
using Regata.Measurements.Devices;


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

        /// <summary>
        /// This event will occur after all detectors complete measurements of all samples
        /// </summary>
        // public event Action SessionComplete;
        // public event Action<MeasurementInfo> MeasurementOfSampleDone;

        /// <summary>
        /// This event will occur after one of managed detector by the session complete measurements of all samples
        /// </summary>
        // public event Action<string> MeasurementDone;

        /// <summary>
        /// Sets duration of measurement and the mode of acqusition for each detector controlled by the session. <seealso cref="CanberraDeviceAccessLib.AcquisitionModes"/>
        /// </summary>
        /// <param name="duration">Characterize number of counts for certain mode of acquisition</param>
        /// <param name="acqm">Characterize mode of spectra acquisition. By default is aCountToRealTime</param>
        //private void SetAcquireDurationAndMode(int duration, CanberraDeviceAccessLib.AcquisitionModes acqm)
        //{
        //    Counts = duration;
        //    foreach (var d in ManagedDetectors)
        //        d.SetAcqureCountsAndMode(Counts, CountMode);
        //}

        /// <summary>
        /// Allows user to get chosen acqusition mode specified via <seealso cref="SetAcquireDurationAndMode(int, CanberraDeviceAccessLib.AcquisitionModes)"/>
        /// </summary>
        private CanberraDeviceAccessLib.AcquisitionModes _countMode;
        public CanberraDeviceAccessLib.AcquisitionModes CountMode
        {
            get { return _countMode; }
            set
            {
                var AvailableAcquisitionModes = new CanberraDeviceAccessLib.AcquisitionModes[]
                                                    {
                                                        CanberraDeviceAccessLib.AcquisitionModes.aCountToRealTime,
                                                        CanberraDeviceAccessLib.AcquisitionModes.aCountToLiveTime
                                                    };

                if (!AvailableAcquisitionModes.Contains(value))
                {
                    _nLogger.Info($"Acquisition mode could be chosen only from this modes: {string.Join(", ", AvailableAcquisitionModes)}. aCountToRealTime will be set");
                    return;
                }
                _nLogger.Info($"Acquisition mode of measurements is set to {value}");
                _countMode = value;
                foreach (var d in ManagedDetectors.Values)
                    d.AcquisitionMode = value;
            }
        }

        private Action StartMeasurementsDelegate;
        private Action StopMeasurementsDelegate;
        private Action ClearMeasurementsDelegate;


        private void RunActionOnDetector(string dName, Action act)
        {
            try
            {
                var det = ManagedDetectors[dName];
                if (det.CurrentMeasurement == null || det.RelatedIrradiation == null)
                {
                    NotificationManager.Notify(new ArgumentNullException($"User should initialize sample on the detector {dName} before {act.Method.Name}ing of measurement"), NotificationLevel.Warning, AppManager.Sender);
                    return;
                }

                _nLogger.Info($"Session '{Name}' will {act.Method.Name} measurements of the sample {det.CurrentMeasurement} on detector '{dName}'");
                act();
            }
            catch (Exception e)
            {
                NotificationManager.Notify(e, NotificationLevel.Error, AppManager.Sender);
            }
        }

        private bool IsDetectorConnected(string dName)
        {
            if (!ManagedDetectors.ContainsKey(dName))
            {
                NotificationManager.Notify(new KeyNotFoundException($"Session '{Name}' doesn't control the detector '{dName}'"), NotificationLevel.Warning, AppManager.Sender);
                return false;
            }
            return true;
        }


        public void StartMeasurementOnDetector(string dName)
        {
            if (IsDetectorConnected(dName))
                RunActionOnDetector(dName, ManagedDetectors[dName].Start);
        }

        public void StopMeasurementOnDetector(string dName)
        {
            if (IsDetectorConnected(dName))
                RunActionOnDetector(dName, ManagedDetectors[dName].Stop);
        }
        public void ClearMeasurementOnDetector(string dName)
        {
            if (IsDetectorConnected(dName))
                RunActionOnDetector(dName, ManagedDetectors[dName].Clear);
        }

        /// <summary>
        /// Start asquisition process on all managed detectors by the session
        /// </summary>
        public void StartMeasurements()
        {
            // TODO: recurcive start delegate in case of exception in one detector it removes from delegates and delegate starts again
            _nLogger.Info($"Session '{Name}' has started measurements process");
            try
            {
                StartMeasurementsDelegate();
            }
            catch (Exception e)
            {
                NotificationManager.Notify(e, NotificationLevel.Error, AppManager.Sender);
            }
        }

        ///<summary>
        /// Stops asquisition process on all managed detectors by the session. 
        /// **This generate acqusition done event**
        /// </summary>.
        public void StopMeasurements()
        {
            try
            {
                _nLogger.Info($"Session will stop measurements process by user command");
                StopMeasurementsDelegate();
            }
            catch (Exception e)
            {
                NotificationManager.Notify(e, NotificationLevel.Error, AppManager.Sender);
            }
        }

        ///<summary>
        /// Clear acquired spectrum on all managed detectors by the session. 
        /// **This generate acqusition done event**
        /// </summary>.
        public void ClearMeasurements()
        {
            try
            {
                _nLogger.Info($"Session will clear acquired data by user command");
                ClearMeasurementsDelegate();
            }
            catch (Exception e)
            {
                NotificationManager.Notify(e, NotificationLevel.Error, AppManager.Sender);
            }
        }

    }   // public partial class Session : IDisposable
}     // namespace Regata.Measurements

