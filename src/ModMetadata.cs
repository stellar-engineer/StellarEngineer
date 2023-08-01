
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace StellarEngineer {
    [JsonObject(MemberSerialization.OptIn)]
    public class ModMetadata {
        public ModMetadata()
        {
        }

        [JsonProperty("schema.version")]
        public int SchemaVersion {get; set;}
        [JsonProperty("name")]
        public string ModName {get; set;}
        [JsonProperty("id")]
        public string ModID {get; set;}
        [JsonProperty("author")]
        public string ModAuthor {get; set;} = "Unknown";
        [JsonProperty("entrypoint")]
        public string Entrypoint {get; set;}
        [JsonProperty("version")]
        public string ModVersion {get; set;} // TODO: make this a struct, so it can be compared, sorted, etc.

        public string ModFolder {get; set;} = "";
        public string EntrypointFullPath => ModFolder + Entrypoint;

        public (bool valid, List<string> errors) IsValid() {
            bool valid = true;
            List<string> errors = new List<string>();

            if (SchemaVersion != 1) {
                valid = false;
                errors.Add($"Unsupported schema version: {SchemaVersion}. Property: [schema.version]");
            }

            if (String.IsNullOrEmpty(ModName)) {
                valid = false;
                errors.Add($"Mod name is missing or empty. Property: [name]");
            }

            if (String.IsNullOrEmpty(ModID)) {
                valid = false;
                errors.Add($"Mod id is missing or empty. Property: [id]");
            }

            if (String.IsNullOrEmpty(ModID)) {
                valid = false;
                errors.Add($"Mod id is missing or empty. Property: [id]");
            }

            if (String.IsNullOrEmpty(ModAuthor)) {
                valid = false;
                errors.Add($"Mod author is missing or empty. Property: [author]");
            }

            if (String.IsNullOrEmpty(ModFolder)) {
                valid = false;
                errors.Add("Mod Folder property for ModMetada was not set. This is an internal StellarEngineer issue, please report this on github.");
            }

            if (String.IsNullOrEmpty(Entrypoint)) {
                valid = false;
                errors.Add($"Mod entrypoint is missing or empty. Property: [entrypoint]");
            } else {
                if (!File.Exists(EntrypointFullPath)) {
                    valid = false;
                    errors.Add($"Mod entrypoint does not exist: {EntrypointFullPath} Property: [entrypoint]");
                }
                if (!Entrypoint.EndsWith(".dll")) {
                    valid = false;
                    errors.Add($"Mod entrypoint must be a dll file (filepath must end with '.dll'): {Entrypoint} Property: [entrypoint]");                
                }
            }
            
            if (String.IsNullOrEmpty(ModVersion)) {
                valid = false;
                errors.Add($"Mod version is missing or empty. Property: [version]");
            }


            return (valid, errors);
        }
    }
}