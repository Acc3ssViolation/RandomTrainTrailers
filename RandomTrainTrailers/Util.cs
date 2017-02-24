using ColossalFramework;
using ColossalFramework.IO;
using UnityEngine;

namespace RandomTrainTrailers
{
    public static class Util
    {
        public static SavedBool enableLogs = new SavedBool("EnableLogs", Mod.settingsFile, false, true);

        private static string cachedModDir;

        /// <summary>
        /// Returns the directory the mod is in. Does not contain trailing slashes.
        /// </summary>
        public static string ModDirectory
        {
            get
            {
                if(cachedModDir == null)
                {
                    var asm = System.Reflection.Assembly.GetAssembly(typeof(Util));
                    var pluginInfo = ColossalFramework.Plugins.PluginManager.instance.FindPluginInfo(asm);
                    cachedModDir = pluginInfo.modPath;
                }
                return cachedModDir;
            }
        }

        public static void Log(object message, bool always = false)
        {
            if(!enableLogs && !always) { return; }

            Debug.Log(Mod.name + ": " + message.ToString());
        }

        public static void LogError(object message)
        {
            Debug.LogError(Mod.name + ": " + message.ToString());
        }

        public static void LogWarning(object message)
        {
            Debug.LogWarning(Mod.name + ": " + message.ToString());
        }

        public static VehicleInfo FindVehicle(string prefabName, string packageName)
        {
            var prefab = PrefabCollection<VehicleInfo>.FindLoaded(prefabName) ??
                         PrefabCollection<VehicleInfo>.FindLoaded(prefabName + "_Data") ??
                         PrefabCollection<VehicleInfo>.FindLoaded(PathEscaper.Escape(prefabName) + "_Data") ??
                         PrefabCollection<VehicleInfo>.FindLoaded(packageName + "." + prefabName + "_Data") ??
                         PrefabCollection<VehicleInfo>.FindLoaded(packageName + "." + PathEscaper.Escape(prefabName) + "_Data");

            return prefab;
        }
    }
}
