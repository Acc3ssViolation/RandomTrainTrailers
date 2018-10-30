using ColossalFramework;
using ColossalFramework.Math;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace RandomTrainTrailers.Detour
{
    /// <summary>
    /// Harmony is not capable of detouring Vehicle.Spawn(ushort vehicleID) (it causes an invalid IL exception)
    /// So this class prefixes the train and tram AIs to call our version of Spawn instead
    /// The prefixes are slightly modified copies of the TrySpawn methods and will never let the original run
    /// </summary>
    public class HarmonyDetourAIs
    {
        public void Deploy()
        {
            var harmony = HarmonyInstance.Create(Mod.harmonyPackage);
            Version currentVersion;
            if(harmony.VersionInfo(out currentVersion).ContainsKey(Mod.harmonyPackage))
            {
                Util.LogWarning("Harmony patches already present");
                return;
            }
            Util.Log("Harmony v" + currentVersion, true);

            // Mod to game
            Util.Log("Redirecting mod to game");

            RedirectionHelper.RedirectCalls(
                typeof(TrainAI_Detour).GetMethod("InitializePath", BindingFlags.Static | BindingFlags.Public), 
                typeof(TrainAI).GetMethod("InitializePath", BindingFlags.Static | BindingFlags.NonPublic));

            Util.Log("Finished redirecting mod to game");

            // Harmony

            // TODO: Trams, trucks, passenger trains, that sort of stuff
            // We will randomize the train whenever the variations get refreshed
            var cargoTrainAIRefreshVariations = typeof(CargoTrainAI).GetMethod("RefreshVariations", BindingFlags.Instance | BindingFlags.NonPublic);
            var cargoTrainAIPost = typeof(CargoTrainAI_Detour).GetMethod("RefreshVariations_Postfix", BindingFlags.Static | BindingFlags.Public);

            Util.Log("CargoTrainAI is " + (cargoTrainAIRefreshVariations == null ? "null" : "not null"));
            Util.Log("CargoTrainAI_Detour post is " + (cargoTrainAIPost == null ? "null" : "not null"));

            Util.Log("Patching AI methods...", true);

            harmony.Patch(cargoTrainAIRefreshVariations, null, new HarmonyMethod(cargoTrainAIPost), null);

            Util.Log("Harmony patches applied", true);
        }

        internal void Revert()
        {
            // TODO: Actually revert when that becomes possible
            Util.Log("(Not) Reverting redirects...", true);
        }

        class TrainAI_Detour
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            public static void InitializePath(ushort vehicleID, ref Vehicle vehicleData)
            {
                Util.Log("InitializePath was called even though it is supposed to be redirected to game!");
            }
        }

        class CargoTrainAI_Detour
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            public static void RefreshVariations_Postfix(ushort vehicleID, ref Vehicle vehicleData)
            {
                // Only use valid leading vehicles
                if(vehicleID != 0 && vehicleData.m_leadingVehicle == 0)
                {
                    var config = TrailerManager.GetVehicleConfig(vehicleData.Info.name);
                    if(config != null)
                    {
                        var randomizer = new Randomizer(vehicleID);
                        if(randomizer.Int32(100) < config.RandomTrailerChance)
                        {
                            TrailerRandomizer.RandomizeTrailers(ref vehicleData, vehicleID, config, randomizer);
                            TrainAI_Detour.InitializePath(vehicleID, ref vehicleData);
                        }
                    }
                }
            }
        }
    }

}
