using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;

namespace Measurements.Core
{
    partial class Session : ISession, IDisposable
    {
        private void SetIrradiationsList(DateTime date)
        {
            try
            {
                if (string.IsNullOrEmpty(Type))
                    throw new ArgumentNullException("Before choosing date of irradiations you should choose type of irradiations");

                IrradiationList.AddRange(_infoContext.Irradiations.Where(i => i.DateTimeStart.ToString("dd.MM.yyyy") == date.ToString("dd.MM.yyyy") && i.Type == Type).ToList());

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

        public void SpreadSamplesToDetectors()
        {
            try
            {
                int CountOfContainers = IrradiationList.Select(ir => ir.Container).Max().Value;
                int CountOfDetectors = ManagedDetectors.Count();
                int i = 1;

                if (ManagedDetectors.Count == 0)
                    throw new ArgumentOutOfRangeException("Session has hot managed any detector");

                if (SpreadedSamples.Count != 0)
                    SpreadedSamples.Clear();

                foreach (var d in ManagedDetectors)
                {
                    SpreadedSamples.Add(d.Name, new List<IrradiationInfo>());
                }

                for (var j = 1; j <= CountOfContainers; ++j)
                {
                    SpreadedSamples[$"D{i}"].AddRange(IrradiationList.Where(ir => ir.Container == j).ToList());
                    i++;
                    if (i > CountOfDetectors)
                        i = 1;
                }

                foreach (var d in ManagedDetectors)
                {
                    d.CurrentSample = SpreadedSamples[d.Name][0];
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

        private void SaveLocally()
        {

        }
        private void SaveRemotely()
        {

        }
 
    }
}
