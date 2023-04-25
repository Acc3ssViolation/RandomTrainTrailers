using ColossalFramework.Math;
using HarmonyLib;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace RandomTrainTrailers.Detour
{
    /// <summary>
    /// Harmony is not capable of detouring Vehicle.Spawn(ushort vehicleID) (it causes an invalid IL exception)
    /// So this class prefixes the train and tram AIs to call our version of Spawn instead
    /// The prefixes are slightly modified copies of the TrySpawn methods and will never let the original run
    /// </summary>
    public static class HarmonyDetourAIs
    {
        private static bool _isPatched = false;

        public static void Deploy()
        {
            if (_isPatched)
                return;

            _isPatched = true;

            var harmony = new Harmony(Mod.harmonyPackage);
            Version currentVersion;
            if(Harmony.VersionInfo(out currentVersion).ContainsKey(Mod.harmonyPackage))
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

            RedirectionHelper.RedirectCalls(
                typeof(TramAI_Detour).GetMethod("InitializePath", BindingFlags.Static | BindingFlags.Public),
                typeof(TramBaseAI).GetMethod("InitializePath", BindingFlags.Static | BindingFlags.NonPublic));

            Util.Log("Finished redirecting mod to game");

            // Harmony

            void PatchPostfix(MethodInfo method, MethodInfo postfix)
            {
                Util.Log(method?.DeclaringType.Name + "." + method?.Name + " is " + (method == null ? "null" : "not null"));
                Util.Log(postfix?.DeclaringType.Name + "." + postfix?.Name + " is " + (postfix == null ? "null" : "not null"));

                harmony.Patch(method, null, new HarmonyMethod(postfix), null);
            }

            Util.Log("Patching AI methods...", true);

            // We will randomize cargo trains whenever the variations get refreshed
            PatchPostfix(
                typeof(CargoTrainAI).GetMethod("RefreshVariations", BindingFlags.Instance | BindingFlags.NonPublic),
                typeof(CargoTrainAI_Detour).GetMethod("RefreshVariations_Postfix", BindingFlags.Static | BindingFlags.Public));

            // We will randomize regular trains and metros on spawn only
            PatchPostfix(
                typeof(TrainAI).GetMethod("TrySpawn", BindingFlags.Instance | BindingFlags.Public),
                typeof(TrainAI_Detour).GetMethod("TrySpawn_Postfix", BindingFlags.Static | BindingFlags.Public));

            // And do the same for trams
            PatchPostfix(
                typeof(TramBaseAI).GetMethod("TrySpawn", BindingFlags.Instance | BindingFlags.Public),
                typeof(TramAI_Detour).GetMethod("TrySpawn_Postfix", BindingFlags.Static | BindingFlags.Public));

            Util.Log("Harmony patches applied", true);
        }

        public static void Revert()
        {
            if (!_isPatched)
                return;

            // Reverting redirects isn't really necessary as we only patch the mod assembly, not the game itself
            Util.Log("(Not) Reverting redirects...", true);

            Util.Log("Unpatching Harmony patches...", true);

            var harmony = new Harmony(Mod.harmonyPackage);
            harmony.UnpatchAll(Mod.harmonyPackage);

            Util.Log("Harmony patches reverted", true);

            _isPatched = false;
        }

        class TramAI_Detour
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            public static void InitializePath(ushort vehicleID, ref Vehicle vehicleData)
            {
                Util.Log("InitializePath was called even though it is supposed to be redirected to game!");
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            public static void TrySpawn_Postfix(object __instance, bool __result, ushort vehicleID, ref Vehicle vehicleData)
            {
                if (!__result)
                {
                    // Vehicle wasn't spawned, can't randomize
                    return;
                }

                if (Randomize(vehicleID, ref vehicleData))
                    InitializePath(vehicleID, ref vehicleData);
            }
        }

        class TrainAI_Detour
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            public static void InitializePath(ushort vehicleID, ref Vehicle vehicleData)
            {
                Util.Log("InitializePath was called even though it is supposed to be redirected to game!");
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            public static void TrySpawn_Postfix(object __instance, bool __result, ushort vehicleID, ref Vehicle vehicleData)
            {
                if (!__result)
                {
                    // Vehicle wasn't spawned, can't randomize
                    return;
                }

                if (__instance?.GetType() == typeof(CargoTrainAI))
                {
                    // We already patch CargoTrainAI in a different way
                    return;
                }

                if (Randomize(vehicleID, ref vehicleData))
                    InitializePath(vehicleID, ref vehicleData);
            }
        }

        class CargoTrainAI_Detour
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            public static void RefreshVariations_Postfix(ushort vehicleID, ref Vehicle vehicleData)
            {
                if (Randomize(vehicleID, ref vehicleData))
                    TrainAI_Detour.InitializePath(vehicleID, ref vehicleData);
            }
        }

        private static bool Randomize(ushort vehicleID, ref Vehicle vehicleData)
        {
            // Only use valid leading vehicles
            if (vehicleID != 0 && vehicleData.m_leadingVehicle == 0)
            {
                var config = TrailerManager.GetVehicleConfig(vehicleData.Info.name);
                if (config != null)
                {
                    var randomizer = new Randomizer(Time.frameCount * (long)vehicleID);
                    if (randomizer.Int32(100) < config.RandomTrailerChance)
                    {
                        TrailerRandomizer.RandomizeTrailers(ref vehicleData, vehicleID, config, randomizer);
                        return true;
                    }
                }
                else
                {
                    var pools = TrailerManager.GetVehiclePools(vehicleData.Info.name);
                    if (pools == null)
                        return false;

                    var randomizer = new Randomizer(Time.frameCount * (long)vehicleID);
                    var poolIndex = randomizer.Int32((uint)pools.Count);
                    var pool = pools[poolIndex];
                    TrailerRandomizer.GenerateTrain(ref vehicleData, vehicleID, pool, randomizer);
                    return true;
                }
            }
            return false;
        }
    }

}
