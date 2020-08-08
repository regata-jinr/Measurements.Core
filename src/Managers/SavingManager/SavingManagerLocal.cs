using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Regata.Measurements.Models;
using System.Threading.Tasks;
using System.IO;

namespace Regata.Measurements.Managers
{
    public static partial class SavingManager
    {


        private static async Task SaveSpectraFileToLocalStorageAsync(string sessionName, string dName, string fName)
        {

        }


        private static async Task SaveMeasurementsInfoToLocalStorageAsync(MeasurementInfo mi) { }


        private static async Task<string> GenerateSpectraFileNameFromLocalStorageAsync(string dName, string type)
        {
            AppManager.logger.Info($"Generate file spectra name  for the detector {dName} from local storage");

            int maxNumber = 0;
            try
            {
                if (!Directory.Exists(@"D:\Spectra"))
                    throw new Exception("Spectra Directory doesn't exist");

                var dir = new DirectoryInfo(@"D:\Spectra");
                var files = dir.GetFiles("*.cnf", SearchOption.AllDirectories).Where(f => f.CreationTime >= DateTime.Now.AddDays(-30)).ToList();

                maxNumber = files.Where(f =>
                                            f.Name.Length == 11 &&
                                            f.Name.Substring(1, 1) == SessionTypeMap[type].ToString() &&
                                            IsNumber(Path.GetFileNameWithoutExtension(f.Name)) &&
                                            f.Name.Substring(0, 1) == dName.Substring(1, 1)
                                       ).
                                  Select(f => new
                                  {
                                      FileNumber = int.Parse(f.Name.Substring(2, 5))
                                  }
                                        ).
                                  Max(f => f.FileNumber);

                return $"{dName.Substring(1, 1)}{SessionTypeMap[type]}{(++maxNumber).ToString("D5")}";
            }
            catch (Exception e)
            {
                NotificationManager.Notify(e, NotificationLevel.Error, AppManager.Sender);
                return "";
            }

        }

        private static void SaveMeasurementInfoLocally(MeasurementInfo mi)
        {
            try
            {
                AppManager.logger.Info($"Seriliazation of sample '{mi}' has begun");
                var options = new JsonSerializerOptions();
                options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
                options.WriteIndented = true;

                var dir = new DirectoryInfo(@"D:\MeasurementsLocalData");

                if (!dir.Exists)
                    dir.Create();

                File.WriteAllText(Path.Combine(dir.FullName, $"{mi}-{mi.FileSpectra}", ".json"), JsonSerializer.Serialize(mi));
                AppManager.logger.Info($"Seriliazation of sample '{mi}' has done");
            }
            catch (Exception e)
            {
                NotificationManager.Notify(e, NotificationLevel.Error, AppManager.Sender);
            }
        }

    }
}

