using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;

namespace Measurements.Core
{
    partial class Session : ISession, IDisposable
    {
       public void NextSample()
        {
            foreach (var d in _managedDetectors)
            {
                int currentIndex = SpreadedSamples[d.Name].IndexOf(d.CurrentSample);
                d.CurrentSample = SpreadedSamples[d.Name][++currentIndex];
            }
        }

        public void MakeSampleCurrentOnDetector(ref IrradiationInfo ii, ref Detector det)
        {
            det.CurrentSample = ii;
        }

       public void PrevSample()
        {
            foreach (var d in _managedDetectors)
            {
                int currentIndex = SpreadedSamples[d.Name].IndexOf(d.CurrentSample);
                d.CurrentSample = SpreadedSamples[d.Name][--currentIndex];
            }
        }
    }
}
