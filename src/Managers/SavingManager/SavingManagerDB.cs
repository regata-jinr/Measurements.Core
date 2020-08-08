using System;
using System.Linq;
using Microsoft.Data.SqlClient;
using Regata.Measurements.Models;
using System.Threading.Tasks;

namespace Regata.Measurements.Managers
{
    public static partial class SavingManager
    {
        private static async Task SaveMeasurementsInfoToDBAsync(MeasurementInfo mi)
        {
            try
            {
                AppManager.logger.Info($"Information about measurement of current sample {mi} from the detector '{mi.Detector}' will be save to the data base");
                if (AppManager.DbContext.Measurements.Where(m => m.Id == mi.Id && string.IsNullOrEmpty(m.FileSpectra)).Any())
                {
                    AppManager.DbContext.Measurements.Update(mi);
                }
                else
                {
                    await AppManager.DbContext.Measurements.AddAsync(mi);
                }
                AppManager.DbContext.SaveChanges();
            }
            catch (Exception dbe)
            {
                NotificationManager.Notify(dbe.InnerException, NotificationLevel.Error, AppManager.Sender);
                //SaveLocally(ref det);
            }
        }

        /// <summary>
        /// Generator of unique name of file spectra
        /// Name of file spectra should be unique and it has constraint in data base
        /// There is an algorithm:
        /// For the specified type it determines maximum of spectra number from the numbers that might be converted to integer number
        /// Then it choose the max number and convert it to string using next code:
        /// First digit of name spectra is the digit from the name of detector
        /// Second digit is number of type - {SLI - 0} {LLI-1 - 1} {LLI-2 - 2}
        /// The next five digits is number of spectra
        /// Typical name of spectra file: 1006261 means
        /// The spectra was acquried on detector 'D1' it was SLI type and it has a number 6261.
        /// **Pay attention that beside FileSpectra filed in MeasurementInfo**
        /// **each Detector has a property with FullName that included path on local storage**
        /// <see cref="Detector.FullFileSpectraName"/>
        /// </summary>
        /// <param name="detName">Name of detector which save acquiring session to file</param>
        /// <returns>Name of spectra file</returns>

        private static async Task<string> GenerateSpectraFileNameFromDBAsync(string dName, string type)
        {
            AppManager.logger.Info($"Generate file spectra name for the detector {dName} from DB");
            int maxNumber = 0;
            try
            {
                maxNumber = AppManager.DbContext.Measurements.Where(m =>
                                                            (
                                                                m.FileSpectra.Length == 7 &&
                                                                m.Type == type &&
                                                                IsNumber(m.FileSpectra) &&
                                                                m.FileSpectra.Substring(0, 1) == dName.Substring(1, 1)
                                                            )
                                                            ).
                                                       Select(m => new
                                                       {
                                                           FileNumber = int.Parse(m.FileSpectra.Substring(2, 5))
                                                       }
                                                                       ).
                                                       Max(m => m.FileNumber);

                return $"{dName.Substring(1, 1)}{SessionTypeMap[type]}{(++maxNumber).ToString("D5")}";
            }
            catch (SqlException sqle)
            {
                NotificationManager.Notify(sqle, NotificationLevel.Warning, AppManager.Sender);
                return await GenerateSpectraFileNameFromLocalStorageAsync(dName, type);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbe) // for duplicates
            {
                NotificationManager.Notify(dbe, NotificationLevel.Error, AppManager.Sender);
                return await GenerateSpectraFileNameFromLocalStorageAsync(dName, type);
            }
            catch (Exception e)
            {
                NotificationManager.Notify(e, NotificationLevel.Error, AppManager.Sender);
                return await GenerateSpectraFileNameFromLocalStorageAsync(dName, type);
            }
        }

    }
}

