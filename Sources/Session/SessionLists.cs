using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        //TODO: add event in case of sample number increase the size of disk, but not break the measurements!
        private void SpreadLLISamplesToDetectors()
        {
            try
            {
                var NumberOfContainers = IrradiationList.Select(ir => ir.Container).Distinct().ToArray();
                int i = 0;

                foreach (var conNum in NumberOfContainers)
                {
                    var sampleList = new List<IrradiationInfo> (IrradiationList.Where(ir => ir.Container == conNum).ToList());
                    SpreadedSamples[ManagedDetectors[i].Name].AddRange(sampleList);
                    _nLogger.Info($"Samples {sampleList.First().SetKey}-[{(string.Join(",", sampleList))}] will measure on the detector {ManagedDetectors[i].Name}");
                    i++;
                    if (i > ManagedDetectors.Count())
                        i = 0;
                }

                foreach (var d in ManagedDetectors)
                    d.CurrentSample = SpreadedSamples[d.Name][0];
            }
            catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, new Handlers.ExceptionEventsArgs { Message = $"{e.Message}", Level = NLog.LogLevel.Error });
            }
        }

        //TODO: add option for spreading (for lli by containers, or uniform, or by the order)
        // uniform means - count all samples, then divide to count of detectors (pay attention to boundaries)
        // by the order means first {sizeOfDisk} to first detector so on....
        private void SpreaSLISampleToDetectors()
        {
            try
            {

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

                if (Type.Contains("LLI"))
                    SpreadLLISamplesToDetectors();
                else if (Type.Contains("SLI"))
                    SpreaSLISampleToDetectors();
                else
                    throw new Exception("Type of measurement was not recognized by the program. Use only 'SLI', 'LLI-1', 'LLI-2'"); 

                foreach (var d in ManagedDetectors)
                    d.CurrentSample = SpreadedSamples[d.Name][0];
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

        private void SaveLocally()
        {

        }
        private void SaveRemotely()
        {

        }
 
    }
}
