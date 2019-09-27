/***************************************************************************
 *                                                                         *
 *                                                                         *
 * Copyright(c) 2017-2019, REGATA Experiment at FLNP|JINR                  *
 * Author: [Boris Rumyantsev](mailto:bdrum@jinr.ru)                        *
 * All rights reserved                                                     *
 *                                                                         *
 *                                                                         *
 ***************************************************************************/

using System;
using System.Linq;

namespace Measurements.Core
{
    // this file contains method for changing and setting sample to detectors
    // Session class divided by few files:
    // ├── ISession.cs                - interface of Session class
    // ├── SessionDetectorsControl.cs - contains methods that related with managing of detector
    // ├── SessionInfo.cs             - contains fields of Session for EF core interaction
    // ├── SessionLists.cs            - contains method that forms list of samples and measurements 
    // ├── SessionMain.cs             - contains general fields and methods of the Session class.
    // └── SessionSamplesMoves.cs     --> opened
    public partial class Session : ISession, IDisposable
    {
        /// <summary>
        /// Change sample on chosen detector to the next one by the order in SpreadSamples dictionary
        /// </summary>
        /// <paramref name="d">Reference to the detector object</paramref>
        /// <returns>True in case of next sample exist and was changed successfuly</returns>
        public bool NextSample(ref IDetector d)
        {
            try
            { 
                _nLogger.Info($"Change sample {d.CurrentSample.ToString()} to the next one for detector {d.Name}");
                int currentIndex = SpreadSamples[d.Name].IndexOf(d.CurrentSample);
                if (currentIndex != SpreadSamples[d.Name].Count - 1)
                {
                    d.CurrentSample = SpreadSamples[d.Name][++currentIndex];
                    int IrId = d.CurrentSample.Id;
                    d.CurrentMeasurement = MeasurementList.Where(cm => cm.IrradiationId == IrId).First();
                    return true;
                }
                _nLogger.Info($"Sample {d.CurrentSample.ToString()} was the last sample on detector {d.Name}. Detector will send signal that measurements has completed");
            }
            catch (IndexOutOfRangeException ie)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, ie, Handlers.ExceptionLevel.Warn);
            }
            catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, e, Handlers.ExceptionLevel.Error);
            }

            return false;
        }

        // TODO: add option to set first, last, in case of number on one of the detector more than exist on another set last
        /// <summary>
        /// Allows user to set samples on all detectors by the number
        /// </summary>
        /// <param name="n"></param>
        public void MakeSamplesCurrentOnAllDetectorsByNumber(int n = 0)
        {
            try
            {
                foreach (var d in ManagedDetectors)
                {
                    if (n < 0 || n >= SpreadSamples[d.Name].Count)
                        continue;
                        //throw new IndexOutOfRangeException($"For detector '{d.Name}' index out of range");

                    d.CurrentSample = SpreadSamples[d.Name][n];
                }
            }
            catch (IndexOutOfRangeException ie)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, ie, Handlers.ExceptionLevel.Warn);
            }
        }

        /// <summary>
        /// Allows user to set certain sample to certain detector
        /// </summary>
        /// <paramref name="ii">Irradiated sample<see cref="IrradiationInfo"/></paramref>
        /// <paramref name="d">Reference to the Detector instance</paramref>
        public void MakeSampleCurrentOnDetector(ref IrradiationInfo ii, ref IDetector d)
        {
            _nLogger.Info($"Make sample {ii.ToString()} current on detector {d.Name}");
            d.CurrentSample = ii;
        }

        /// <summary>
        /// Change sample on chosen detector to the previous one by the order in SpreadSamples dictionary 
        /// </summary>
        /// <param name="d"></param>
        public void PrevSample(ref IDetector d)
        {
            try
            {
                _nLogger.Info($"Change sample {d.CurrentSample.ToString()} to the previous one for dtector {d.Name}");
                int currentIndex = SpreadSamples[d.Name].IndexOf(d.CurrentSample);
                if (currentIndex == 0)
                    throw new IndexOutOfRangeException($"Current sample on detector {d.Name} has 0 index. Can't go to the previous sample");
                d.CurrentSample = SpreadSamples[d.Name][--currentIndex];
            }
            catch (IndexOutOfRangeException ie)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, ie, Handlers.ExceptionLevel.Warn);
            }
            catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, e, Handlers.ExceptionLevel.Error);
            }
        }
    } // class
} // namespace
