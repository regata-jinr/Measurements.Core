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
using System.Collections.Generic;
using System.Linq;
using AutoMapper;

namespace Measurements.Core
{
    // this file  contains method that forms list of samples and measurements 
    // Session class divided by few files:
    // ├── ISession.cs                - interface of Session class
    // ├── SessionDetectorsControl.cs - contains methods that related with managing of detector
    // ├── SessionInfo.cs             - contains fields of Session for EF core interaction
    // ├── SessionLists.cs            --> opened
    // ├── SessionMain.cs             - contains general fields and methods of the Session class.
    // └── SessionSamplesMoves.cs     - contains method for changing and setting sample to detectors

    public partial class Session : ISession, IDisposable
    {
        /// <summary>
        /// Formes list of samples that were irradiated in chosen date
        /// </summary>
        /// <param name="date">Date of irradiation samples</param>
        private void SetIrradiationsList(DateTime date)
        {
            _nLogger.Info($"List of samples from irradiations journal will be loaded. Then list of measurements will be prepare");
            try
            {
                if (string.IsNullOrEmpty(Type))
                    throw new ArgumentNullException("Before choosing date of irradiations you should choose type of irradiations");
                IrradiationList.Clear();
                IrradiationList.AddRange(_infoContext.Irradiations.Where(i => i.DateTimeStart.HasValue && i.DateTimeStart.Value.Date == date.Date && i.Type == Type).ToList());

                var configuration = new MapperConfiguration(cfg => cfg.AddMaps("MeasurementsCore"));
                var mapper = new Mapper(configuration);

                foreach (var i in IrradiationList)
                {
                    var m = mapper.Map<MeasurementInfo>(i);
                    m.Type = Type;
                    m.Height = Height;
                    m.Assistant = SessionControllerSingleton.ConnectionStringBuilder.UserID;
                    MeasurementList.Add(m);
                }

                SpreadSamplesToDetectors();
            }
            catch (ArgumentNullException ane)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, ane, Handlers.ExceptionLevel.Error);
            }
            catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, e, Handlers.ExceptionLevel.Error);
            }
        }

        /// <summary>
        /// Sometimes we have more samples than the disk might contain. In this case
        /// Event of disk overflow should be generate
        /// </summary>
        private void CheckExcessionOfDiskSize()
        {
                //if ((IrradiationList.Count / ManagedDetectors.Count) > SampleChanger.SizeOfDisk)
                //ExcessTheSizeOfDiskEvent?.Invoke(this, EventArgs.Empty);
        }

        //TODO: add event that occures when sample number increase the size of disk, but user should decide break the measurements or not!
        //      also in case of continue of measurements in the end of measurements user should accept that sample were changed on the disk
        /// <summary>
        /// This mode of spreading related with distribution sample to the detectors according to their containers.
        /// There is an algorithm:
        /// 1. Get ordered list of unique container numbers (not necessary one by one (1,3,4,5) also is possible)
        /// 2. Then it starts to asign samples from first container number to first detector, so on till iteration by detectors  is over
        /// 3. When iteration by detectors is over, but container numbers is not, continue assign samples from next container numbers
        ///    to first detector.
        /// </summary>
        private void SpreadSamplesByContainer()
        {
            try
            {
                CheckExcessionOfDiskSize();

                var NumberOfContainers = IrradiationList.Select(ir => ir.Container).Where(ir => ir.HasValue).Distinct().OrderBy(ir => ir.Value).ToArray();

                if (!NumberOfContainers.Any())
                {
                    SpreadSamplesUniform();
                    throw new ArgumentException("Spreading by container could not be use for the measurements because samples don't have information about containers numbers. Uniform option was used, also you can use spreading by the order for this type");
                }

                int i = 0;

                foreach (var conNum in NumberOfContainers)
                {
                    var sampleList = new List<IrradiationInfo> (IrradiationList.Where(ir => ir.Container == conNum).ToList());
                    SpreadedSamples[ManagedDetectors[i].Name].AddRange(sampleList);
                    i++;
                    if (i >= ManagedDetectors.Count())
                        i = 0;
                }
            }
            catch (ArgumentException ae)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, ae, Handlers.ExceptionLevel.Warn);
            }
             catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, e, Handlers.ExceptionLevel.Error);
            }
        }

        /// <summary>
        /// This option allows merely divide all samples to detectors in the same portions.
        /// </summary>
        private void SpreadSamplesUniform()
        {
            try
            {
                CheckExcessionOfDiskSize();

                int i = 0;

                foreach (var sample in IrradiationList)
                {
                    SpreadedSamples[ManagedDetectors[i].Name].Add(sample);
                    i++;
                    if (i >= ManagedDetectors.Count())
                        i = 0;
                }
            }
            catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, e, Handlers.ExceptionLevel.Error);
            }
        }

        /// <summary>
        /// When this option has chosen samples will divide to the detector by the order:
        /// First sample to first detector, second to second, so on...
        /// </summary>
        private void SpreadSamplesByTheOrder()
        {
            try
            {
                CheckExcessionOfDiskSize();

                int i = 0; // number of detector
                int n = 0; // number of sample

                foreach (var sample in IrradiationList)
                {

                    if (i >= ManagedDetectors.Count())
                        throw new IndexOutOfRangeException("Count of samples more then disk can contains");
                    SpreadedSamples[ManagedDetectors[i].Name].Add(sample);
                    n++;

                    if (n >= SampleChanger.SizeOfDisk)
                    {
                        i++;
                        n = 0;
                    }
                }
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

        /// <summary>
        /// This method describes the division of samples to detectors according with chosen SpreadedOption with requred checks
        /// <seealso cref="SpreadSamplesByContainer"/>
        /// <seealso cref="SpreadSamplesUniform"/>
        /// <seealso cref="SpreadSamplesByTheOrder"/>
        /// </summary>
        private void SpreadSamplesToDetectors()
        {
            _nLogger.Info($"Spreading samples to detectors has began");
            try
            {
                if (!ManagedDetectors.Any())
                    throw new ArgumentOutOfRangeException("Session has managed no-one detector");

                if (!IrradiationList.Any())
                    throw new ArgumentOutOfRangeException("Session doesn't contain samples to measure");

                foreach (var dName in ManagedDetectors.Select(d => d.Name).ToArray())
                {
                    if (SpreadedSamples[dName].Any())
                        SpreadedSamples[dName].Clear();
                }

                if (SpreadOption == SpreadOptions.container)
                    SpreadSamplesByContainer();
                else if (SpreadOption == SpreadOptions.inOrder)
                    SpreadSamplesByTheOrder();
                else if (SpreadOption == SpreadOptions.uniform)
                    SpreadSamplesUniform();
                else
                {
                    SpreadSamplesByContainer();
                    throw new Exception("Type of spreaded options doesn't recognize. Spreaded by container has done.");
                }

                MakeSamplesCurrentOnAllDetectorsByNumber();

                foreach (var d in ManagedDetectors)
                {
                    if (SpreadedSamples[d.Name].Count == 0)
                        continue;

                    d.CurrentMeasurement = MeasurementList.Where(cm => cm.IrradiationId == d.CurrentSample.Id).First();
                    _nLogger.Info($"Samples [{(string.Join(",", SpreadedSamples[d.Name].OrderBy(ss => $"{ss.SetKey}-{ss.SampleNumber}").Select(ss => $"{ss.SetKey}-{ss.SampleNumber}").ToArray()))}] will measure on the detector {d.Name}");
                }

            }
            catch (ArgumentOutOfRangeException ae)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, ae, Handlers.ExceptionLevel.Error);
                SessionComplete?.Invoke();
            }
           catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, e, Handlers.ExceptionLevel.Error);
            }
        }
    }
}
