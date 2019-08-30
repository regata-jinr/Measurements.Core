using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Measurements.Core
{
    partial class Session : ISession, IDisposable
    {
        //for each detector task run (or await) start measure queue
        public void StartMeasurements()
        {
            foreach (var d in ManagedDetectors)
                d.Start();
        }
        public void StopMeasurements()
        {
            foreach (var d in ManagedDetectors)
                d.Stop();

        }

        public void SaveSpectra(ref IDetector d)
        {
                d.CurrentMeasurement.FileSpectra = GenerateFileSpectraName(d.Name);
                d.Save();
        }

       public void ContinueMeasurements()
        {
            foreach (var d in ManagedDetectors)
                d.Continue();
        }
        public void PauseMeasurements()
        {
            foreach (var d in ManagedDetectors)
                d.Pause();
        }
        public void ClearMeasurements()
        {
            foreach (var d in ManagedDetectors)
                d.Clear();
        }

       public void AttachDetector(string dName)
        {
            try
            {
                if (SessionControllerSingleton.AvailableDetectors == null || SessionControllerSingleton.AvailableDetectors.Count == 0)
                    throw new InvalidOperationException();

                var det = SessionControllerSingleton.AvailableDetectors.Find(d => d.Name == dName);
                if (det != null)
                {
                    ManagedDetectors.Add(det);
                    SessionControllerSingleton.AvailableDetectors.Remove(det);
                    det.AcquiringStatusChanged += ProcessAcquiringMessage;
                }
                else
                    throw new ArgumentNullException(dName);
            }
            catch (ArgumentNullException)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, new Handlers.ExceptionEventsArgs { Message = $"Детектор '{dName}' не найден в списке доступных детекторов для панели управления сессиями. Либо он уже используется либо он не доступен.", Level = NLog.LogLevel.Error });
            }
            catch (InvalidOperationException)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, new Handlers.ExceptionEventsArgs { Message = $"Все детекторы уже заняты в других сессиях или не доступны по причине проблем с оборудованием. Проверьте соединения с детекторами вручную.", Level = NLog.LogLevel.Error });
            }
            catch (Exception ex)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, new Handlers.ExceptionEventsArgs { Message = $"{ex.Message}", Level = NLog.LogLevel.Error });
            }
        }
        public void DetachDetector(string dName)
        {
            try
            {
                if (ManagedDetectors == null && ManagedDetectors.Count == 0)
                throw new InvalidOperationException();

                // todo: check should i disconnect detector before removing?

                var det = ManagedDetectors.Find(d => d.Name == dName);
                if (det != null)
                {
                    det.Dispose(); //?
                    SessionControllerSingleton.AvailableDetectors.Add(det);
                    ManagedDetectors.Remove(det);
                }
                else
                    throw new ArgumentNullException();
            }
            catch (ArgumentNullException)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, new Handlers.ExceptionEventsArgs { Message = $"Детектор '{dName}' не найден в списке доступных для сессии детекторов. Скорее всего он не был добавлен.", Level = NLog.LogLevel.Error });
            }
            catch (InvalidOperationException)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, new Handlers.ExceptionEventsArgs { Message = $"Детектор с заданным именем '{dName}' отсутствует в списке детекторов сессии.", Level = NLog.LogLevel.Error });
            }
            catch (Exception ex)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, new Handlers.ExceptionEventsArgs { Message = $"{ex.Message}", Level = NLog.LogLevel.Error });
            }
        }

        private void ProcessAcquiringMessage(object o, EventArgs args)
        {
            try
            {
                if (o is Detector)
                {
                    IDetector d = (Detector) o;
                    if (d.Status == DetectorStatus.ready)
                    {
                        SaveSpectra(ref d);
                        NextSample(ref d);
                        d.Start();
                    }
                }
                else
                    throw new ArgumentException("Object has a wrong type");
            }
            catch (ArgumentException ar)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, new Handlers.ExceptionEventsArgs { Message = $"{ar.Message}", Level = NLog.LogLevel.Error });
            }
            catch (Exception ex)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, new Handlers.ExceptionEventsArgs { Message = $"{ex.Message}", Level = NLog.LogLevel.Error });
            }
        }

        private string GenerateFileSpectraName(string detName)
        {
            var typeDict = new Dictionary<string, string> { {"SLI", "0"}, {"LLI-1", "1"}, {"LLI-2", "2"} };
            int maxNumber = 0;
            try
            {
                maxNumber = _infoContext.Measurements.Where(m =>
                                                            (
                                                                m.FileSpectra.Length == 7 &&
                                                                m.Type == Type &&
                                                                IsNumber(m.FileSpectra) &&
                                                                m.FileSpectra.Substring(0, 1) == detName.Substring(1, 1)
                                                            )
                                                            ).
                                                       Select(m => new
                                                                 {
                                                                     FileNumber = int.Parse(m.FileSpectra.Substring(3, 4))
                                                                 }
                                                                       ).
                                                       Max(m => m.FileNumber);

                return $"{detName.Substring(1,1)}{typeDict[Type]}{maxNumber}";
            }
            catch (System.Data.SqlClient.SqlException sqle)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, new Handlers.ExceptionEventsArgs { Message = $"{sqle.InnerException.Message}{Environment.NewLine}Program will try to generate new file name from existing spectra file in 'D:\\Spectra'", Level = NLog.LogLevel.Warn });

                if (!Directory.Exists(@"D:\Spectra"))
                    throw new Exception("Spectra Directory doesn't exist");

                var dir = new DirectoryInfo(@"D:\Spectra");
                var files = dir.GetFiles("*.cnf", SearchOption.AllDirectories).Where(f => f.CreationTime >= DateTime.Now.AddDays(-30)).ToList();

                maxNumber = files.Where(f => 
                                            f.Name.Length == 7 &&
                                            f.Name.Substring(1,1) == typeDict[Type] &&
                                            IsNumber(f.Name) &&
                                            f.Name.Substring(0,1) == detName.Substring(1,1)
                                       ).
                                  Select(f => new
                                                {
                                                    FileNumber = int.Parse(f.Name.Substring(3,4))
                                                }
                                        ).
                                  Max(f => f.FileNumber);

                return $"{detName.Substring(1,1)}{typeDict[Type]}{maxNumber}";
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbe) // for duplicates
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, new Handlers.ExceptionEventsArgs { Message = $"{dbe.InnerException.Message}", Level = NLog.LogLevel.Error });
            }
            catch (Exception ex)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, new Handlers.ExceptionEventsArgs { Message = $"{ex.Message}", Level = NLog.LogLevel.Error });
            }
            return $"{detName[1]}{typeDict[Type]}{maxNumber}";
        }

        private bool IsNumber(string str)
        {
            int a = 0;
            return int.TryParse(str, out a);
        }
   }
}
