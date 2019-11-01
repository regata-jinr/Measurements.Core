/***************************************************************************
 *                                                                         *
 *                                                                         *
 * Copyright(c) 2017-2019, REGATA Experiment at FLNP|JINR                  *
 * Author: [Boris Rumyantsev](mailto:bdrum@jinr.ru)                        *
 * All rights reserved                                                     *
 *                                                                         *
 *                                                                         *
 ***************************************************************************/

// Contains methods for connection, disconnection to the device. Reset connection and so on.
// Detector class divided by few files:

// ├── DetectorAcquisition.cs      --> Contains methods that allow to manage of spectra acquisition process 
// |                                    Start, stop, pause, clear acquisition process and also specify acquisition mode
// ├── DetectorCalibration.cs      --> Contains methods for loading calibration files by energy and height
// ├── DetectorConnection.cs       --> opened
// ├── DetectorFileInteractions.cs --> The code in this file determines interaction with acquiring spectra. 
// |                                    E.g. filling information about sample. Save file
// ├── DetectorInitialization.cs   --> Contains constructor of type, destructor and additional parameters. Like Status enumeration
// |                                    Events arguments and so on
// ├── DetectorParameters.cs       --> Contains methods for getting and setting any parameters by special code
// |                                    See codes here CanberraDeviceAccessLib.ParamCodes 
// |                                    Also some of important parameters wrapped into properties
// ├── DetectorProperties.cs       --> Contains description of basics properties, events, enumerations and additional classes
// └── IDetector.cs                --> Interface of the Detector type

using System;
using System.Threading.Tasks;

namespace Measurements.Core
{
    public partial class Detector : IDetector, IDisposable
    {
        public async void Connect()
        {
            try
            {
                _nLogger.Info($"Starts connecting to the detector");
                var connectionTask = Task.Run(ConnectInternal);
                var delayTask = Task.Delay(TimeSpan.FromSeconds(_timeOutLimitSeconds));

                var firstTask = await Task.WhenAny(connectionTask, delayTask);

                if (firstTask == delayTask)
                    throw new TimeoutException("Connection timed out. The most probably some problem with detector. Try to connect it manually via mvcg.exe");

                if (_device.IsConnected)
                    _nLogger.Info($"Connection to the detector was successful");

            }
            catch (TimeoutException te)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, te, Handlers.ExceptionLevel.Warn);
            }
            catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, e, Handlers.ExceptionLevel.Error);
            }

        }

        private void ConnectInternal()
        {
            try
            {
                _nLogger.Debug($"Starts internal connection to the detector");

                Status = DetectorStatus.off;
                _device.Connect(Name, _conOption);
                Status = DetectorStatus.ready;

                _nLogger.Debug($"Internal connection was successful");
            }
            catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, e, Handlers.ExceptionLevel.Error);
                if (e.Message.Contains("278e2a")) Status = DetectorStatus.busy;
            }
        }

        /// <summary>
        /// Recconects will trying to ressurect connection with detector. In case detector has status error or ready, 
        /// it will do nothing. In case detector is off it will just call connect.
        /// In case status is busy, it will run recursively before 3 attempts with 5sec pausing.
        public void Reconnect()
        {
            _nLogger.Info($"Attempt to reconnect to the detector.");
            //if (_device.IsConnected) return;
            Disconnect();
            System.Threading.Thread.Sleep(1000);
            Connect();
        }

        public void Disconnect()
        {
            try
            {
                _nLogger.Info($"Disconnecting from the detector.");
                if (_device.IsConnected)
                    _device.Disconnect();
                _nLogger.Info($"Disconnecting was successful.");
                Status = DetectorStatus.off;
                ErrorMessage = "";
            }
            catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, e, Handlers.ExceptionLevel.Error);
            }
        }

        public bool IsConnected => _device.IsConnected;
    }
}

