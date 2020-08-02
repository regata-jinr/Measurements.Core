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
        public Detector(string name, int timeOutLimitSeconds = 5)
        {
            try
            {
                _nLogger = SessionControllerSingleton.logger.WithProperty("ParamName", name);
                _nLogger.Info($"Initialisation of the detector {name} with mode {ConnectOptions.aReadWrite} and timeout limit {timeOutLimitSeconds}");

                _conOption         = ConnectOptions.aReadWrite;
                _isDisposed        = false;
                Status             = DetectorStatus.off;
                ErrorMessage       = "";
                _device            = new DeviceAccessClass();
                CurrentMeasurement = new MeasurementInfo();

                if (CheckNameOfDetector(name))
                    _name = name;
                else
                    throw new Exception($"Detector with name '{name}' doesn't exist in MID Data base");

                _device.DeviceMessages += DeviceMessagesHandler;
                _timeOutLimitSeconds   = timeOutLimitSeconds;
                IsPaused               = false;

                Connect();

                AcquisitionMode    = AcquisitionModes.aCountToRealTime;
            }
            catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this,e, Handlers.ExceptionLevel.Error);
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

