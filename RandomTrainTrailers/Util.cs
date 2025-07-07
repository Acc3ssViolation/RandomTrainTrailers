using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.IO;
using ColossalFramework.UI;
using System.Collections.Generic;
using UnityEngine;
using System;
using ColossalFramework.Packaging;
using System.IO;
using System.Xml.Serialization;

namespace RandomTrainTrailers
{
    public static class Util
    {
        public static System.Random Random = new System.Random();

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

        public static void ShowWarningMessage(string message)
        {
            UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel").SetMessage(Mod.name, message, false);
        }

        public static VehicleInfo FindVehicle(string prefabName, string packageName)
        {
            // Prevent unnecessary logging
            var restore = CODebugBase<LogChannel>.IsChannelEnabled(LogChannel.Serialization);
            CODebugBase<LogChannel>.DisableChannels(LogChannel.Serialization);
            var prefab = PrefabCollection<VehicleInfo>.FindLoaded(prefabName) ??
                         PrefabCollection<VehicleInfo>.FindLoaded(prefabName + "_Data") ??
                         PrefabCollection<VehicleInfo>.FindLoaded(PathEscaper.Escape(prefabName) + "_Data") ??
                         PrefabCollection<VehicleInfo>.FindLoaded(packageName + "." + prefabName + "_Data") ??
                         PrefabCollection<VehicleInfo>.FindLoaded(packageName + "." + PathEscaper.Escape(prefabName) + "_Data");
            if (restore)
                CODebugBase<LogChannel>.EnableChannels(LogChannel.Serialization);

            return prefab;
        }

        public static void LogException(Exception e)
        {
            LogError("The following exception was thrown:");
            Debug.LogException(e);
        }

        public static string GetVehicleDisplayName(string assetname)
        {
            string locale = Locale.GetUnchecked("VEHICLE_TITLE", assetname);
           
            if(locale.StartsWith("VEHICLE_TITLE"))
            {
                return assetname;
            }
            return locale;
        }

        public static IList<T> SwapChecked<T>(this IList<T> list, int indexA, int indexB)
        {
            if(indexA >= 0 && indexB >= 0 && indexA < list.Count && indexB < list.Count)
            {
                return list.Swap(indexA, indexB);
            }
            return list;
        }

        public static IList<T> Swap<T>(this IList<T> list, int indexA, int indexB)
        {
            T tmp = list[indexA];
            list[indexA] = list[indexB];
            list[indexB] = tmp;
            return list;
        }

        public static void IterateModsAndAssets(string filename, Action<string, string, bool> found)
        {
            if (found == null)
                throw new ArgumentNullException(nameof(found));

            var checkedPaths = new HashSet<string>();
            // Load definitions from mod folders
            foreach (var current in ColossalFramework.Plugins.PluginManager.instance.GetPluginsInfo())
            {
                if (current.isEnabled)
                {
                    var path = Path.Combine(current.modPath, filename);
                    // skip files which were already parsed
                    if (checkedPaths.Contains(path)) continue;
                    checkedPaths.Add(path);
                    if (!File.Exists(path)) continue;
                    found(path, current.name, true);
                }
            }

            // Load definitions from prefabs
            for (uint i = 0; i < PrefabCollection<VehicleInfo>.LoadedCount(); i++)
            {
                var prefab = PrefabCollection<VehicleInfo>.GetLoaded(i);

                // Check if asset is valid
                if (prefab == null) continue;

                var asset = PackageManager.FindAssetByName(prefab.name);

                var crpPath = asset?.package?.packagePath;
                if (crpPath == null) continue;

                var path = Path.Combine(Path.GetDirectoryName(crpPath) ?? "", filename);
                // skip files which were already parsed
                if (checkedPaths.Contains(path)) continue;
                checkedPaths.Add(path);
                if (!File.Exists(path)) continue;
                found(path, asset.package.packageName, false);
            }
        }

        /// <summary>
        /// Tries to deserialize an xml file, returns null on failure.
        /// Exceptions are logged via Logging.
        /// </summary>
        /// <typeparam name="T">The type to deserialize</typeparam>
        /// <param name="path">Path of the .xml file</param>
        /// <returns>Deserialized object or null on failure</returns>
        public static T XMLDeserialize<T>(string path) where T : class
        {
            T result = null;
            var xmlSerializer = new XmlSerializer(typeof(T));
            try
            {
                using (var streamReader = new System.IO.StreamReader(path))
                {
                    result = xmlSerializer.Deserialize(streamReader) as T;
                }
            }
            catch (Exception e)
            {
                Util.LogException(e);
            }
            return result;
        }
    }
}
