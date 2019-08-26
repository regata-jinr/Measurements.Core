using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;

namespace Measurements.Core
{
    partial class Session : ISession, IDisposable
    {
        //for each detector task run (or await) start measure queue
        public void Start()
        {
                foreach (var d in _managedDetectors)
                    d.Start();
        }
        public void Stop()
        {
            foreach (var d in _managedDetectors)
                d.Stop();

        }
        //if connection closed save locally to json check if json exists
        public void Save()
        {
            foreach (var d in _managedDetectors)
                d.Save();

        }

       public void Continue()
        {
            foreach (var d in _managedDetectors)
                d.Continue();
        }
        public void Pause()
        {
            foreach (var d in _managedDetectors)
                d.Pause();
        }
        public void Clear()
        {
            foreach (var d in _managedDetectors)
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
                    _managedDetectors.Add(det);
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
                if (_managedDetectors == null && _managedDetectors.Count == 0)
                throw new InvalidOperationException();

                // todo: check should i disconnect detector before removing?

                var det = _managedDetectors.Find(d => d.Name == dName);
                if (det != null)
                {
                    det.Dispose(); //?
                    SessionControllerSingleton.AvailableDetectors.Add(det);
                    _managedDetectors.Remove(det);
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
                    var d = (Detector) o;
                    if (d.Status == DetectorStatus.ready)
                    {
                        Save();
                        NextSample();
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

        //TODO: move to Detector class
        private string GenerateFileSpectraName(string detName)
        {
            var typeDict = new Dictionary<string, string> { {"SLI", "0"}, {"LLI-1", "1"}, {"LLI-2", "2"} };
            int maxNumber = 0;
            try
            {
                maxNumber = _measurementInfoContext.Measurements.Where(m =>
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
            }
            catch (System.Data.SqlClient.SqlException sqle)
            {
                //TODO: search files in D drive
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbe) // for duplications
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
