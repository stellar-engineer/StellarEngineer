
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace StellarEngineer {
    public static class ModLoader {
        internal static void LoadAllMods(string modDirPath) {
            try {
                GetModsMetadata(modDirPath);
                // List<ModMetadata> mods = GetModsMetadata(modDirPath);
                // // TODO: load them in order.
                // foreach (var mod in mods ) {
                //     ModLoader.LoadMod(mod);
                // }
            } catch (Exception e) {
                StellarLogger.logger.LogError(e.Message);
                StellarLogger.logger.LogError(e.StackTrace);
                StellarLogger.logger.LogError(e.InnerException);
            }
        } 

        internal static List<ModMetadata> GetModsMetadata(string modDirPath) {
            var result = new List<ModMetadata>(); 

            StellarLogger.logger.LogInfo($"Searching for subdirectories in {modDirPath}");
            foreach (var dir in Directory.GetDirectories(modDirPath)) {
                StellarLogger.logger.LogDebug($"Searching for a mod.json in {dir}");
                var filepath = dir + "/mod.json";
                if (!File.Exists(filepath)) {
                    continue;
                }
                
                StellarLogger.logger.LogDebug($"Found {filepath}");
                ModMetadata metadata = null;
                try {
                    string value = File.ReadAllText(filepath);
                    StellarLogger.logger.LogInfo($"{value}");
                    
                    metadata = (ModMetadata)JsonConvert.DeserializeObject(value, typeof(ModMetadata));
                    StellarLogger.logger.LogInfo($"Bruh what");
                } catch (Exception e) {
                    StellarLogger.logger.LogError($"Failed to load mod: {filepath}");
                    StellarLogger.logger.LogError(e.Message);
                    StellarLogger.logger.LogError(e.StackTrace);
                    continue;
                }

                if (metadata == null) {
                    StellarLogger.logger.LogError($"Failed to load mod: {filepath}");
                    StellarLogger.logger.LogError("Reason: deserialized metadata is null");
                    continue;
                }

                var (isValid, errors) = metadata.IsValid();
                if (!isValid) {
                    StellarLogger.logger.LogError($"Failed to load mod: {filepath}");
                    foreach (var error in errors) {
                        StellarLogger.logger.LogError(error);
                    }
                    continue;
                }

                StellarLogger.logger.LogDebug($"Found mod: {metadata.ModName}");
                StellarLogger.logger.LogDebug($"Schema Version: {metadata.SchemaVersion}");
                StellarLogger.logger.LogDebug($"Name: {metadata.ModName}");
                StellarLogger.logger.LogDebug($"ID: {metadata.ModID}");
                StellarLogger.logger.LogDebug($"Author: {metadata.ModAuthor}");
                StellarLogger.logger.LogDebug($"Entrypoint: {metadata.Entrypoint}");
                StellarLogger.logger.LogDebug($"Mod Version: {metadata.ModVersion}");
                StellarLogger.logger.LogDebug($"");
            }

            return result;
        }
        public static void LoadMod(ModMetadata mod) {
            StellarLogger.logger.LogInfo($"Loading Mod: {mod.ModID}");
            
            Assembly assembly;
            try {
                assembly = Assembly.LoadFrom(mod.Entrypoint);
            } catch (Exception e) {
                StellarLogger.logger.LogError($"Couldn't load assembly (DLL): {mod.ModID}");
                StellarLogger.logger.LogError(e.Message);
                StellarLogger.logger.LogError(e.StackTrace);
                return;
            }

            if (assembly == null) {
                StellarLogger.logger.LogError($"Couldn't load assembly (DLL): {mod.ModID}");
                StellarLogger.logger.LogError("Reason: assembly is null.");
                return;
            }

            Type[] types;
            try {
                types = assembly.GetTypes();
            } catch (Exception e) {
                StellarLogger.logger.LogError($"Couldn't get assembly types: {mod.ModID}");
                StellarLogger.logger.LogError(e.Message);
                StellarLogger.logger.LogError(e.StackTrace);
                return;
            }
            
            foreach (var type in types) {
                foreach (var method in type.GetRuntimeMethods()) {
                    if (!method.IsStatic) continue;
                    if (method.ReturnType != typeof(void)) continue;
                    if (method.GetParameters().Length != 0) continue;

                    var attribute = method.GetCustomAttribute<EntrypointAttribute>();

                    if (attribute != null) {
                        StellarLogger.logger.LogInfo($"Mod started loading: {mod.ModID}");
                        try {
                            method.Invoke(null,null);
                        } catch (Exception e) {
                            StellarLogger.logger.LogError($"Error in mod entrypoint: {mod.ModID}");
                            StellarLogger.logger.LogError(e.Message);
                            StellarLogger.logger.LogError(e.StackTrace);
                            return;
                        }
                        StellarLogger.logger.LogInfo($"Mod finished loading: {mod.ModID}");
                        return;
                    }
                }
            }
            StellarLogger.logger.LogError($"Failed to load DLL: {mod.ModID}");
            StellarLogger.logger.LogError($"Check if you have a function marked with [Entrypoint]. It should also be static, have return type 'void' and require no parameters.");
        }
    }
}