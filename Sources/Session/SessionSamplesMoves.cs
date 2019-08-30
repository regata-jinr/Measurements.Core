using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;

namespace Measurements.Core
{
    partial class Session : ISession, IDisposable
    {
       public void NextSample(ref IDetector d)
        {
                int currentIndex = SpreadedSamples[d.Name].IndexOf(d.CurrentSample);

                if (SpreadedSamples[d.Name].Count - 1 != currentIndex)
                    d.CurrentSample = SpreadedSamples[d.Name][++currentIndex];
                // else // TODO: notify that this detector done and check are all detectors is done?
                    
        }

        public void MakeSampleCurrentOnDetector(ref IrradiationInfo ii, ref IDetector det)
        {
            det.CurrentSample = ii;
        }

       public void PrevSample(ref IDetector d)
        {
                int currentIndex = SpreadedSamples[d.Name].IndexOf(d.CurrentSample);
                d.CurrentSample = SpreadedSamples[d.Name][--currentIndex];
        }
    }
}
