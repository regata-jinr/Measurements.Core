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
using Regata.Measurements.Devices;
using Regata.Measurements.Models;

namespace Regata.Measurements
{
    // this file contains general fields and methods of the Session class.
    // Session class divided by few files:
    // ├── ISession.cs                - interface of Session class
    // ├── SessionDetectorsControl.cs - contains methods that related with managing of detector
    // ├── SessionInfo.cs             - contains fields of Session for EF core interaction
    // └── SessionMain.cs --> opened

    /// <summary>
    /// Session class is used for control measurement process. Our measurement process involved few principal parameters:
    /// Type of measurement might be short lived(SLI), long lived that divided by two types just 1 or 2(LLI-1, LLI-2), measurement of background(FON)
    /// Date of irradiation that allow to receive list of sample which were irradiated in this date.
    /// Duration of measurement
    /// Count Mode - is internal parameter of MCA. It might be aCountToNormal, aCountToRealTime, aCountToLiveTime
    /// Height - is the distance between head of the detector and the sample
    /// </summary>
    public partial class Session : IDisposable
    {
        /// <summary>
        /// See description of logger in <see cref="SessionControllerSingleton"/>
        /// </summary>
        private NLog.Logger _nLogger;

        private string _activeMeasurementsRegister;

        public bool IsCyclic { get; set; }

        public uint CyclicPauseTime { get; set; }

        /// <summary>
        /// List of irradiated samples with specified date and type. <see cref="IrradiationInfo"/>
        /// </summary>
        public List<IrradiationInfo> IrradiationList { get; private set; }

        private string _type;

        /// <summary>
        /// Type of measurement characterize some internal working logic for analysis. <seealso cref="MeasurementTypes"/>
        /// After type will specify. List of the irradiations date become available for the usage.
        /// </summary>
        public string Type
        {
            get { return _type; }
            set
            {
                try
                {
                    _nLogger.Info($"Type of measurement is {value}. List of irradiations dates will be prepare");

                    if (!MeasurementInfo.SessionTypeMapStr.Values.Contains(value))
                        throw new ArgumentException($"Type of measurement should contained in this list [{string.Join(",", MeasurementInfo.SessionTypeMapStr.Values)}]");

                    _type = value;

                    IrradiationDateList.Clear();
                    IrradiationDateList.AddRange(AppManager.DbContext.Irradiations.Where(i => i.DateTimeStart.HasValue && i.Type == _type).Select(i => i.DateTimeStart.Value.Date).Distinct().OrderByDescending(d => d.Date).ToList());
                }
                catch (ArgumentException ae)
                {
                    NotificationManager.Notify(ae, NotificationLevel.Warning, AppManager.Sender);
                }
                catch (Exception e)
                {
                    NotificationManager.Notify(e, NotificationLevel.Error, AppManager.Sender);
                }
            }
        }

        public override string ToString() => $"{Name}-{Type}";

        /// <summary>
        /// Contains additional information about current session
        /// </summary>
        public string Note { get; set; }
        private string _name;
        /// <summary>
        /// Property for setting of the name of session.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set
            {
                if (!string.IsNullOrEmpty(Name))
                    _nLogger.Info($"Session '{Name}' will rename to '{value}'");

                if (AppManager.ActiveSessions.ContainsKey(_name))
                {
                    AppManager.ActiveSessions[value] = this;
                    AppManager.ActiveSessions.Remove(_name);
                }
                _name = value;
                _nLogger = AppManager.logger.WithProperty("Sender", $"{typeof(Session).Name} {_name}");

            }
        }


        /// <summary>
        /// This event will occur after all detectors complete measurements of all samples
        /// </summary>
        public event Action SessionComplete;
        public event Action<MeasurementInfo> MeasurementOfSampleDone;

        /// <summary>
        /// This event will occur after one of managed detector by the session complete measurements of all samples
        /// </summary>
        public event Action<string> MeasurementDone;

        /// <summary>
        /// List of detectors that controlled by the session
        /// </summary>
        public Dictionary<string, Detector> ManagedDetectors { get; private set; }

        public Dictionary<string, List<MeasurementInfo>> MeasurementsRegisters { get; private set; }

        /// <summary>
        /// Default constructor of the session class. This initialize field and specify some default values. For more details see the code.
        /// </summary>
        public Session()
        {
            Name = $"UntitledSession_{AppManager.ActiveSessions.Count}";

            _nLogger.Info("Initialisation of session has begun");

            ManagedDetectors = new Dictionary<string, Detector>();
            MeasurementsRegisters = new Dictionary<string, List<MeasurementInfo>>();
            CountMode = CanberraDeviceAccessLib.AcquisitionModes.aCountToRealTime;
            // MeasurementDone += MeasurementDoneHandler;
        }

        private bool _isDisposed = false;
        ~Session()
        {
            CleanUp();
        }

        public void Dispose()
        {
            CleanUp();
            GC.SuppressFinalize(this);
        }

        private void CleanUp()
        {
            _nLogger.Info($"Disposing session has begun");

            if (!_isDisposed)
            {
                for (var i = ManagedDetectors.Count - 1; i >= 0; --i)
                    DetachDetector(ManagedDetectors.ElementAt(i).Key);

                // MeasurementDone -= MeasurementDoneHandler;
                AppManager.ActiveSessions.Remove(Name);
            }
            _isDisposed = true;
        }

    } // public partial class Session : IDisposable


} // namespace Regata.Measurements