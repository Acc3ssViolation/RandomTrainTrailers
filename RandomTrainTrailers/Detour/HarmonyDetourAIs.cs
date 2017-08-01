using ColossalFramework;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

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
            Util.Log("Harmony v" + currentVersion);

            // Redirects (mod to game)

            Util.Log("Redirecting for access to private methods...");

            RedirectionHelper.RedirectCalls
            (
                typeof(TrainAI_Detour).GetMethod("PublicInitializePath", BindingFlags.Public | BindingFlags.Static),
                typeof(TrainAI).GetMethod("InitializePath", BindingFlags.NonPublic | BindingFlags.Static)
            );

            RedirectionHelper.RedirectCalls
            (
                typeof(TramBaseAI_Detour).GetMethod("PublicInitializePath", BindingFlags.Public | BindingFlags.Static),
                typeof(TramBaseAI).GetMethod("InitializePath", BindingFlags.NonPublic | BindingFlags.Static)
            );

            Util.Log("Redirections completed");

            // Harmony

            var trainAISrc = typeof(TrainAI).GetMethod("TrySpawn");
            var trainAIPre = typeof(TrainAI_Detour).GetMethod("TrainAI_Prefix_TrySpawn", BindingFlags.Static | BindingFlags.Public);

            var tramBaseAISrc = typeof(TramBaseAI).GetMethod("TrySpawn");
            var tramBaseAIPre = typeof(TramBaseAI_Detour).GetMethod("TramBaseAI_Prefix_TrySpawn", BindingFlags.Static | BindingFlags.Public);

            Util.Log("TrainAI.TrySpawn is " + (trainAISrc == null ? "null" : "not null"));
            Util.Log("TramBaseAI.TrySpawn is " + (tramBaseAISrc == null ? "null" : "not null"));

            Util.Log("TrainAI_Detour.TrySpawn is " + (trainAIPre == null ? "null" : "not null"));
            Util.Log("TramBaseAI_Detour.TrySpawn is " + (tramBaseAIPre == null ? "null" : "not null"));


            Util.Log("Patching Vehicle.Spawn...");

            harmony.Patch(trainAISrc, new HarmonyMethod(trainAIPre), null);
            harmony.Patch(tramBaseAISrc, new HarmonyMethod(tramBaseAIPre), null);

            Util.Log("Harmony patches applied");





            // Test
            /*var testSrc = typeof(Detour_Test).GetMethod("AlwaysReturnsTrueIfTwoOrFour");
            var testPre = typeof(Detour_Test).GetMethod("PrefixFalseIfTwo");

            Util.Log("Testing before Harmony...");
            Detour_Test.DoTest();

            harmony.Patch(testSrc, new HarmonyMethod(testPre), null);

            Util.Log("Testing after Harmony...");
            Detour_Test.DoTest();*/
        }

        internal void Revert()
        {
            Util.Log("Reverting redirects...");
        }

        /*class Detour_Test
        {
            public bool AlwaysReturnsTrueIfTwoOrFour(int number)
            {
                return number == 2 || number == 4;
            }

            public static bool PrefixFalseIfTwo(ref bool __result, int number)
            {
                if(number == 3)
                {
                    __result = true;
                    return false;
                }
                return true;
            }

            public static void DoTest()
            {
                var test = new Detour_Test();
                Util.Log("Testing... number is 2, result is " + test.AlwaysReturnsTrueIfTwoOrFour(2));
                Util.Log("Testing... number is 3, result is " + test.AlwaysReturnsTrueIfTwoOrFour(3));
                Util.Log("Testing... number is 4, result is " + test.AlwaysReturnsTrueIfTwoOrFour(4));
            }
        }*/

        class TrainAI_Detour
        {
            [MethodImpl(MethodImplOptions.NoInlining)] //to prevent inlining
            public static void PublicInitializePath(ushort vehicleID, ref Vehicle vehicleData)
            {
                //This line is crucial for success! We can't detour empty or too simple methods
                UnityEngine.Debug.Log($"Class: This is a static method. Instance type: {typeof(TrainAI_Detour).ToString() ?? "Null"}. Arg: {vehicleID}");
            }

            [MethodImpl(MethodImplOptions.NoInlining)] //to prevent inlining
            public static bool TrainAI_Prefix_TrySpawn(ref bool __result, ushort vehicleID, ref Vehicle vehicleData)
            {
                if((vehicleData.m_flags & Vehicle.Flags.Spawned) != (Vehicle.Flags)0)
                {
                    // Since we never execute the original method (by returning false) we should modify the result to what the original would have done.
                    // So where the original did return true, we do __result = true
                    __result = true;
                    return false;
                }
                if(vehicleData.m_path != 0u)
                {
                    PathManager instance = Singleton<PathManager>.instance;
                    PathUnit.Position pathPos;
                    if(instance.m_pathUnits.m_buffer[(int)((UIntPtr)vehicleData.m_path)].GetPosition(0, out pathPos))
                    {
                        uint laneID = PathManager.GetLaneID(pathPos);
                        if(laneID != 0u && !Singleton<NetManager>.instance.m_lanes.m_buffer[(int)((UIntPtr)laneID)].CheckSpace(1000f, vehicleID))
                        {
                            vehicleData.m_flags |= Vehicle.Flags.WaitingSpace;
                            __result = false;
                            return false;
                        }
                    }
                }
                // The commented lines have been replaced with the ones below
                //vehicleData.Spawn(vehicleID);
                VehiclePatch_Spawn.Spawn_Imp(ref vehicleData, vehicleID);
                // Call the original method to ensure TMPE can do what it has to do. If they ever switch to Harmony postfixes we can remove this line.
                vehicleData.Spawn(vehicleID);
                vehicleData.m_flags &= ~Vehicle.Flags.WaitingSpace;
                //TrainAI.InitializePath(vehicleID, ref vehicleData);
                PublicInitializePath(vehicleID, ref vehicleData);
                __result = false;
                return false;
            }
        }

        class TramBaseAI_Detour
        {
            [MethodImpl(MethodImplOptions.NoInlining)] //to prevent inlining
            public static void PublicInitializePath(ushort vehicleID, ref Vehicle vehicleData)
            {
                //This line is crucial for success! We can't detour empty or too simple methods
                UnityEngine.Debug.Log($"Class: This is a static method. Instance type: {typeof(TramBaseAI_Detour).ToString() ?? "Null"}. Arg: {vehicleID}");
            }

            [MethodImpl(MethodImplOptions.NoInlining)] //to prevent inlining
            public static bool TramBaseAI_Prefix_TrySpawn(ref bool __result, ushort vehicleID, ref Vehicle vehicleData)
            {
                if((vehicleData.m_flags & Vehicle.Flags.Spawned) != (Vehicle.Flags)0)
                {
                    // Since we never execute the original method (by returning false) we should modify the result to what the original would have done.
                    // So where the original did return true, we do __result = true
                    __result = true;
                    return false;
                }
                if(vehicleData.m_path != 0u)
                {
                    PathManager instance = Singleton<PathManager>.instance;
                    PathUnit.Position pathPos;
                    if(instance.m_pathUnits.m_buffer[(int)((UIntPtr)vehicleData.m_path)].GetPosition(0, out pathPos))
                    {
                        uint laneID = PathManager.GetLaneID(pathPos);
                        if(laneID != 0u && !Singleton<NetManager>.instance.m_lanes.m_buffer[(int)((UIntPtr)laneID)].CheckSpace(1000f, vehicleID))
                        {
                            vehicleData.m_flags |= Vehicle.Flags.WaitingSpace;
                            __result = false;
                            return false;
                        }
                    }
                }
                // The commented lines have been replaced with the ones below
                //vehicleData.Spawn(vehicleID);
                VehiclePatch_Spawn.Spawn_Imp(ref vehicleData, vehicleID);
                // Call the original method to ensure TMPE can do what it has to do. If they ever switch to Harmony postfixes we can remove this line.
                vehicleData.Spawn(vehicleID);
                vehicleData.m_flags &= ~Vehicle.Flags.WaitingSpace;
                //TramBaseAI.InitializePath(vehicleID, ref vehicleData);
                PublicInitializePath(vehicleID, ref vehicleData);
                __result = false;
                return false;
            }
        }
    }
}
