/***************************************************************************
 *                                                                         *
 *                                                                         *
 * Copyright(c) 2017-2019, REGATA Experiment at FLNP|JINR                  *
 * Author: [Boris Rumyantsev](mailto:bdrum@jinr.ru)                        *
 * All rights reserved                                                     *
 *                                                                         *
 *                                                                         *
 ***************************************************************************/

// Contains constructor of type, destructor and additional parameters. Like Status enumeration
// Events arguments and so on
// Detector class divided by few files:

// ├── DetectorAcquisition.cs      --> Contains methods that allow to manage of spectra acquisition process. 
// |                                    Start, stop, pause, clear acquisition process and also specify acquisition mode.
// ├── DetectorCalibration.cs      --> Contains methods for loading calibration files by energy and height
// ├── DetectorConnection.cs       --> Contains methods for connection, disconnection to the device. Reset connection and so on.
// ├── DetectorFileInteractions.cs --> The code in this file determines interaction with acquiring spectra. 
// |                                    E.g. filling information about sample. Save file.
// ├── DetectorInitialization.cs   --> opened
// ├── DetectorParameters.cs       --> Contains methods for getting and setting any parameters by special code.
// |                                    See codes here CanberraDeviceAccessLib.ParamCodes. 
// |                                    Also some of important parameters wrapped into properties
// ├── DetectorProperties.cs       --> Contains description of basics properties, events, enumerations and additional classes
// └── IDetector.cs                --> Interface of the Detector type

using System;
using CanberraDeviceAccessLib;

namespace Measurements.Core
{
    public partial class Detector : IDetector, IDisposable
    {
        /// <summary>Constructor of Detector class.</summary>
        /// <param name="name">Name of detector. Without path.</param>
        /// <param name="option">CanberraDeviceAccessLib.ConnectOptions {aReadWrite, aContinue, aNoVerifyLicense, aReadOnly, aTakeControl, aTakeOver}.By default ConnectOptions is ReadWrite.</param>
        public Detector(string name, ConnectOptions option = ConnectOptions.aReadWrite, int timeOutLimitSeconds = 5)
        {
            try
            {
                _nLogger = SessionControllerSingleton.logger.WithProperty("ParamName", name);
                _nLogger.Info($"Initialisation of the detector {name} with mode {option.ToString()} and timeout limit {timeOutLimitSeconds}");

                _conOption         = option;
                _isDisposed        = false;
                Status             = DetectorStatus.off;
                ErrorMessage       = "";
                AcquisitionMode    = AcquisitionModes.aCountToRealTime;
                _device            = new DeviceAccessClass();
                CurrentMeasurement = new MeasurementInfo();

                if (CheckNameOfDetector(name))
                    Name = name;
                else
                    throw new Exception($"Detector with name '{name}' doesn't exist in MID Data base");

                _device.DeviceMessages += ProcessDeviceMessages;
                _timeOutLimitSeconds   = timeOutLimitSeconds;
                IsPaused               = false;

                Connect();
            }
            catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this,e, Handlers.ExceptionLevel.Error);
            }
        }

        /// <summary>
        ///
        ///  |Advise Mask        |Description                                        |int value(lParam)|
        ///  |:-----------------:|:-------------------------------------------------:|:---------------:|
        ///  |DisplaySetting     | Display settings have changed                     |  1              |
        ///  |ExternalStart      | Acquisition has been started externall            |  1048608        |
        ///  |CalibrationChange  | A calibration parameter has changed               |  4              |
        ///  |AcquireStart       | Acquisition has been started                      |  134217728      |
        ///  |AcquireDone        | Acquisition has been stopped                      | -2147483648     |
        ///  |DataChange         | Data has been changes (occurs after AcquireClear) |  67108864       |
        ///  |HardwareError      | Hardware error                                    |  2097152        |
        ///  |HardwareChange     | Hardware setting has changed                      |  268435456      |
        ///  |HardwareAttention  | Hardware is requesting attention                  |  16777216       |
        ///  |DeviceUpdate       | Device settings have been updated                 |  8388608        |
        ///  |SampleChangerSet   | Sample changer set                                |  1073741824     |
        ///  |SampleChangeAdvance| Sample changer advanced                           |  4194304        |

        /// </summary>
        /// <param name="message">DeviceMessages type from CanberraDeviceAccessLib</param>
        /// <param name="wParam">The first parameter of information associated with the message.</param>
        /// <param name="lParam">The second parameter of information associated with the message</param>
        private void ProcessDeviceMessages(int message, int wParam, int lParam)
        {
            string response = "";
            bool isForCalling = false;
            try
            {
                if ((int)AdviseMessageMasks.amAcquireDone == lParam && !IsPaused)
                {
                    _nLogger.Info($"Has got message AcquireDone.");
                    response = "Acquire has done";
                    Status = DetectorStatus.ready;
                    CurrentMeasurement.DateTimeFinish = DateTime.Now;
                    isForCalling = true;
                }

                if ((int)AdviseMessageMasks.amAcquireStart == lParam)
                {
                    _nLogger.Info($"Has got message amAcquireStart.");
                    response = "Acquire has start";
                    Status = DetectorStatus.busy;
                    isForCalling = true;
                }

                if ((int)AdviseMessageMasks.amHardwareError == lParam)
                {
                    Status = DetectorStatus.error;
                    ErrorMessage = $"{_device.Message((MessageCodes)lParam)}";
                    response = ErrorMessage;
                    isForCalling = true;
                }
                if ((int)AdviseMessageMasks.amAcquisitionParamChange == lParam)
                {
                    response = "Device ready to use!";

                }

                if (isForCalling)
                    AcquiringStatusChanged?.Invoke(this, new DetectorEventsArgs { Message = response, AcquireMessageParam = lParam, Name = this.Name, Status = this.Status });
            }
            catch (Exception e)
            {
                    Handlers.ExceptionHandler.ExceptionNotify(this, e, Handlers.ExceptionLevel.Error);
            }
        }

        private void CleanUp(bool isDisposing)
        {
            _nLogger.Info($"Cleaning of the detector {Name}");

            if (!_isDisposed)
            {
                if (isDisposing)
                {
                    NLog.LogManager.Flush();
                }
                Disconnect();
            }
            _isDisposed = true;
        }

        ~Detector()
        {
            CleanUp(false);
        }

        /// <summary>
        /// Disconnects from detector. Changes status to off. Resets ErrorMessage. Clears the detector.
        /// </summary>
        public void Dispose()
        {
            CleanUp(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Power reset for the detector. 
        /// </summary>
        public void Reset()
        {
            //FIXME: not tested
            try
            {
                _nLogger.Info($"Attempt to reset the detector");
                _device.SendCommand(DeviceCommands.aReset);

                if (Status == DetectorStatus.ready)
                    _nLogger.Info($"Resetting was successful");
                else
                    throw new Exception($"Something were wrong during reseting of the detector '{Name}'");
            }
            catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, e, Handlers.ExceptionLevel.Error);
            }

        }

    }
}

