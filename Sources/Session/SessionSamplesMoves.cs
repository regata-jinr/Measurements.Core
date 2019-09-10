using System;
using System.Linq;

namespace Measurements.Core
{
    public partial class Session : ISession, IDisposable
    {
        public bool NextSample(ref IDetector d)
        {
            try
            { 
                _nLogger.Info($"Change sample {d.CurrentSample.ToString()} to the next one for detector {d.Name}");
                int currentIndex = SpreadedSamples[d.Name].IndexOf(d.CurrentSample);
                if (currentIndex != SpreadedSamples[d.Name].Count - 1)
                {
                    d.CurrentSample = SpreadedSamples[d.Name][++currentIndex];
                    int IrId = d.CurrentSample.Id;
                    d.CurrentMeasurement = MeasurementList.Where(cm => cm.IrradiationId == IrId).First();
                    return true;
                }
            }
            catch (IndexOutOfRangeException ie)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, new Handlers.ExceptionEventsArgs { Message = ie.Message, Level = NLog.LogLevel.Warn });
            }
            catch (Exception ex)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, new Handlers.ExceptionEventsArgs { Message = ex.Message, Level = NLog.LogLevel.Error });
            }

            return false;
        }

        public void MakeSamplesCurrentOnAllDetectorsByNumber(int n = 0)
        {
            try
            {
                foreach (var d in ManagedDetectors)
                {
                    if (n < 0 || n >= SpreadedSamples[d.Name].Count)
                        throw new IndexOutOfRangeException($"For detector '{d.Name}' index out of range");

                    d.CurrentSample = SpreadedSamples[d.Name][n];
                }
            }
            catch (IndexOutOfRangeException ie)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, new Handlers.ExceptionEventsArgs { Message = ie.Message, Level = NLog.LogLevel.Warn });
            }
        }

        public void MakeSampleCurrentOnDetector(ref IrradiationInfo ii, ref IDetector d)
        {
            _nLogger.Info($"Make sample {ii.ToString()} current on detector {d.Name}");
            d.CurrentSample = ii;
        }

       public void PrevSample(ref IDetector d)
        {
            try
            {
                _nLogger.Info($"Change sample {d.CurrentSample.ToString()} to the previous one for dtector {d.Name}");
                int currentIndex = SpreadedSamples[d.Name].IndexOf(d.CurrentSample);
                if (currentIndex == 0)
                    throw new IndexOutOfRangeException($"Current sample on detector {d.Name} has 0 index. Can't go to the previous sample");
                d.CurrentSample = SpreadedSamples[d.Name][--currentIndex];
            }
            catch (IndexOutOfRangeException ie)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, new Handlers.ExceptionEventsArgs { Message = ie.Message, Level = NLog.LogLevel.Warn });
            }
            catch (Exception ex)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, new Handlers.ExceptionEventsArgs { Message = ex.Message, Level = NLog.LogLevel.Error });
            }
        }
    } // class
} // namespace
