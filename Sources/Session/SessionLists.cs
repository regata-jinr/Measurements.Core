using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;

namespace Measurements.Core
{
    public partial class Session : ISession, IDisposable
    {
        private void SetIrradiationsList(DateTime date)
        {
            _nLogger.Info($"List of samples from irradiations journal will be loaded. Then list of measurements will be prepare");
            try
            {
                if (string.IsNullOrEmpty(Type))
                    throw new ArgumentNullException("Before choosing date of irradiations you should choose type of irradiations");

                IrradiationList.AddRange(_infoContext.Irradiations.Where(i => i.DateTimeStart.HasValue && i.DateTimeStart.Value.Date == date.Date && i.Type == Type).ToList());

                var configuration = new MapperConfiguration(cfg => cfg.AddMaps("MeasurementsCore"));
                var mapper = new Mapper(configuration);

                foreach (var i in IrradiationList)
                {
                    var m = mapper.Map<MeasurementInfo>(i);
                    m.Type = Type;
                    m.Assistant = SessionControllerSingleton.ConnectionStringBuilder.UserID;
                    MeasurementList.Add(m);
                }
            }
            catch (ArgumentNullException ane)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, new Handlers.ExceptionEventsArgs { Message = $"{ane.Message}", Level = NLog.LogLevel.Error });
            }
            catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, new Handlers.ExceptionEventsArgs { Message = $"{e.Message}", Level = NLog.LogLevel.Error });
            }
        }

        private void CheckExcessionOfDiskSize()
        {
                //if ((IrradiationList.Count / ManagedDetectors.Count) > SampleChanger.SizeOfDisk)
                //ExcessTheSizeOfDiskEvent?.Invoke(this, EventArgs.Empty);
        }

        //TODO: add event in case of sample number increase the size of disk, but user should decide break the measurements or not!
        private void SpreadSamplesByContainer()
        {
            try
            {
                CheckExcessionOfDiskSize();

                var NumberOfContainers = IrradiationList.Select(ir => ir.Container).Distinct().OrderBy(ir => ir.Value).ToArray();

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

                MakeSamplesCurrentOnAllDetectorsByNumber();
            }
            catch (ArgumentException ae)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, new Handlers.ExceptionEventsArgs { Message = ae.Message, Level = NLog.LogLevel.Warn });
            }
             catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, new Handlers.ExceptionEventsArgs { Message = $"{e.Message}", Level = NLog.LogLevel.Error });
            }
        }

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

                MakeSamplesCurrentOnAllDetectorsByNumber();
            }
            catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, new Handlers.ExceptionEventsArgs { Message = $"{e.Message}", Level = NLog.LogLevel.Error });
            }
        }

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
                        throw new Exception("Count of samples more then disk can contains");
                    SpreadedSamples[ManagedDetectors[i].Name].Add(sample);
                    n++;

                    if (n >= SampleChanger.SizeOfDisk)
                    {
                        i++;
                        n = 0;
                    }
                }

                MakeSamplesCurrentOnAllDetectorsByNumber();
            }
            catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, new Handlers.ExceptionEventsArgs { Message = $"{e.Message}", Level = NLog.LogLevel.Error });
            }
        }

        public void SpreadSamplesToDetectors()
        {
            _nLogger.Info($"Spreading samples to detectors has began");
            try
            {
                if (!ManagedDetectors.Any())
                    throw new ArgumentOutOfRangeException("Session has managed no-one detector");

                if (!IrradiationList.Any())
                    throw new ArgumentOutOfRangeException("Session doesn't contain sample to measure");

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

                foreach (var d in ManagedDetectors)
                {
                    d.CurrentSample = SpreadedSamples[d.Name][0];
                    d.CurrentMeasurement = MeasurementList.Where(cm => cm.IrradiationId == d.CurrentSample.Id).First();
                    _nLogger.Info($"Samples [{(string.Join(",", SpreadedSamples[d.Name].OrderBy(ss => $"{ss.SetKey}-{ss.SampleNumber}").Select(ss => $"{ss.SetKey}-{ss.SampleNumber}").ToArray()))}] will measure on the detector {d.Name}");
                }

            }
            catch (ArgumentOutOfRangeException ae)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, new Handlers.ExceptionEventsArgs { Message = $"{ae.Message}", Level = NLog.LogLevel.Error });
            }
           catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, new Handlers.ExceptionEventsArgs { Message = $"{e.Message}", Level = NLog.LogLevel.Error });
            }
        }
    }
}
