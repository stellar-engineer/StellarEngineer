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
            StellarLogger.Enable("stellar.engineer.logger");

            StellarLogger.logger.Log("Doorstop loaded succesfully!");

            var harmony = new Harmony("stellar.engineer");
            StellarLogger.logger.Log("Harmony Loaded.");
            
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            StellarLogger.logger.Log("Finished automatic patching.");

            SceneManager.sceneLoaded += Load;
        }

        public static void Load(Scene _arg0, LoadSceneMode _arg2) {
            SceneManager.sceneLoaded -= Load;

            Assembly assembly = Assembly.LoadFrom("./mods/HelloWorld/src/bin/Release/net4.5/HelloWorld.dll");
            foreach (var type in assembly.GetTypes()) {
                foreach (var method in type.GetRuntimeMethods()) {
                    if (!method.IsStatic) continue;
                    if (method.ReturnType != typeof(void)) continue;
                    if (method.GetParameters().Length != 0) continue;


                    var attribute = method.GetCustomAttribute<EntrypointAttribute>();

                    if (attribute != null) {
                        method.Invoke(null,null);
                        return;
                    }
                }
            }
        }
    }
}

[HarmonyPatch(typeof(GlobalC), "Update")]
class Patch01 {
    static void Prefix() {
        if (Input.GetKeyDown(KeyCode.Q)) {
            StellarLogger.logger.Log("q key was pressed");
        }
    }
}
