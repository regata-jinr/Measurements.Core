/***************************************************************************
 *                                                                         *
 *                                                                         *
 * Copyright(c) 2017-2020, REGATA Experiment at FLNP|JINR                  *
 * Author: [Boris Rumyantsev](mailto:bdrum@jinr.ru)                        *
 * All rights reserved                                                     *
 *                                                                         *
 *                                                                         *
 ***************************************************************************/

using Regata.Measurements.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Regata.Measurements
{
    public partial class Session : IDisposable
    {
        public void CreateMeasurementsRegister(string name, string type)
        {
            // TODO: add record to db and dictionary
            // TODO: check if register with such pair of name and type already exist in this case add _n to the end of name
        }
        public void RenameMeasurementsRegister(string oldName, string newName)
        {
            // TODO: change record in db and dictionary
        }
        public async Task AddSampleToRegisterAsync(MeasurementInfo mi)
        {
            MeasurementsRegisters[_activeMeasurementsRegister].Add(mi);
        }

        public void AddSampleSRangeToRegisterAsync(IEnumerable<MeasurementInfo> mis)
        {
            MeasurementsRegisters[_activeMeasurementsRegister].AddRange(mis);
        }

        public async Task CopyMeasurementsRegisterAsync(string mName, string newName)
        {
            
        }
        public async Task RemoveMeasurementsRegisterAsync(string mName)
        {
            
        }

        public async Task ExportToExcelAsync(string mName)
        {
            
        }

    }
}


