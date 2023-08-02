
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace StellarEngineer {
    public static class ModLoader {
        internal static void LoadAllMods(string modDirPath) {
            List<ModMetadata> modMetadata = GetAllModMetadata(modDirPath);
            ModList modList = GetModList(modDirPath, modMetadata);
            
            // Because the modlist auto-sorted them when loading, all mods with load in incerasing order of priority (priority 1, then 2, then ...)
            foreach (var mod in modList.Metadata.Where(m => modList.GetEntry(m).Enabled)) {
                ModLoader.LoadMod(mod);
            }
        }

        internal static List<ModMetadata> GetAllModMetadata(string modDirPath) {
            var result = new List<ModMetadata>(); 

            StellarLogger.logger.LogInfo($"Searching for subdirectories in {modDirPath}");
            foreach (var dir in Directory.GetDirectories(modDirPath)) {
                StellarLogger.logger.LogDebug($"Searching for a mod.json in {dir}");
                var filepath = dir + "/mod.json";
                if (!File.Exists(filepath)) {
                    StellarLogger.logger.LogWarning($"Found a directory ({dir}) with no mod.json file. Is it on purpose?");
                    continue;
                }
                
                StellarLogger.logger.LogDebug($"Found {filepath}");
                ModMetadata metadata = null;
                try {
                    string raw_json = File.ReadAllText(filepath);
                    metadata = (ModMetadata)JsonConvert.DeserializeObject(raw_json, typeof(ModMetadata));
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

                metadata.ModFolder = dir + "/";

                var (isValid, errors) = metadata.IsValid();
                if (!isValid) {
                    StellarLogger.logger.LogError($"Failed to load mod: {filepath}");
                    foreach (var error in errors) {
                        StellarLogger.logger.LogError(error);
                    }
                    continue;
                }

                StellarLogger.logger.LogDebug($"Found mod: {metadata.ModName}");
                StellarLogger.logger.LogDebug(JsonConvert.SerializeObject(metadata, Formatting.Indented));

                result.Add(metadata);
            }

            return result;
        }
        
        internal static ModList GetModList(string modDirPath, List<ModMetadata> mods) {
            var modlistPath = modDirPath + "/modlist.json";
            StellarLogger.logger.LogDebug($"Looking for modlist in {modlistPath}");

            if (!File.Exists(modlistPath)) {
                StellarLogger.logger.LogInfo($"Creating new modlist at {modlistPath}");
                ModList.CreateNew(modlistPath, mods);
            }

            var modList = ModList.Load(modlistPath, mods);
            
            StellarLogger.logger.LogInfo($"Loaded {modList.Entries.Count} mod entries, of which {modList.Entries.Where(e => e.Enabled).Count()} are enabled.");
            foreach (var entry in modList.Entries) {
                StellarLogger.logger.LogDebug(JsonConvert.SerializeObject(entry, Formatting.Indented));
            }

            return modList;
        }
        internal static void LoadMod(ModMetadata mod) {
            StellarLogger.logger.LogInfo($"Loading Mod: {mod.ModID}");
            
            Assembly assembly;
            try {
                assembly = Assembly.LoadFrom(mod.EntrypointFullPath);
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
                        StellarLogger.logger.LogDebug($"Invoking mod entrypoint: {mod.ModID}");
                        try {
                            method.Invoke(null,null);
                        } catch (Exception e) {
                            StellarLogger.logger.LogError($"Error in mod entrypoint: {mod.ModID}");
                            StellarLogger.logger.LogError(e.Message);
                            StellarLogger.logger.LogError(e.StackTrace);
                            return;
                        }
                        StellarLogger.logger.LogDebug($"Mod entrypoint function done: {mod.ModID}");
                        return;
                    }
                }
            }
            StellarLogger.logger.LogError($"Failed to load DLL: {mod.ModID}");
            StellarLogger.logger.LogError($"Check if you have a function marked with [Entrypoint]. It should also be static, have return type 'void' and require no parameters.");
        }
    }
}