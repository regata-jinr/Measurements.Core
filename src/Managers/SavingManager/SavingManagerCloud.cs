using System;
using System.Linq;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using Regata.Measurements.Models;
using System.Threading.Tasks;
using System.IO;

namespace Regata.Measurements.Managers
{
    public static partial class SavingManager
    {

        private static async Task<string> SaveSpectraFileToCloudStorageAsync(string fileName) 
        {
            if (await WebDavClientApi.UploadFile(fileName, AppManager.TimeOutCancToken))
                return await WebDavClientApi.MakeShareable(fileName, AppManager.TimeOutCancToken);
            return "";
        }
    }
}
