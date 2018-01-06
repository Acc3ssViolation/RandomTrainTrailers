using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.IO;
using ColossalFramework.UI;
using System.Collections.Generic;
using UnityEngine;
using System;

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
            var prefab = PrefabCollection<VehicleInfo>.FindLoaded(prefabName) ??
                         PrefabCollection<VehicleInfo>.FindLoaded(prefabName + "_Data") ??
                         PrefabCollection<VehicleInfo>.FindLoaded(PathEscaper.Escape(prefabName) + "_Data") ??
                         PrefabCollection<VehicleInfo>.FindLoaded(packageName + "." + prefabName + "_Data") ??
                         PrefabCollection<VehicleInfo>.FindLoaded(packageName + "." + PathEscaper.Escape(prefabName) + "_Data");

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
    }
}
