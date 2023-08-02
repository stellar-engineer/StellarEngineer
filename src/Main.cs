using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using HarmonyLib;
using StellarEngineer;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Doorstop {
    class Entrypoint {
        public static void Start() {
            // NOTE: Possible race condition here, if file log enables after the regular logger, then queued messages
            // will not be put into the file log. This issue will happen just for Stellar Engineer, as only it should enable
            // the file log. 
            StellarLogger.EnableFileLog();
            StellarLogger.Enable();

            try {
                StellarLogger.logger.LogInfo("Doorstop loaded succesfully!");

                var harmony = new Harmony("stellar.engineer");
                StellarLogger.logger.LogInfo("Harmony Loaded.");
                
                harmony.PatchAll(Assembly.GetExecutingAssembly());
                StellarLogger.logger.LogInfo("Finished automatic patching.");

                ModLoader.LoadAllMods("./mods");
                StellarLogger.logger.LogInfo("Finished loading all mods. Have fun!");
            } catch (Exception e) {
                StellarLogger.logger.LogError("-=-=-=-=-=- FATAL EXCEPTION -=-=-=-=-=-");
                StellarLogger.logger.LogError(e.Message);
                StellarLogger.logger.LogError(e.StackTrace);
            }
        }
    }
}

[HarmonyPatch(typeof(GlobalC), "Update")]
class Patch01 {
    static void Prefix() {
        if (Input.GetKeyDown(KeyCode.Q)) {
            StellarLogger.logger.LogInfo("q key was pressed");
        }
    }
}
