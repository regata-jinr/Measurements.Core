using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;

namespace Measurements.Core
{
    public partial class Session : ISession, IDisposable
    {
       public void NextSample(ref IDetector d)
        {
            _nLogger.Info($"Change sample {d.CurrentSample.ToString()} to the next one for dtector {d.Name}");
            int currentIndex = SpreadedSamples[d.Name].IndexOf(d.CurrentSample);
            SpreadedSamples[d.Name].Remove(d.CurrentSample);
            if (SpreadedSamples[d.Name].Any())
                d.CurrentSample = SpreadedSamples[d.Name][++currentIndex];
            else
                MeasurementDone.Invoke(d, EventArgs.Empty);
                    
        }

        public void MakeSampleCurrentOnDetector(ref IrradiationInfo ii, ref IDetector d)
        {
            _nLogger.Info($"Make sample {ii.ToString()} current on detector {d.Name}");
            d.CurrentSample = ii;
        }

       public void PrevSample(ref IDetector d)
        {
            _nLogger.Info($"Change sample {d.CurrentSample.ToString()} to the previous one for dtector {d.Name}");
            int currentIndex = SpreadedSamples[d.Name].IndexOf(d.CurrentSample);
            d.CurrentSample = SpreadedSamples[d.Name][--currentIndex];
        }
    }
}
