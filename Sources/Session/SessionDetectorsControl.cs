using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Measurements.Core
{
    public partial class Session : ISession, IDisposable
    {

        public void StartMeasurements()
        {
            _nLogger.Info($"starts measurements of the sample sets");

            try
            {
                if (Counts <= 0 || string.IsNullOrEmpty(Type) || IrradiationList.Count == 0)
                    throw new ArgumentException($"Either some of principal arguments doesnt assign: Duration={Counts}, type of measurements={Type} or list of samples is empty {IrradiationList.Count}");

                SpreadSamplesToDetectors();

                foreach (var d in ManagedDetectors)
                {
                    if (SpreadedSamples[d.Name].Count != 0)
                        d.Start();
                    else
                    {
                        Handlers.ExceptionHandler.ExceptionNotify(this, new Exception($"For the detector '{d.Name}' list of samples is empty"), NLog.LogLevel.Warn);
                        MeasurementDone?.Invoke(d, EventArgs.Empty);
                    }
                }
            }
            catch (ArgumentOutOfRangeException are)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, are, NLog.LogLevel.Warn);
            }
            catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, e, NLog.LogLevel.Error);
            }
        
        }
        //TODO: wrap all operation to try - catch. In the other case it will be not possible to continue working with application.
        public void StopMeasurements()
        {
            try
            {
                _nLogger.Info($"stops measurements by user command");
                foreach (var d in ManagedDetectors)
                    d.Stop();
            }
            catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, e, NLog.LogLevel.Error);
            }
        }

        public void SaveSpectra(ref IDetector d)
        {
            try
            {
                d.CurrentMeasurement.FileSpectra = GenerateFileSpectraName(d.Name);
            _nLogger.Info($"Detector {d.Name} will save spectra to file with name - '{d.CurrentMeasurement.FileSpectra}'");
                d.Save();
            }
            catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, e, NLog.LogLevel.Error);
            };
        }

       public void ContinueMeasurements()
        {
            try
            {
                _nLogger.Info($"will continue measurements by user command");
                foreach (var d in ManagedDetectors)
                    d.Start();
            }
            catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, e, NLog.LogLevel.Error);
            };
        }
        public void PauseMeasurements()
        {
            try
            {
                _nLogger.Info($"will pause measurements by user command");
                foreach (var d in ManagedDetectors)
                    d.Pause();
            }
            catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, e, NLog.LogLevel.Error);
            };
        }
        public void ClearMeasurements()
        {
            try
            {
                _nLogger.Info($"will clear measurements by user command");
                foreach (var d in ManagedDetectors)
                    d.Clear();
            }
            catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, e, NLog.LogLevel.Error);
            };
        }

       public void AttachDetector(string dName)
        {

            _nLogger.Info($"will take a control for detector '{dName}'");
            try
            {
                if (SessionControllerSingleton.AvailableDetectors == null || SessionControllerSingleton.AvailableDetectors.Count == 0)
                    throw new InvalidOperationException();

                var det = SessionControllerSingleton.AvailableDetectors.Find(d => d.Name == dName);
                if (det != null)
                {

                    ManagedDetectors.Add(det);
                    SpreadedSamples.Add(det.Name, new List<IrradiationInfo>());
                    SessionControllerSingleton.AvailableDetectors.Remove(det);
                    det.AcquiringStatusChanged += ProcessAcquiringMessage;
                    _nLogger.Info($"successfuly attached detector {det.Name}");
                }
                else
                    throw new ArgumentNullException(dName);
            }
            catch (ArgumentNullException ae)
            {
                //TODO: russian language returns "??????? ..."
                Handlers.ExceptionHandler.ExceptionNotify(this, ae, NLog.LogLevel.Error);
            }
            catch (InvalidOperationException ie)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, ie, NLog.LogLevel.Error);
            }
            catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, e, NLog.LogLevel.Error);
            }
        }
        public void DetachDetector(string dName)
        {
            _nLogger.Info($"will detach detector 'dName'");
            try
            {
                if (ManagedDetectors == null && ManagedDetectors.Count == 0)
                throw new InvalidOperationException();

                var det = ManagedDetectors.Find(d => d.Name == dName);
                if (det != null)
                {
                    SessionControllerSingleton.AvailableDetectors.Add(det);
                    SpreadedSamples.Remove(det.Name);
                    ManagedDetectors.Remove(det);
                    _nLogger.Info($"Successfuly detached detector {det.Name}");
                }
                else
                    throw new ArgumentNullException();
            }
            catch (ArgumentNullException ae)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, ae, NLog.LogLevel.Error);
            }
            catch (InvalidOperationException ie)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, ie, NLog.LogLevel.Error);
            }
            catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, e, NLog.LogLevel.Error);
            }
        }

        private void MeasurementDoneHandler(Object detObj, EventArgs eventArgs)
        {

                _nLogger.Info($"Detector {((Detector)detObj).Name} has done measurement process");
                _countOfDetectorsWichDone++;

            if (_countOfDetectorsWichDone == ManagedDetectors.Count)
            {
                _nLogger.Info($"All detectors [{(string.Join(",", ManagedDetectors.Select(d => d.Name).ToArray()))}] has done measurement process");
                _countOfDetectorsWichDone = 0;
                SessionComplete?.Invoke(this, eventArgs);
            }
        }

       private void ProcessAcquiringMessage(object o, DetectorEventsArgs args)
       {
            try
            {
                if (o is Detector)
                {
                    IDetector d = (Detector) o;

                    if (d.Status == DetectorStatus.ready && args.AcquireMessageParam == (int)CanberraDeviceAccessLib.AdviseMessageMasks.amAcquireDone)
                    {
                        SaveSpectra(ref d);
                        SaveMeasurement(ref d);
                        if (NextSample(ref d))
                            d.Start();
                        else MeasurementDone?.Invoke(d, EventArgs.Empty);
                    }
                }
                else
                    throw new ArgumentException("Object has a wrong type");
            }
            catch (ArgumentException ae)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, ae, NLog.LogLevel.Error);
            }
            catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, e, NLog.LogLevel.Error);
            }
       }

        private string GenerateFileSpectraName(string detName)
        {
            _nLogger.Info($"will generate file spectra name for the detector {detName}");
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

                return $"{detName.Substring(1, 1)}{typeDict[Type]}{(++maxNumber).ToString("D5")}";
            }
            catch (System.Data.SqlClient.SqlException sqle)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, sqle, NLog.LogLevel.Warn);
            
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

                return $"{detName.Substring(1,1)}{typeDict[Type]}{(++maxNumber).ToString("D5")}";
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbe) // for duplicates
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, dbe, NLog.LogLevel.Error);
            }
            catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, e, NLog.LogLevel.Error);
            }
            return $"{detName[1]}{typeDict[Type]}{maxNumber}";
        }

        private bool IsNumber(string str)
        {
            return int.TryParse(str, out _);
        }
   }
}
