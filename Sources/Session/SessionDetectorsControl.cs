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
        private void SaveLocally()
        {

        }
        private void SaveRemotely()
        {

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
   }
}
