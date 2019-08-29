using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;

namespace Measurements.Core
{
    partial class Session : ISession, IDisposable
    {
       public void NextSample(ref Detector d)
        {
                int currentIndex = SpreadedSamples[d.Name].IndexOf(d.CurrentSample);
                if (SpreadedSamples[d.Name].Count - 1 != currentIndex)
                    d.CurrentSample = SpreadedSamples[d.Name][++currentIndex];
                // TODO: else notify that this detector done and check are all detectors is done?
                    
        }

        public void MakeSampleCurrentOnDetector(ref IrradiationInfo ii, ref Detector det)
        {
            det.CurrentSample = ii;
        }

       public void PrevSample(ref Detector d)
        {
                int currentIndex = SpreadedSamples[d.Name].IndexOf(d.CurrentSample);
                d.CurrentSample = SpreadedSamples[d.Name][--currentIndex];
        }
    }
}
