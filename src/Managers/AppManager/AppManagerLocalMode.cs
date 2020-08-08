/***************************************************************************
 *                                                                         *
 *                                                                         *
 * Copyright(c) 2020, REGATA Experiment at FLNP|JINR                       *
 * Author: [Boris Rumyantsev](mailto:bdrum@jinr.ru)                        *
 * All rights reserved                                                     *
 *                                                                         *
 *                                                                         *
 ***************************************************************************/

using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Regata.Measurements.Models;

namespace Regata.Measurements.Managers
{
    public static partial class AppManager
    {
        public static event Action AppGoToLocalMode;
        public static event Action AppLeaveLocalMode;

        private static bool _localMode;
        public static bool LocalMode
        {
            get
            {
                return _localMode;
            }
            private set
            {
                _localMode = value;
                if (value)
                    AppGoToLocalMode?.Invoke();
                else
                    AppLeaveLocalMode?.Invoke();
            }
        }

        /// <summary>
        /// This internal method will be call when ConnectionRestoreEvent will occur <see cref="SessionControllerSingleton.ConectionRestoreEvent"/>
        /// It upload all files into memory via usage of desirilizer and then upload it to database.
        /// </summary>
        /// <returns>List of object with MeasurementInfo type that will be load to the data base. <seealso cref="MeasurementInfo"/></returns>
        private static List<MeasurementInfo> LoadMeasurementsFiles()
        {
            var MeasurementsInfoForUpload = new List<MeasurementInfo>();
            try
            {
                logger.Info($"Deserilization informataion inside 'D:\\MeasurementsLocalData'  has begun");
                var dir = new DirectoryInfo(@"D:\MeasurementsLocalData");

                if (!dir.Exists)
                    return MeasurementsInfoForUpload;

                var files = dir.GetFiles("*.json").ToList();
                var options = new JsonSerializerOptions();
                options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
                foreach (var file in files)
                {
                    logger.Info($"Deserilization informataion from the file '{file.Name}'");
                    MeasurementsInfoForUpload.Add(JsonSerializer.Deserialize<MeasurementInfo>(File.ReadAllText(file.FullName), options));
                }
                logger.Info($"Deserilization informataion inside 'D:\\MeasurementsLocalData'  has done");
            }
            catch (Exception e)
            {
                NotificationManager.Notify(e, NotificationLevel.Error, Sender);
            }

            return MeasurementsInfoForUpload;
        }

        public static bool CheckLocalStorage()
        {
            throw new NotImplementedException();
        }

    } // public static partial class AppManager
} // namespace Regata.Measurements.Managers
