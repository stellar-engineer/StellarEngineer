using System;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using StrategyCardContainer;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StellarEngineer {
    /// <summary>
    /// Stellar Engineer Logger. This puts all of your messages into the Unity Debug Console and into the "stellar.engineer.log" file located
    /// next to the executable.
    /// </summary>
    public class StellarLogger {
// ============================================= // 
// =                 Instanced                 = // 
// ============================================= // 
        public readonly string id;

        public StellarLogger(string id) {
            this.id = id;
        }

        public void LogDebug(object message) {
            Log(new LogMessage() {
                logLevel = LogLevel.Debug,
                id = this.id,
                message = message
            });
        }
        public void LogInfo(object message) {
            Log(new LogMessage() {
                logLevel = LogLevel.Info,
                id = this.id,
                message = message
            });
        }
        public void LogWarning(object message) {
            Log(new LogMessage() {
                logLevel = LogLevel.Warning,
                id = this.id,
                message = message
            });
        }
        public void LogError(object message) {
            Log(new LogMessage() {
                logLevel = LogLevel.Error,
                id = this.id,
                message = message
            });
        }

// ========================================== // 
// =                 Static                 = // 
// ========================================== // 

        // This is our instance of the logger.
        internal static StellarLogger logger = new StellarLogger("stellar.engineer");
        internal static LogLevel defaultLogLevel = LogLevel.Debug; // TODO: get this from a config file

        private static readonly List<LogMessage> messagesBacklog = new List<LogMessage>();
        private static bool unityEnabled = false;
        private static StreamWriter logStream = null;
        private static bool enabled = false;

        internal static void Log(LogMessage logMessage) {
            if (!enabled) {
                return;
            }

            if (logMessage.logLevel < defaultLogLevel) {
                // return;
            }

            // We need to wait for unity to finish initializing before we can start logging using Debug.Log().
            // Untiy then, store all messages into a temporary list.
            if (!unityEnabled) {
                messagesBacklog.Add(logMessage);
                LogToFile(FormatMessage(logMessage));
                return;
            }

            // // There could be a race condition here. Leaving this here in case people report missing log messages. 
            // if (messagesBacklog.Count > 0) {
            //     foreach (object msg in messagesBacklog) {
            //         if (msg != null) {
            //             Debug.Log(msg);
            //         }
            //     }
            //     messagesBacklog.Clear();
            // }

            LogUnity(logMessage.logLevel, FormatMessage(logMessage));
            LogToFile(FormatMessage(logMessage));
        }

        private static string FormatMessage(LogMessage logMessage) {
            return $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} ({logMessage.id}) [{logMessage.logLevel}]: {logMessage.message}";
        }

        private static void LogUnity(LogLevel logLevel, string formattedMessage) {
            switch (logLevel) {
                case LogLevel.Debug:
                case LogLevel.Info:
                    Debug.Log(formattedMessage);
                    break;
                case LogLevel.Warning:
                    Debug.LogWarning(formattedMessage);
                    break;
                case LogLevel.Error:
                    Debug.LogError(formattedMessage);
                    break;
                default:
                    Debug.LogError(new NotImplementedException().Message);
                    LogToFile(new NotImplementedException().Message);
                    break;
            }
        }

        /// <summary>
        /// Enable the logger. This is game-wide, because as we load the assemblies, the "static" information here is shared with the loaded assemblies.
        /// As such, we truly need to enable once. Only called internally by Stellar Engineer.
        /// </summary>
        internal static void Enable() {
            StellarLogger.enabled = true;
            SceneManager.sceneLoaded += Bootstrap_Log;
        }

        /// <summary>
        /// Enable logging to file. Only called internally by Stellar Engineer.
        /// </summary>
        internal static void EnableFileLog() {
            logStream = new StreamWriter("./stellar.engineer.log");
            logStream.AutoFlush = true;
            
            logStream.WriteLine(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff") + ": Logger initialized.");
            logStream.WriteLine("For the full backtrace of every log call, see Unity's log: C:/Users/<YOUR_USER>/AppData/LocalLow/Kalla Gameworks/The Pegasus Expedition/Player.log\n");            
            logStream.Flush();
        }

        private static void LogToFile(object message) {
            if (StellarLogger.logStream != null) {
                logStream.WriteLine(message.ToString());
                logStream.Flush();
            }           
        }

// ==================================================== // 
// =                 Injected Methods                 = // 
// ==================================================== // 
        private static void Bootstrap_Log(Scene _arg0, LoadSceneMode _arg2) {
            // When Unity finishes setting up, we can start using Debug.Log to print out the backlog.
            if (!StellarLogger.unityEnabled) {
                StellarLogger.unityEnabled = true;
                SceneManager.sceneLoaded -= Bootstrap_Log; // <-- move this outside if block?


                if (messagesBacklog.Count > 0) {
                    foreach (var msg in messagesBacklog) {
                        if (msg != null) {
                            LogUnity(msg.logLevel, FormatMessage(msg));
                        }
                    }
                    messagesBacklog.Clear();
                }
            }
        }
        public enum LogLevel {
            Debug   = 0,
            Info    = 1,
            Warning = 2,
            Error   = 3,
        }
        public class LogMessage {
            public LogLevel logLevel = LogLevel.Debug;
            public string id = "unknown";
            public object message = "no message";

            public LogMessage()
            {
            }

            public LogMessage(LogLevel logLevel, string id, object message) {
                this.logLevel = logLevel;
                this.id = id;
                this.message = message;
            }
        }
    }
}