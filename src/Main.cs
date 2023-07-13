using System.IO;
using System.Reflection;
using HarmonyLib;
using StellarEngineer;
using UnityEngine;


namespace Doorstop {
    class Entrypoint {
        public static void Start() {
            // NOTE: Possible race condition here, if file log enables after the regular logger, then queued messages
            // will not be put into the file log. This issue will happen just for Stellar Engineer, as only it should enable
            // the file log. 
            StellarLogger.EnableFileLog();
            StellarLogger.Enable("stellar.engineer.logger");

            StellarLogger.Log("Doorstop loaded succesfully!");

            var harmony = new Harmony("stellar.engineer");
            StellarLogger.Log("Harmony Loaded.");
            
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            StellarLogger.Log("Finished automatic patching.");
        }
    }
}

[HarmonyPatch(typeof(GlobalC), "Update")]
class Patch01 {
    static void Prefix() {
        if (Input.GetKeyDown(KeyCode.Q)) {
            StellarLogger.Log("q key was pressed");
        }
    }
}
