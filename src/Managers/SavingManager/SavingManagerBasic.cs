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
        public static async Task SaveAsync(string sessionName, string dName)
        {
            AppManager.logger.Info($"Start saving spectra for session '{sessionName}' and detector '{dName}'");

            if (!AppManager.ActiveSessions.ContainsKey(sessionName))
            {
                var e = new KeyNotFoundException($"List of active session doesn't contain session with name'{sessionName}'");
                NotificationManager.Notify(e, NotificationLevel.Error, AppManager.Sender);
                throw e;
            }

            if (!AppManager.ActiveSessions[sessionName].ManagedDetectors.ContainsKey(dName))
            {
                var e = new KeyNotFoundException($"Session '{sessionName}' doesn't control the detector '{dName}'");
                NotificationManager.Notify(e, NotificationLevel.Error, AppManager.Sender);
                throw e;
            }

            var spectraName = Guid.NewGuid().ToString().Split('-')[0];

            var currSes = AppManager.ActiveSessions[sessionName];
            var currDet = AppManager.ActiveSessions[sessionName].ManagedDetectors[dName];
            var currMeas = AppManager.ActiveSessions[sessionName].ManagedDetectors[dName].CurrentMeasurement;

            if (currMeas.ToString() == "-")
            {
                NotificationManager.Notify(new Exception($"Session '{sessionName}' has empty sample on the detector '{dName}'"), NotificationLevel.Error, AppManager.Sender);
            }
            else
                spectraName += $"_{currMeas}";

            try
            {
                await SaveSpectraFileToLocalStorageAsync(sessionName, dName, spectraName);

                var genSpectraName = await GetSpectraFileNameAsync(dName, currSes.Type);

                File.Move(currDet.FullFileSpectraName, currDet.FullFileSpectraName.Replace(spectraName, genSpectraName));

                currMeas.Token = await SaveSpectraFileToCloudStorageAsync(currDet.FullFileSpectraName);

                if (string.IsNullOrEmpty(currMeas.Token))
                {
                    NotificationManager.Notify(new Exception($"SavingManager can't provide cloud token for the spectra file '{currDet.FullFileSpectraName}' for the session '{sessionName}' and the detector '{dName}'"), NotificationLevel.Error, AppManager.Sender);
                }

                await SaveMeasurementsInfoToDBAsync(currMeas);
            }
            catch (IOException ioe)
            {
                NotificationManager.Notify(ioe, NotificationLevel.Error, AppManager.Sender);
            }
            //catch (LocalFileSavingException lfse)
            //{

            //}
            //catch (GenerateFileException gfe)
            //{
            //    var e = new Exception($"Can't genereta name of file spectra for session '{sessionName}' and detector '{dName}'. File was saved anyway with the name '{currDet.FullFileSpectraName}'", gfe);
            //    NotificationManager.Notify(gfe, NotificationLevel.Error, AppManager.Sender);
            //}

        }

        private static async Task<string> GetSpectraFileNameAsync(string dName, string type)
        {
            try
            {
                if (await AppManager.IsDbConnectedAsync())
                    return await GenerateSpectraFileNameFromDBAsync(dName, type);
            }
            catch { }
            return await GenerateSpectraFileNameFromLocalStorageAsync(dName, type);
        }

        private static readonly IReadOnlyDictionary<string, int> SessionTypeMap = new Dictionary<string, int> { { "SLI", 0 }, { "LLI-1", 1 }, { "LLI-2", 2 } };

        private static bool IsNumber(string str)
        {
            return int.TryParse(str, out _);
        }
    }
}
