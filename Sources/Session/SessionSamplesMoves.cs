using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;

namespace Measurements.Core
{
    partial class Session : ISession, IDisposable
    {
        void ISession.NextSample()
        {

        }

        void ISession.MakeSampleCurrent(IrradiationInfo ii)
        {

        }

       void ISession.PrevSample()
        {

        }
    }
}
