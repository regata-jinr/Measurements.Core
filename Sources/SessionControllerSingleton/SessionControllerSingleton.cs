/***************************************************************************
 *                                                                         *
 *                                                                         *
 * Copyright(c) 2017-2019, REGATA Experiment at FLNP|JINR                  *
 * Author: [Boris Rumyantsev](mailto:bdrum@jinr.ru)                        *
 * All rights reserved                                                     *
 *                                                                         *
 *                                                                         *
 ***************************************************************************/

using System.Collections.Generic;
using System.Data.SqlClient;
using CanberraDeviceAccessLib;
using System;
using System.Threading.Tasks;
using System.Linq;

// TODO:  add static analyzers

// FIXME: adding costura for merging dlls, but pay attention that it will break tests.
//        find out how to exclude test. exclude assemblies with xunit didn't help

/// <summary>
/// More information about the design of this core, general schemas and content see in the readme file of this repository:
/// https://github.com/regata-jinr/MeasurementsCore
/// </summary>
namespace Measurements.Core
{
    /// <summary>
    /// SessionControllerSingleton is the class that implemented singleton pattern and is used for control <seealso cref="Session"/> of measurements 
    /// </summary>
    public static class SessionControllerSingleton
    {
        /// <summary>
        /// For the logging we use NLog framework. Now we keep logs in the directory of application. 
        /// Logger creates 'MeasurementsLogs' folder nad file with name {short-date}.log.
        /// Also we consider implementation of web-monitor based on keeping logs in the data base.
        /// </summary>
        /// <see>
        public static NLog.Logger logger;
        /// <summary>
        /// The list of created session managed by SessionController
        /// </summary>
        public static List<ISession>              ManagedSessions         { get; private set; }

        /// <summary>
        /// The list of MCA devices available for usage. This list forms via MCA databases provided by Canberra DeviceAccessClass
        /// </summary>
        public static List<IDetector> AvailableDetectors { get; private set; }

        /// <summary>
        /// This internal method add detector to the list of available detectors for usage
        /// with all checkings
        /// </summary>
        /// <param name="name">Name of the detector. You can see detectors name in MCA Input Defenition Editor</param>
        private static void AddDetectorToList(string name)
        {
            IDetector d = null;
            try
            {
                    d = new Detector(name);
                    if (d.IsConnected)
                        AvailableDetectors.Add(d);
 
            }
            catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(null, e, Handlers.ExceptionLevel.Error);
                d?.Dispose(); 
            }
        }

        public static event Action SessionsInfoListHasChanged;
        public static void SessionsInfoListsChangedHaveOccurred() => SessionsInfoListHasChanged?.Invoke();

        public static event Action AvailableDetectorsListHasChanged;

        public static void AvailableDetectorsChangesHaveOccurred() => AvailableDetectorsListHasChanged?.Invoke();

        /// <summary>
        /// Add available detectors to list using internal MCA database
        /// </summary>
        private static void DetectorsInititalisation()
        {
            try
            {
                logger.Info("Initialization of detectors:");

                var _device = new DeviceAccessClass();
                var detNames = (IEnumerable<object>)_device.ListSpectroscopyDevices;

                foreach (var n in detNames)
                    AddDetectorToList(n.ToString());

            }
            catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(null, e, Handlers.ExceptionLevel.Error);
            }
        }

        /// <summary>
        /// Static constructor for initialization of all fields of the class
        /// </summary>
        static SessionControllerSingleton()
        {
            var conf = new Measurements.Configurator.ConfigManager();
            NLog.GlobalDiagnosticsContext.Set("LogConnectionString", conf.LogConnectionString);
            logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info("Inititalization of Session Controller instance has begun");

            _isDisposed              = false;
            AvailableDetectors       = new List<IDetector>();
            ManagedSessions          = new List<ISession>();

            DetectorsInititalisation();

            logger.Info("Creation of session controller instance has done");
        }


        /// <summary>
        /// Allows user to create session from the scratch
        /// </summary>
        /// <returns></returns>
        public static ISession Create()
        {
            logger.Info("Creating of the new session instance");
            ISession session = new Session();
            ManagedSessions.Add(session);
            return session;
        }

        /// <summary>
        /// Allow users to load saved session from db
        /// </summary>
        /// <param name="sName">Name of saved session</param>
        /// <returns>Session object with filled information such as: Name of session, List of detectors, type of measurement, spreaded option, counts, countmode, height, assistant, note</returns>
        public static ISession Create(SessionInfo sessionInfo)
        {

            logger.Info($"Createing existing session with name '{sessionInfo.Name}' from DB");
            try
            {
                if (sessionInfo == null)
                    throw new ArgumentNullException("Such session doesn't exist. Check the name or create the new one");

                ISession session = new Session(sessionInfo);
                ManagedSessions.Add(session);

                return session;
            }
            catch (ArgumentException ae)
            {
                Handlers.ExceptionHandler.ExceptionNotify(null, ae, Handlers.ExceptionLevel.Warn );
            }
            catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(null, e, Handlers.ExceptionLevel.Error);
            }
            return null;
        }

        /// <summary>
        /// Allows users to close session, remove it from the list of session managed by the SessionControllerSingleton and free resources correctly 
        /// </summary>
        /// <param name="session"></param>
        public static void CloseSession(ISession session)
        {
            try
            {
                if (session == null)
                {
                    logger.Info($"Session is null. Please, provide instance of Session type");
                    throw new ArgumentNullException("You try to remove null object instead of instanced session");
                }

                if (!ManagedSessions.Contains(session))
                {
                    logger.Info($"List of managed session doesn't contain such session - {session.Name}");
                    throw new InvalidOperationException($"Such session {session.Name} doesn't exist in the list of managed session by SessionControllerSingleton");
                }
                ManagedSessions.Remove(session);
                session.Dispose();
            }
            catch (ArgumentNullException ae)
            {
                Handlers.ExceptionHandler.ExceptionNotify(null, ae, Handlers.ExceptionLevel.Warn);
            }
            catch (InvalidOperationException ie)
            {
                Handlers.ExceptionHandler.ExceptionNotify(null, ie, Handlers.ExceptionLevel.Error);
            }
            catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(null, e, Handlers.ExceptionLevel.Error);
            }
        }

        private static bool _isDisposed;

        public static void Dispose()
        {
            CleanUp(true);
        }

        private static void CleanUp(bool isDisposing)
        {

            if (!_isDisposed)
            {
                if (isDisposing)
                {
                    ManagedSessions.Clear();
                    foreach (var d in AvailableDetectors)
                        d.Dispose();
                    AvailableDetectors.Clear();
                }
            }
            _isDisposed = true;
        }

    } // class SessionControllerSingleton
} // namespace
