//Only fields and properties of Detector class


using System;
using System.Linq;
using System.Collections.Generic;
using CanberraDeviceAccessLib;

//TODO: set up db target to logger https://knightcodes.com/.net/2016/05/25/logging-to-a-database-wth-nlog.html

/// <summary>
/// This namespace contains implementations of core interfaces or internal classes.
/// </summary>
namespace Measurements.Core
{
    /// <summary>
    ///  Enumeration of possible detector's working statuses
    ///  ready - Detector is enabled and ready for acquiring
    ///  off   - Detector is disabled
    ///  busy  - Detector is acquiring spectrum
    ///  error - Detector has porblems
    /// </summary>
    public enum DetectorStatus { off, ready, busy, error }

    /// <summary>
    /// Detector is one of the main class, because detector is the main part of our experiment. It allows to manage real detector and has protection from crashes. You can start, stop and do any basics operations which you have with detector via mvcg.exe. This software based on dlls provided by [Genie2000] (https://www.mirion.com/products/genie-2000-basic-spectroscopy-software) for interactions with [HPGE](https://www.mirion.com/products/standard-high-purity-germanium-detectors) detectors also from [Mirion Tech.](https://www.mirion.com). Personally we are working with [Standard Electrode Coaxial Ge Detectors](https://www.mirion.com/products/sege-standard-electrode-coaxial-ge-detectors)
    /// </summary>
    /// <seealso cref="https://www.mirion.com/products/genie-2000-basic-spectroscopy-software"/>


    //TODO: improve logs readability
    //TODO: save logs to db
    public partial class Detector : IDetector, IDisposable
    {
        private DeviceAccessClass _device;
        private string _name;
        private int _timeOutLimitSeconds;
        private int _countToRealTime;
        private int _countToLiveTime;
        private int _countNormal;
        private bool isDisposed;
        private DetectorStatus _status;
        private ConnectOptions _conOption;
        public event EventHandler DetectorChangedStatusEvent;
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private NLog.Logger _nLogger;
        private string fileName;

        public Measurement CurrentMeasurement { get; set; }
        public IrradiationInfo CurrentSample { get; set; }

        public string Name
        {
            get { return _name; }
            private set
            {
                var detsList = (IEnumerable<object>)_device.ListSpectroscopyDevices;
                if (detsList.Contains(value))
                {
                    _name = value;
                    _nLogger.Info($"{value})--Detector with name '{value}' was found in the MID wizard list and will be used.");
                }
                else
                {

                    Status = DetectorStatus.error;
                    ErrorMessage = $"{value})--Detector with name '{value}' wasn't find in the MID wizard list. Status will change to 'error'.";
                    Handlers.ExceptionHandler.ExceptionNotify(this, new Handlers.ExceptionEventsArgs { Message = $"Detector({_name}). {ErrorMessage}", Level = NLog.LogLevel.Error });
                }
            }
        }
        public int CountToRealTime
        {
            get { return _countToRealTime; }
            set
            {
                _nLogger.Info($"{value})--Setting aCountToRealTime.");
                _device.SpectroscopyAcquireSetup(CanberraDeviceAccessLib.AcquisitionModes.aCountToRealTime, value);
            }
        }

        public int CountToLiveTime
        {
            get { return _countToLiveTime; }
            set
            {
                _nLogger.Info($"{value})--Setting aCountToLiveTime.");
                _device.SpectroscopyAcquireSetup(CanberraDeviceAccessLib.AcquisitionModes.aCountToLiveTime, value);
            }
        }

        public int CountNormal
        {
            get { return _countNormal; }
            set
            {
                _nLogger.Info($"{value})--Setting aCountNormal.");
                _device.SpectroscopyAcquireSetup(CanberraDeviceAccessLib.AcquisitionModes.aCountNormal, value);
            }
        }

        /// <summary> Returns status of detector. {ready, off, busy, error}. </summary>
        /// <seealso cref="Enum Status"/>
        public DetectorStatus Status
        {
            get { return _status; }

            private set
            {
                if (_status != value)
                {
                    _nLogger.Info($"value)--The detector status changed from {_status} to {value}");
                    _status = value;
                    DetectorChangedStatusEvent?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Returns true if high voltage is on.
        /// </summary>
        public bool IsHV { get { return _device.HighVoltage.On; } }

        /// <summary>
        /// Returns true if detector connected successfully.
        /// </summary>
        public bool IsConnected { get { return _device.IsConnected; } }

        /// <summary>
        /// Returns error message.
        /// </summary>
        public string ErrorMessage { get; private set; }

    }
   
}

