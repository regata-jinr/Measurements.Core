/***************************************************************************
 *                                                                         *
 *                                                                         *
 * Copyright(c) 2017-2019, REGATA Experiment at FLNP|JINR                  *
 * Author: [Boris Rumyantsev](mailto:bdrum@jinr.ru)                        *
 * All rights reserved                                                     *
 *                                                                         *
 *                                                                         *
 ***************************************************************************/

// Interface of the Detector type
// Detector class divided by few files:

// ├── DetectorAcquisition.cs      --> Contains methods that allow to manage of spectra acquisition process. 
// |                                    Start, stop, pause, clear acquisition process and also specify acquisition mode.
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
// └── IDetector.cs                --> opened

//TODO: add groups for interface members according to file division

using System;
using CanberraDeviceAccessLib;

namespace Measurements.Core
{
    public interface IDetector
    {
        // DetectorAcquisition
        void             Start();
        void             Pause();
        void             Stop();
        void             Clear();
        bool             IsPaused        { get; }
        AcquisitionModes AcquisitionMode { get; set; }
        int              Counts          { get; set; }

        // DetectorCalibration
        void             AddEfficiencyCalibrationFileByHeight(decimal height);
        void             AddEfficiencyCalibrationFileByEnergy();

        // DetectorConnection
        void             Connect();
        void             Reconnect();
        void             Disconnect();
        bool             IsConnected { get; }

        // DetectorFileInteractions
        void             LoadMeasurementInfoToDevice(MeasurementInfo measurement, IrradiationInfo irradiation);
        void             Save(string fullFileName="");

        // DetectorInitialization
        void             Dispose();
        void             Reset();


        // DetectorParameters
        string           GetParameterValue(ParamCodes parCode);
        void             SetParameterValue<T>(ParamCodes parCode, T val);
        decimal          DeadTime            { get; }
        int              PresetRealTime      { get; }
        int              PresetLiveTime      { get; }
        decimal          ElapsedRealTime     { get; }
        decimal          ElapsedLiveTime     { get; }
        bool             IsHV                { get; }

        // DetectorProperties
        DetectorStatus   Status              { get; }
        string           ErrorMessage        { get; }
        MeasurementInfo  CurrentMeasurement  { get; }
        IrradiationInfo  RelatedIrradiation  { get; }

        event EventHandler StatusChanged;
        event EventHandler<DetectorEventsArgs> AcquiringStatusChanged;
    }
}
