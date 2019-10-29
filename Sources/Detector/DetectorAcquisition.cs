/***************************************************************************
 *                                                                         *
 *                                                                         *
 * Copyright(c) 2017-2019, REGATA Experiment at FLNP|JINR                  *
 * Author: [Boris Rumyantsev](mailto:bdrum@jinr.ru)                        *
 * All rights reserved                                                     *
 *                                                                         *
 *                                                                         *
 ***************************************************************************/

// This file contains methods that allow to manage of spectra acquisition process. 
// Start, stop, pause, clear acquisition process and also specify acquisition mode.
// Detector class divided by few files:

// ├── DetectorAcquisition.cs      --> opened
// ├── DetectorCalibration.cs      --> Contains methods for loading calibration files by energy and height
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
using CanberraDeviceAccessLib;

namespace Measurements.Core
{
    public partial class Detector : IDetector, IDisposable
    {
        /// <summary>
        ///  Starts acquiring with specified aCountToLiveTime, before this the device will be cleared.
        /// </summary>
        /// <param name="time"></param>
        public void Start()
        {
            try
            {
                if (!IsPaused)
                    _device.Clear();

                IsPaused = false;

                if (Status != DetectorStatus.ready)
                    throw new Exception($"Status of detector '{Name}' is not 'ready'");
                _device.SpectroscopyAcquireSetup(AcquisitionModes.aCountToRealTime, CurrentMeasurement.Duration.Value);
                _device.AcquireStart(); // already async
                _nLogger.Info($"Acquiring in process...");
                Status = DetectorStatus.busy;
                CurrentMeasurement.DateTimeStart = DateTime.Now;
                CurrentMeasurement.Detector = Name;
            }
            catch (Exception e) 
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, e, Handlers.ExceptionLevel.Error);
            }

        }

        /// <summary>
        /// Set acquiring on pause.
        /// </summary>
        public void Pause()
        {
            try
            {
                if (Status == DetectorStatus.ready)
                    return;

                _nLogger.Info($"Attempt to set pause for the acquiring");
                _device.AcquirePause();
                IsPaused = true;
                Status = DetectorStatus.ready;
                _nLogger.Info($"Paused was successful. Detector ready to continue acquire process");
            }
            catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, e, Handlers.ExceptionLevel.Error);
            }

        }

        /// <summary>
        /// Stops acquiring. Means pause and clear and **generate acquire done event.**
        /// </summary>
        public void Stop()
        {
            try
            {
                if (Status == DetectorStatus.ready)
                    return;
                _nLogger.Info($"Attempt to stop the acquiring");
                _device.AcquireStop(StopOptions.aNormalStop);
                //_device.SendCommand(DeviceCommands.aStop); // use command sending because in this case it will generate AcquireDone message
                IsPaused = false;
                Status = DetectorStatus.ready;
                _nLogger.Info($"Stop was successful. Acquire done event will be generate. Detector ready to acquire again");
            }
            catch (Exception e) 
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, e, Handlers.ExceptionLevel.Error);
            }

        }

        /// <summary>
        /// Clears current acquiring status.
        /// </summary>
        public void Clear()
        {
            try
            {
                _nLogger.Info($"Clearing the detector");
                _device.Clear();
            }
            catch (Exception e) 
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, e, Handlers.ExceptionLevel.Error);
            }
        }

        // TODO: add properties for acquisition mode and counts. In case of change either counts or acquisition mode. SetSpectroscopySetUp should be called
        public AcquisitionModes AcquisitionMode { get; set; }

        public int Counts { get; set; }

        /// <summary>
        /// The reason of this field that stop method generates acquire done event, this means
        /// that we should distinguish stop and pause. That's why this field exist
        /// </summary>
        public bool IsPaused { get; private set; }


    }
}

