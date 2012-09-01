﻿// -----------------------------------------------------------------------
// <copyright file="Logger.cs" company="Uhuru Software, Inc.">
// Copyright (c) 2011 Uhuru Software, Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace Uhuru.Utilities
{
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using log4net;
    using log4net.Appender;
    
    /// <summary>
    /// This is a helper logger class that is used throughout the code.
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// A lock object used to make sure multiple threads don't configure an event log source at the same time.
        /// </summary>
        private static readonly object configureEventLogSourceLock = new object();

        /// <summary>
        /// The log4net ILog object used for logging.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(System.AppDomain.CurrentDomain.FriendlyName);
        
        /// <summary>
        /// Specifies whether the Windows Event Log source has been configured.
        /// </summary>
        private static bool isSourceConfigured = false;

        /// <summary>
        /// Logs a fatal message.
        /// This indicates a really severe error, that will probably make the application crash.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        public static void Fatal(string message)
        {
            SetEventLogSource();
            log.Fatal(message);
        }

        /// <summary>
        /// Logs an error message.
        /// This indicates an error, but the application may be able to continue.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        public static void Error(string message)
        {
            SetEventLogSource();
            log.Error(message);
        }

        /// <summary>
        /// Logs a warning message.
        /// This indicates a situation that could lead to some bad things.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        public static void Warning(string message)
        {
            SetEventLogSource();
            log.Warn(message);
        }

        /// <summary>
        /// Logs an information message.
        /// The message is used to indicate some progress.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        public static void Info(string message)
        {
            SetEventLogSource();
            log.Info(message);
        }

        /// <summary>
        /// Logs a debug message.
        /// This is an informational message, that is useful when debugging.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        public static void Debug(string message)
        {
            if (message != null && message.Contains("connection 0"))
            {
                return;
            }

            SetEventLogSource();
            log.Debug(message);
        }

        /// <summary>
        /// Logs a fatal message and formats it.
        /// This indicates a really severe error, that will probably make the application crash.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments used for formatting.</param>
        public static void Fatal(string message, params object[] args)
        {
            SetEventLogSource();
            log.FatalFormat(CultureInfo.InvariantCulture, message, args);
        }

        /// <summary>
        /// Logs an error message and formats it.
        /// This indicates an error, but the application may be able to continue.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments used for formatting.</param>
        public static void Error(string message, params object[] args)
        {
            SetEventLogSource();
            log.ErrorFormat(CultureInfo.InvariantCulture, message, args);
        }

        /// <summary>
        /// Logs a warning message and formats it.
        /// This indicates a situation that could lead to some bad things.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments used for formatting.</param>
        public static void Warning(string message, params object[] args)
        {
            SetEventLogSource();
            log.WarnFormat(CultureInfo.InvariantCulture, message, args);
        }

        /// <summary>
        /// Logs an information message and formats it.
        /// The message is used to indicate some progress.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments used for formatting.</param>
        public static void Info(string message, params object[] args)
        {
            SetEventLogSource();
            log.InfoFormat(CultureInfo.InvariantCulture, message, args);
        }

        /// <summary>
        /// Logs a debug message and formats it.
        /// This is an informational message, that is useful when debugging.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments used for formatting.</param>
        public static void Debug(string message, params object[] args)
        {
            SetEventLogSource();
            log.DebugFormat(CultureInfo.InvariantCulture, message, args);
        }

        /// <summary>
        /// Sets up the event log source.
        /// </summary>
        private static void SetEventLogSource()
        {
            if (!isSourceConfigured)
            {
                lock (configureEventLogSourceLock)
                {
                    if (!isSourceConfigured)
                    {
                        isSourceConfigured = true;
                        EventLogAppender eventLogAppender = log.Logger.Repository.GetAppenders().FirstOrDefault(a => a.Name == "EventLogAppender") as EventLogAppender;
                        if (eventLogAppender != null)
                        {
                            if (!EventLog.Exists(eventLogAppender.LogName))
                            {
                                EventLog.CreateEventSource(System.AppDomain.CurrentDomain.FriendlyName, ((log4net.Appender.EventLogAppender)log.Logger.Repository.GetAppenders().Single(a => a.Name == "EventLogAppender")).LogName);
                            }

                            ((log4net.Appender.EventLogAppender)log.Logger.Repository.GetAppenders().Single(a => a.Name == "EventLogAppender")).ApplicationName = System.AppDomain.CurrentDomain.FriendlyName;
                            ((log4net.Appender.EventLogAppender)log.Logger.Repository.GetAppenders().Single(a => a.Name == "EventLogAppender")).ActivateOptions();
                        }
                    }
                }
            }
        }
    }
}
