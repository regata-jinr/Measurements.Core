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


    public partial class Detector : IDetector, IDisposable
    {
        private readonly DeviceAccessClass  _device;
        private readonly string    _name;
        private int                _timeOutLimitSeconds;
        private bool               _isDisposed;
        private DetectorStatus     _status;
        private ConnectOptions     _conOption;
        private NLog.Logger        _nLogger;
        private IrradiationInfo    _currentSample;

        public MeasurementInfo CurrentMeasurement { get; set; }
        public event EventHandler  StatusChanged;
        public string FullFileSpectraName { get; private set; }
        public event EventHandler<DetectorEventsArgs>  AcquiringStatusChanged;

        //protected virtual void OnProcessAcquiringMessaget(DetectorEventsArgs e)
        //{
            //AcquiringStatusChanged?.Invoke(this, e);
        //}

       /// <summary>
       /// The reason of this field that stop method generates acquire done event, this means
       /// that we should distinguish stop and pause. That's why this field exist
       /// </summary>
        public bool IsPaused { get; private set; }

        public decimal DeadTime
        {
            get
            {
                if (decimal.Parse(_device.Param[ParamCodes.CAM_X_EREAL].ToString()) == 0)
                   return 0;
                else
                   return 100 * (1 - decimal.Parse(_device.Param[ParamCodes.CAM_X_ELIVE].ToString()) / decimal.Parse(_device.Param[ParamCodes.CAM_X_EREAL].ToString()));
            }
        }
        
        /// <summary>
        /// Irradiated sample spectra of which is acquiring
        /// </summary>
        public IrradiationInfo CurrentSample
        {
            get { return _currentSample; }
            set
            {
                _nLogger.Info($"Set sample {value} as current");
                _currentSample = value;
            }
        }

        /// <summary>
        /// Name of detector
        /// </summary>
        public string Name
        {
            get { return _name; }
        }
        private  bool CheckNameOfDetector(string name)
        {
            try
            {
                var detsList = (IEnumerable<object>)_device.ListSpectroscopyDevices;
                if (detsList.Contains(name))
                {
                    _nLogger.Info($"Detector with name '{name}' was found in the MID wizard list and will be used");
                    return true;
                }
                else
                {
                    Status = DetectorStatus.error;
                    ErrorMessage = $"Detector with name '{name}' wasn't find in the MID wizard list. Status will change to 'error'";
                    Handlers.ExceptionHandler.ExceptionNotify(this, new ArgumentException(ErrorMessage), Handlers.ExceptionLevel.Error);
                    return false;
                }
            }
            catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, e, Handlers.ExceptionLevel.Error);
                return false;
            }
        }
        

        public int CountToRealTime =>  int.Parse(GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_X_PREAL)); 
        public int CountToLiveTime =>  int.Parse(GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_X_PLIVE)); 

       
        /// <summary> Returns status of detector. {ready, off, busy, error}. </summary>
        /// <seealso cref="Enum Status"/>
        public DetectorStatus Status
        {
            get { return _status; }

            private set
            {
                if (_status != value)
                {
                    _nLogger.Info($"The detector status changed from {_status} to {value}");
                    _status = value;
                    StatusChanged?.Invoke(this, EventArgs.Empty);
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

