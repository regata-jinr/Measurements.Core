/***************************************************************************
 *                                                                         *
 *                                                                         *
 * Copyright(c) 2017-2019, REGATA Experiment at FLNP|JINR                  *
 * Author: [Boris Rumyantsev](mailto:bdrum@jinr.ru)                        *
 * All rights reserved                                                     *
 *                                                                         *
 *                                                                         *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Measurements.Core
{
    // this file contains methods that related with managing of detector
    // Session class divided by few files:
    // ├── ISession.cs                - interface of Session class
    // ├── SessionDetectorsControl.cs --> opened
    // ├── SessionInfo.cs             - contains fields of Session for EF core interaction
    // └── SessionMain.cs             - contains general fields and methods of the Session class.

    public partial class Session : ISession, IDisposable
    {
        /// <summary>
        /// Start asquisition process on all managed detectors by the session
        /// </summary>
        public void StartMeasurements()
        {
            try
            {
                foreach (var det in ManagedDetectors)
                {
                    if (det.CurrentMeasurement == null || det.RelatedIrradiation == null)
                    {
                        Handlers.ExceptionHandler.ExceptionNotify(this, new ArgumentNullException($"User should initialize sample on the detector {det.Name} before start of measurement"), Handlers.ExceptionLevel.Warn);
                        break;
                    }

                    _nLogger.Info($"Session will start measurements of the sample {det.CurrentMeasurement.ToString()} on detector {det.Name}");
                    det.Start();
                }
            }
            catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, e, Handlers.ExceptionLevel.Error);
            }
        }

        ///<summary>
        /// Stops asquisition process on all managed detectors by the session. 
        /// **This generate acqusition done event**
        /// </summary>.
        public void StopMeasurements()
        {
            try
            {
                _nLogger.Info($"Session will stop measurements by user command");
                foreach (var d in ManagedDetectors)
                    d.Stop();
            }
            catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, e, Handlers.ExceptionLevel.Error);
            }
        }

        public void StopMeasurementsOnDetector(ref IDetector det)
        {
            try
            {
                _nLogger.Info($"Session will stop measurements on detector {det.Name} by user command");
                    det.Stop();
            }
            catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, e, Handlers.ExceptionLevel.Error);
            }
        }

        ///<summary>
        /// Save asquisition process on specified detector into the cnf file
        /// Name of file will generate automatically <see cref="GenerateFileSpectraName(string)"/>
        /// </summary>.
        public void SaveSpectraOnDetectorToFile(ref IDetector d)
        {
            try
            {
                if (!string.IsNullOrEmpty(d.CurrentMeasurement.FileSpectra))
                    Handlers.ExceptionHandler.ExceptionNotify(this, new ArgumentException($"Current sample '{d.CurrentMeasurement}' from detector {d.Name} already has the name of FileSpectra - '{d.CurrentMeasurement.FileSpectra}'. New file will be create and new record will be added to db, the old one will not modify."), Handlers.ExceptionLevel.Warn);


                d.CurrentMeasurement.FileSpectra = GenerateFileSpectraName(d.Name);
                _nLogger.Info($"Detector {d.Name} will save spectra to file with name - '{d.CurrentMeasurement.FileSpectra}'");
                d.Save();
            }
            catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, e, Handlers.ExceptionLevel.Error);
            };
        }

        public void SaveSpectraOnDetectorToFileAndDataBase(ref IDetector d)
        {
            try
            {
                SaveSpectraOnDetectorToFile(ref d);
                SaveMeasurement(ref d);
            }
            catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, e, Handlers.ExceptionLevel.Error);
            };
        }

        ///<summary>
        /// Continue asquisition process on all managed detectors by the session
        /// </summary>.
        public void ContinueMeasurements()
        {
            try
            {
                _nLogger.Info($"Session will continue measurements on all detectors by user command");
                foreach (var d in ManagedDetectors)
                    d.Start();
            }
            catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, e, Handlers.ExceptionLevel.Error);
            };
        }
       
        ///<summary>
        /// Pause asquisition process on all managed detectors by the session
        /// In case you just stop acquisition and then continue use this method
        /// </summary>.
        public void PauseMeasurements()
        {
            try
            {
                _nLogger.Info($"Session will pause measurements on all detectors by user command");
                foreach (var d in ManagedDetectors)
                    d.Pause();
            }
            catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, e, Handlers.ExceptionLevel.Error);
            };
        }

        ///<summary>
        /// Clear asquisition process on all managed detectors by the session
        /// </summary>.
        public void ClearMeasurements()
        {
            try
            {
                _nLogger.Info($"Session will clear measurements on all detectors by user command");
                foreach (var d in ManagedDetectors)
                    d.Clear();
            }
            catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, e, Handlers.ExceptionLevel.Error);
            };
        }

        private event Action DetectorsListsChanged;

        /// <summary>
        /// Attach detector defined by the name to the session
        /// Chosen detector will remove from available detectors list that SessionControllerSingleton controled
        /// </summary>
        /// <param name="dName">Name of detector</param>
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
                    SessionControllerSingleton.AvailableDetectors.Remove(det);
                    det.AcquiringStatusChanged += ProcessAcquiringMessage;
                    _nLogger.Info($"successfuly attached detector {det.Name}");
                    DetectorsListsChanged?.Invoke();
                }
                else
                    throw new ArgumentNullException($"{dName}. The most probably you are already use this detector");
            }
            catch (ArgumentNullException ae)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, ae, Handlers.ExceptionLevel.Error);
            }
            catch (InvalidOperationException ie)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, ie, Handlers.ExceptionLevel.Error);
            }
            catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, e, Handlers.ExceptionLevel.Error);
            }
        }
        /// <summary>
        /// Remove detector given by name from list of managed detectors by the session
        /// Such detector will add to the list of available detectors that controlled by SessionControllerSingleton
        /// </summary>
        /// <param name="dName">Name of detector</param>
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
                    ManagedDetectors.Remove(det);
                    det.AcquiringStatusChanged -= ProcessAcquiringMessage;
                    _nLogger.Info($"Successfuly detached detector {det.Name}");
                    DetectorsListsChanged?.Invoke();
                }
                else
                    throw new ArgumentNullException();
            }
            catch (ArgumentNullException ae)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, ae, Handlers.ExceptionLevel.Error);
            }
            catch (InvalidOperationException ie)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, ie, Handlers.ExceptionLevel.Error);
            }
            catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, e, Handlers.ExceptionLevel.Error);
            }
        }

        /// <summary>
        /// This is the handler of MeasurementDone events. In case of detector has done measurements process, 
        /// this method add counter <see cref="_countOfDetectorsWichDone"/> and will check it with number of managed detectors.
        /// In case of matching <see cref="SessionComplete"/> will invoke.
        /// </summary>
        /// <param name="det">Detector that generated event of measurements has done</param>
        /// <param name="eventArgs"></param>
        private void MeasurementDoneHandler(string detName)
        {
            _nLogger.Info($"Detector {detName} has done measurement process");
            _countOfDetectorsWichDone++;

            if (_countOfDetectorsWichDone == ManagedDetectors.Count)
            {
                _nLogger.Info($"All detectors [{(string.Join(",", ManagedDetectors.Select(d => d.Name).ToArray()))}] has done measurement process");
                _countOfDetectorsWichDone = 0;
                SessionComplete?.Invoke();
            }
        }

        /// <summary>
        /// This internal method process message from the detector. <see cref="Detector.ProcessDeviceMessages(int, int, int)"/>
        /// </summary>
        /// <param name="o">Boxed detector</param>
        /// <param name="args"><see cref="DetectorEventsArgs"/></param>
       private void ProcessAcquiringMessage(object o, DetectorEventsArgs args)
       {
            try
            {
                var d = o as IDetector;

                if (d.Status == DetectorStatus.ready && args.AcquireMessageParam == (int)CanberraDeviceAccessLib.AdviseMessageMasks.amAcquireDone && !d.IsPaused)
                {
                    SaveSpectraOnDetectorToFileAndDataBase(ref d);
                    MeasurementOfSampleDone?.Invoke(d.CurrentMeasurement);
                }
            }
            catch (ArgumentException ae)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, ae, Handlers.ExceptionLevel.Error);
            }
            catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, e, Handlers.ExceptionLevel.Error);
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
        private string GenerateFileSpectraName(string detName)
        {
            _nLogger.Info($"will generate file spectra name for the detector {detName}");
            var typeDict = new Dictionary<string, string> { {"SLI", "0"}, {"LLI-1", "1"}, {"LLI-2", "2"} };
            int maxNumber = DateTime.Now.Second;
            using (var ic = new InfoContext())
            {
                try
                {
                    maxNumber = ic.Measurements.Where(m =>
                                                                (
                                                                    m.FileSpectra.Length == 7 &&
                                                                    m.Type == Type &&
                                                                    IsNumber(m.FileSpectra) &&
                                                                    m.FileSpectra.Substring(0, 1) == detName.Substring(1, 1)
                                                                )
                                                                ).
                                                           Select(m => new
                                                           {
                                                               FileNumber = int.Parse(m.FileSpectra.Substring(2, 5))
                                                           }
                                                                           ).
                                                           Max(m => m.FileNumber);

                    return $"{detName.Substring(1, 1)}{typeDict[Type]}{(++maxNumber).ToString("D5")}";
                }
                catch (System.Data.SqlClient.SqlException sqle)
                {
                    Handlers.ExceptionHandler.ExceptionNotify(this, sqle, Handlers.ExceptionLevel.Warn);
                    return GenerateFileSpectraNameBasedOnLocalStorage(detName);
                }
                catch (Microsoft.EntityFrameworkCore.DbUpdateException dbe) // for duplicates
                {
                    Handlers.ExceptionHandler.ExceptionNotify(this, dbe, Handlers.ExceptionLevel.Error);
                    return GenerateFileSpectraNameBasedOnLocalStorage(detName);
                }
                catch (Exception e)
                {
                    Handlers.ExceptionHandler.ExceptionNotify(this, e, Handlers.ExceptionLevel.Error);
                }
            }
            return $"{detName[1]}{typeDict[Type]}{maxNumber}";
        }

        private string GenerateFileSpectraNameBasedOnLocalStorage(string detName)
        {
            _nLogger.Info($"will generate file spectra name for the detector {detName}");
            var typeDict = new Dictionary<string, string> { {"SLI", "0"}, {"LLI-1", "1"}, {"LLI-2", "2"} };
            int maxNumber = 0;
            try
            {
                if (!Directory.Exists(@"D:\Spectra"))
                    throw new Exception("Spectra Directory doesn't exist");

                var dir = new DirectoryInfo(@"D:\Spectra");
                var files = dir.GetFiles("*.cnf", SearchOption.AllDirectories).Where(f => f.CreationTime >= DateTime.Now.AddDays(-30)).ToList();

                maxNumber = files.Where(f =>
                                            f.Name.Length == 11 &&
                                            f.Name.Substring(1, 1) == typeDict[Type] &&
                                            IsNumber(Path.GetFileNameWithoutExtension(f.Name))&&
                                            f.Name.Substring(0, 1) == detName.Substring(1, 1)
                                       ).
                                  Select(f => new
                                  {
                                      FileNumber = int.Parse(f.Name.Substring(2, 5))
                                  }
                                        ).
                                  Max(f => f.FileNumber);

                return $"{detName.Substring(1, 1)}{typeDict[Type]}{(++maxNumber).ToString("D5")}";
            }
            catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, e, Handlers.ExceptionLevel.Error);
                return "";
            }
        }

        private bool IsNumber(string str)
        {
            return int.TryParse(str, out _);
        }
   }
}
