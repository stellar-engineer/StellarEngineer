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
    public static class StellarLogger {
        private static readonly List<string> messagesBacklog = new List<string>();
        private static Harmony harmony = null;
        private static bool unityEnabled = false;
        private static StreamWriter logStream = null;
        private static bool enabled = false;

        public static void Log(object message) {
            if (!enabled) {
                return;
            }

            // We need to wait for unity to finish initializing before we can start logging using Debug.Log().
            // Unti then, store all messages into a temporary list.
            if (!unityEnabled) {
                messagesBacklog.Add(FormatMessage(message));
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

            Debug.Log(FormatMessage(message));
        }

        private static string FormatMessage(object message) {
            if (harmony == null) {
                return "! " + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff") + " : " + message.ToString();
            }
            else {
                return "! " + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff") + " [" + harmony.Id + "]: " + message.ToString();
            }
        }

        /// <summary>
        /// Enable the logger for your mod. Unless you enable the logger, <see cref="StellarLogger.Log(object)"/> won't function.
        /// </summary>
        /// <param name="id">An ID for your logger. Should be different from the id of your main Harmony instance.</param>
        // TODO: should we still create a new harmony instance? Kinda only needed by us.
        public static void Enable(string id) {
            StellarLogger.enabled = true;
            StellarLogger.harmony = new Harmony(id);

            SceneManager.sceneLoaded += Bootstrap_Log;
        }

        /// <summary>
        /// Enable logging to file. Only used internally by Stellar Engineer.
        /// </summary>
        internal static void EnableFileLog() {
            logStream = new StreamWriter("./stellar.engineer.log");
            SceneManager.sceneLoaded += Bootstrap_EnableFileLog;
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
                    foreach (object msg in messagesBacklog) {
                        if (msg != null) {
                            Debug.Log(msg);
                        }
                    }
                    messagesBacklog.Clear();
                }
            }
        }

        private static void Bootstrap_EnableFileLog(Scene _arg0, LoadSceneMode _arg2) {
            SceneManager.sceneLoaded -= Bootstrap_EnableFileLog;


            var debugLogOriginal = AccessTools.Method(typeof(UnityEngine.Debug), nameof(Debug.Log), new Type[] { typeof(object)});
            var logPrefix = SymbolExtensions.GetMethodInfo(() => Prefix_LogToFile("placeholder"));
                
            harmony.Patch(debugLogOriginal, new HarmonyMethod(logPrefix));

            logStream.WriteLine(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff") + ": Logger initialized.");
            logStream.WriteLine("For the full backtrace of every log call, see Unity's log: C:/Users/<YOUR_USER>/AppData/LocalLow/Kalla Gameworks/The Pegasus Expedition/Player.log\n");            
            logStream.Flush();
        }
        

        private static void Prefix_LogToFile(object message) {
            if (StellarLogger.logStream != null) {
                if (message is string str && str[0] == '!') {
                    logStream.WriteLine(str);
                    logStream.Flush();
                }
            }           
        }
    }
}