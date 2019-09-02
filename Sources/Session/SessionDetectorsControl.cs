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
                SpreadSamplesToDetectors();

                foreach (var d in ManagedDetectors)
                {
                    if (SpreadedSamples[d.Name].Count == 0) // FIXME: how would be with situations: 1. Only one detector not spreaded (e.g. not enough samples for choosing detectors)
                        throw new ArgumentOutOfRangeException($"Detector {d.Name} doesn't contain samples to measure. Before the acquiring distribute samples on detectors. Perhaps amount of samples not enough to fill all detectors. You could detach empty detector {d.Name}.");
                    // FIXME: I don't like such logic, also it should allow redefinition of some measurement parameters
                    d.CurrentMeasurement.Height = Height;
                    _nLogger.Info($"Height {Height} has specified for sample {d.CurrentMeasurement.ToString()} on detector {d.Name}");
                    d.CurrentMeasurement.Type = Type;
                    d.Start();
                }
            }
            catch (ArgumentOutOfRangeException are)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, new Handlers.ExceptionEventsArgs { Message = are.Message, Level = NLog.LogLevel.Warn });
            }
        }
        public void StopMeasurements()
        {
            _nLogger.Info($"stops measurements by user command");
            foreach (var d in ManagedDetectors)
                d.Stop();
        }

        public void SaveSpectra(ref IDetector d)
        {
            d.CurrentMeasurement.FileSpectra = GenerateFileSpectraName(d.Name);
            _nLogger.Info($"Detector {d.Name} will save spectra to file with name - '{d.CurrentMeasurement.FileSpectra}'");
            d.Save();
        }

       public void ContinueMeasurements()
        {
            _nLogger.Info($"will continue measurements by user command");
            foreach (var d in ManagedDetectors)
                d.Start();
        }
        public void PauseMeasurements()
        {
            _nLogger.Info($"will pause measurements by user command");
            foreach (var d in ManagedDetectors)
                d.Pause();
        }
        public void ClearMeasurements()
        {
            _nLogger.Info($"will clear measurements by user command");
            foreach (var d in ManagedDetectors)
                d.Clear();
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
            catch (ArgumentNullException)
            {
                //TODO: russian language returns "??????? ..."
                Handlers.ExceptionHandler.ExceptionNotify(this, new Handlers.ExceptionEventsArgs { Message = $"Detector '{dName}' was not found in the list of available detectors for the session. Either it already in use or it has hardware error", Level = NLog.LogLevel.Error });
            }
            catch (InvalidOperationException)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, new Handlers.ExceptionEventsArgs { Message = $"All detectors already in use or no-one is not available. Please, check connection with the detector by your hand", Level = NLog.LogLevel.Error });
            }
            catch (Exception ex)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, new Handlers.ExceptionEventsArgs { Message = $"{ex.Message}", Level = NLog.LogLevel.Error });
            }
        }
        public void DetachDetector(string dName)
        {
            _nLogger.Info($"will detach detector 'dName'");
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
                    SpreadedSamples.Remove(det.Name);
                    ManagedDetectors.Remove(det);
                    _nLogger.Info($"successfuly detached detector {det.Name}");
                }
                else
                    throw new ArgumentNullException();
            }
            catch (ArgumentNullException)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, new Handlers.ExceptionEventsArgs { Message = $"Detector '{dName}' was not found in the list of detectors which managed by the session. The most probably you didn't add it", Level = NLog.LogLevel.Error });
            }
            catch (InvalidOperationException)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, new Handlers.ExceptionEventsArgs { Message = $"Detector with specified name '{dName}' doesn't exist in the list of detectos which managed by the session", Level = NLog.LogLevel.Error });
            }
            catch (Exception ex)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, new Handlers.ExceptionEventsArgs { Message = $"{ex.Message}", Level = NLog.LogLevel.Error });
            }
        }


        //TODO: parse messages correctly it's not only done message
        private void MeasurementDoneHandler(Object detObj, EventArgs eventArgs)
        {

            if (_countOfDetectorsWichDone == ManagedDetectors.Count)
            {
                _nLogger.Info($"Detector {((Detector)detObj).Name} has done measurement process");
                SessionComplete?.Invoke(this, eventArgs);
                _countOfDetectorsWichDone = 0;
            }
            else
            {
                _nLogger.Info($"All detectors [{(string.Join(",", ManagedDetectors.Select(d => d.Name).ToArray()))}] has done measurement process");
                _countOfDetectorsWichDone++;
            }
        }

        private void ProcessAcquiringMessage(object o, EventArgs args)
        {
            try
            {
                if (o is Detector && args is DetectorEventsArgs)
                {
                    //FIXME: how to avoid boxing gere? to use my own delegate?
                    IDetector d = (Detector) o;
                    DetectorEventsArgs darg = (DetectorEventsArgs) args;

                    if (d.Status == DetectorStatus.ready)
                    {
                        SaveSpectra(ref d);
                        NextSample(ref d);
                        if (SpreadedSamples[d.Name].Any())
                            d.Start();
                        else
                            MeasurementDone?.Invoke(d, EventArgs.Empty);
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

                return $"{detName.Substring(1,1)}{typeDict[Type]}{maxNumber}.cnf";
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
            return int.TryParse(str, out _);
        }
   }
}
