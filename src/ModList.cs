using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Linq;

namespace StellarEngineer {
    [JsonObject(MemberSerialization.OptIn)]
    public class ModList {
        public ModList() {}

        [JsonProperty("entries")]
        public List<ModListEntry> Entries {get; set;} = new List<ModListEntry>();

        public List<ModMetadata> Metadata {get; set;}

        public static void CreateNew(string path, List<ModMetadata> mods) {
            if (File.Exists(path)) {
                StellarLogger.logger.LogError($"modlist.json already exists, but tried to create one. Aborting creation. Path: {path}");
                return;
            }

            ModList result = new ModList();
            foreach (var mod in mods) {
                result.Entries.Add(ModListEntry.DefaultEntry(mod));
            }

            string raw_json = JsonConvert.SerializeObject(result, Formatting.Indented);
            File.WriteAllText(path, raw_json);
        }

        public static ModList Load(string path, List<ModMetadata> mods) {
            if (!File.Exists(path)) {
                StellarLogger.logger.LogError($"modlist.json does not exist. Cannot load mod list: {path}");
                return null;
            }

            ModList modList = JsonConvert.DeserializeObject<ModList>(File.ReadAllText(path));
            modList.Metadata = mods;

            // We want to make sure each ModMetada has a corresponding entry, creating one if needed.
            // If an entry does not have an associated metadata, then remove it.
            modList.Entries.RemoveAll(e => modList.GetMetadata(e) == null);

            mods.Where(mod => modList.GetEntry(mod) == null).ToList()
                .ForEach(mod => modList.Entries.Add(ModListEntry.DefaultEntry(mod)));


            modList.Entries = modList.Entries.OrderBy(mod => mod.Priority).ToList();

            return modList;
        }

        public void Save(string path) {
            string raw_json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(path, raw_json);
        }

        public ModMetadata GetMetadata(ModListEntry entry) {
            if (entry == null) { 
                return null;
            }

            foreach (var mod in this.Metadata) {
                if (entry.Id == mod.ModID) {
                    return mod;
                }
            }

            return null;
        }

        public ModListEntry GetEntry(ModMetadata metadata) {
            if (metadata == null) { 
                return null;
            }

            foreach (var entry in this.Entries) {
                if (entry.Id == metadata.ModID) {
                    return entry;
                }
            }

            return null;
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class ModListEntry {
        public ModListEntry() {}

        [JsonProperty("id")]
        public string Id {get; set;}
        [JsonProperty("priority")]
        public int Priority {get; set;} 
        [JsonProperty("enabled")]
        public bool Enabled {get; set;}

        public static ModListEntry DefaultEntry(ModMetadata modMetadata) {
            return new ModListEntry {
                Id = modMetadata.ModID,
                Priority = 100,
                Enabled = false
            };
        } 
    }
}
