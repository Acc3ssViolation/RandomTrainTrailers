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

            // Redirects (mod to game)

            Util.Log("Redirecting for access to private methods...", true);

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

            Util.Log("Redirections completed", true);

            // Harmony

            var trainAISrc = typeof(TrainAI).GetMethod("TrySpawn");
            var trainAIPre = typeof(TrainAI_Detour).GetMethod("TrainAI_Prefix_TrySpawn", BindingFlags.Static | BindingFlags.Public);

            var tramBaseAISrc = typeof(TramBaseAI).GetMethod("TrySpawn");
            var tramBaseAIPre = typeof(TramBaseAI_Detour).GetMethod("TramBaseAI_Prefix_TrySpawn", BindingFlags.Static | BindingFlags.Public);

            var cargoTrainAISrc = typeof(CargoTrainAI).GetMethod("SimulationStep", BindingFlags.Instance | BindingFlags.Public, null, new[] { typeof(ushort), typeof(Vehicle).MakeByRefType(), typeof(Vector3) }, null);
            var cargoTrainAIPre = typeof(CargoTrainAI_Detour).GetMethod("SimulationStep_Prefix", BindingFlags.Static | BindingFlags.Public);
            var cargoTrainAIPost = typeof(CargoTrainAI_Detour).GetMethod("SimulationStep_Postfix", BindingFlags.Static | BindingFlags.Public);


            Util.Log("TrainAI.TrySpawn is " + (trainAISrc == null ? "null" : "not null"));
            Util.Log("TramBaseAI.TrySpawn is " + (tramBaseAISrc == null ? "null" : "not null"));

            Util.Log("TrainAI_Detour.TrySpawn is " + (trainAIPre == null ? "null" : "not null"));
            Util.Log("TramBaseAI_Detour.TrySpawn is " + (tramBaseAIPre == null ? "null" : "not null"));

            Util.Log("CargoTrainAI is " + (cargoTrainAISrc == null ? "null" : "not null"));
            Util.Log("CargoTrainAI_Detour pre is " + (cargoTrainAIPre == null ? "null" : "not null"));
            Util.Log("CargoTrainAI_Detour post is " + (cargoTrainAIPost == null ? "null" : "not null"));

            Util.Log("Patching AI methods...", true);

            harmony.Patch(trainAISrc, new HarmonyMethod(trainAIPre), null);
            harmony.Patch(tramBaseAISrc, new HarmonyMethod(tramBaseAIPre), null);
            harmony.Patch(cargoTrainAISrc, new HarmonyMethod(cargoTrainAIPre), new HarmonyMethod(cargoTrainAIPost), null);

            Util.Log("Harmony patches applied", true);





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
            // TODO: Actually revert when that becomes possible
            Util.Log("Reverting redirects...", true);
        }

        class TrainAI_Detour
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            public static void PublicInitializePath(ushort vehicleID, ref Vehicle vehicleData)
            {
                //This line is crucial for success! We can't detour empty or too simple methods
                UnityEngine.Debug.Log($"Class: This is a static method. Instance type: {typeof(TrainAI_Detour).ToString() ?? "Null"}. Arg: {vehicleID}");
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
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
            [MethodImpl(MethodImplOptions.NoInlining)]
            public static void PublicInitializePath(ushort vehicleID, ref Vehicle vehicleData)
            {
                //This line is crucial for success! We can't detour empty or too simple methods
                UnityEngine.Debug.Log($"Class: This is a static method. Instance type: {typeof(TramBaseAI_Detour).ToString() ?? "Null"}. Arg: {vehicleID}");
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
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

        class CargoTrainAI_Detour
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            public static void SimulationStep_Prefix(out Vehicle.Flags __state, ushort vehicleID, ref Vehicle data)
            {
                __state = data.m_flags;
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            public static void SimulationStep_Postfix(Vehicle.Flags __state, ushort vehicleID, ref Vehicle data)
            {
                if((((__state & Vehicle.Flags.WaitingLoading) > 0) && ((data.m_flags & Vehicle.Flags.WaitingLoading) == 0)))
                {
                    DoTrailerModification(vehicleID, ref data);
                }
            }

            /// <summary>
            /// Modifies a vehicle's trailers to reflect their cargo type if certain conditions are met.
            /// </summary>
            /// <param name="vehicleID"></param>
            /// <param name="vehicleData"></param>
            public static void DoTrailerModification(ushort vehicleID, ref Vehicle vehicleData)
            {
                if(Mod.enableUseCargo && VehicleManager.instance.m_vehicles.m_buffer[vehicleID].m_leadingVehicle == 0)
                {
                    // Change trailers based on cargo
                    // Thanks to Cargo Info
                    var ai = VehicleManager.instance.m_vehicles.m_buffer[vehicleID].Info.m_vehicleAI;

                    // Select random trailer collection to use
                    var def = TrailerManager.GetVehicleConfig(ai.m_info.name);
                    if(def != null && def.UseCargoContents)
                    {
                        var cargoData = def.GetRandomCollection().m_cargoData;
                        if(cargoData != null)
                        {
                            var cargo = VehicleManager.instance.m_vehicles.m_buffer[vehicleID].m_firstCargo;
                            var result = new float[CargoParcel.ResourceTypes.Length];
                            var guard = 0;
                            while(cargo != 0)
                            {
                                var parcel = new CargoParcel(0, false,
                                    VehicleManager.instance.m_vehicles.m_buffer[cargo].m_transferType,
                                    VehicleManager.instance.m_vehicles.m_buffer[cargo].m_transferSize,
                                    VehicleManager.instance.m_vehicles.m_buffer[cargo].m_flags);
                                result[parcel.ResourceType] += parcel.transferSize;

                                cargo = VehicleManager.instance.m_vehicles.m_buffer[cargo].m_nextCargo;
                                guard++;
                                if(guard > ushort.MaxValue)
                                {
                                    Util.LogError("Invalid list detected!");
                                    return;
                                }
                            }
                            var total = result.Sum();
                           /*Util.Log("Relative cargo contents for " + Util.GetVehicleDisplayName(ai.m_info.name) + @"
Cargo types are:

Oil,
Petrol,
Ore,
Coal,
Logs,
Lumber,
Grain,
Food,
Goods");*/
                            if(total == 0.0f)
                            {
                                //Util.Log("Empty train!");
                                return;
                            }

                            //Util.Log(string.Join(", ", result.Select(v => v / total).Select(f => f.ToString()).ToArray()));
                            if(Math.Abs(total) < 1f) total = 1f;
                            var normalizedResults = result.Select(v => v / total).ToArray();

                           // Util.Log("Changing trailer composition to reflect cargo contents...");
                            var vehicle = VehicleManager.instance.m_vehicles.m_buffer[vehicleID];
                            int trailerIndex = 0;

                            // The amount of randomizable trailers (end offset is not included for now)
                            var trailerCount = vehicle.GetTrailerCount(vehicleID) - def.StartOffset;

                            //Util.Log("Trailer count: " + trailerCount);

                            var trailerCounts = new int[normalizedResults.Length];
                            for(int i = 0; i < trailerCounts.Length; i++)
                            {
                                trailerCounts[i] = (int)Math.Floor(normalizedResults[i] * trailerCount);
                            }

                            // Ensure we have the original amount of trailers in total
                            while(trailerCounts.Sum() < trailerCount)
                            {
                                for(int i = 0; i < trailerCounts.Length; i++)
                                {
                                    if(trailerCounts[i] > 0)
                                    {
                                        trailerCounts[i]++;
                                        break;
                                    }
                                }
                            }

                            //Util.Log("Trailer counts: " + string.Join(", ", trailerCounts.Select(i => i.ToString()).ToArray()));

                            // Randomize cargo type order in the train
                            int cargoIndex = 0;
                            int orderIndex = 0;
                            int[] cargoOrder = new int[trailerCounts.Length];
                            for(int i = 0; i < cargoOrder.Length; i++)
                            {
                                cargoOrder[i] = i;
                            }
                            cargoOrder = cargoOrder.OrderBy(i => Util.Random.Next()).ToArray();

                            cargoIndex = cargoOrder[orderIndex];

                            // Change the trailers
                            while(vehicle.m_trailingVehicle != 0)
                            {
                                if(trailerIndex >= def.StartOffset/* && trailerIndex < trailerCount - def.EndOffset*/)
                                {
                                    while(trailerCounts[cargoIndex] <= 0)
                                    {
                                        orderIndex++;
                                        if(orderIndex > cargoOrder.Length)
                                        {
                                            Util.LogError("Too many trailers!");
                                            return;
                                        }
                                        cargoIndex = cargoOrder[orderIndex];
                                    }

                                    TrailerDefinition.Trailer trailerDef = null;

                                    int randomIndex = cargoData.GetRandomTrailerIndex(cargoIndex);
                                    if(randomIndex >= 0)
                                    {
                                        trailerDef = cargoData.m_trailers[cargoIndex][randomIndex];
                                    }
                                    else
                                    {
                                        for(int i = 1; i < CargoParcel.ResourceFallback[cargoIndex].Length; i++)
                                        {
                                            int fallbackCargoIndex = CargoParcel.LowestFlagToIndex(CargoParcel.ResourceFallback[cargoIndex][i]);
                                            randomIndex = cargoData.GetRandomTrailerIndex(fallbackCargoIndex);
                                            if(randomIndex >= 0)
                                            {
                                                trailerDef = cargoData.m_trailers[fallbackCargoIndex][randomIndex];
                                                break;
                                            }
                                        }

                                        if(trailerDef == null)
                                        {
                                            Util.LogError("Could not find fallback cargo wagon for type " + CargoParcel.ResourceTypes[cargoIndex] + " this should not happen!");
                                            return;
                                        }
                                    }

                                    if(trailerDef.IsMultiTrailer())
                                    {
                                        vehicle.m_trailingVehicle = ForceMultiTrailer(vehicle.m_trailingVehicle, trailerDef);
                                        trailerCounts[cargoIndex] -= trailerDef.SubTrailers.Count;
                                        trailerIndex += trailerDef.SubTrailers.Count - 1;   // 1 is always added at the bottom of the loop
                                    }
                                    else
                                    {
                                        ChangeTrailer(vehicle.m_trailingVehicle, trailerDef);
                                        trailerCounts[cargoIndex]--;
                                    }
                                }
                                vehicle = VehicleManager.instance.m_vehicles.m_buffer[vehicle.m_trailingVehicle];
                                trailerIndex++;
                            }

                            // Clean up trailer positions
                            RepositionTrailers(vehicleID);
                            TrainAI_Detour.PublicInitializePath(vehicleID, ref vehicleData);
                        }
                    }
                }
            }

            /// <summary>
            /// Spawns an amount of trailers behind the leading vehicle/trailer. The info of the leading vehicle is used!
            /// </summary>
            /// <param name="leadingVehicleID">Id of the leading vehicle</param>
            /// <param name="amount">Amount of trailers to spawn</param>
            private static void SpawnTrailers(ushort leadingVehicleID, int amount)
            {
                Util.Log("Spawning " + amount + " new trailers after [" + leadingVehicleID + "]");

                var instance = VehicleManager.instance;
                var _this = instance.m_vehicles.m_buffer[leadingVehicleID];
                var info = _this.Info;

                bool hasVerticalTrailers = info.m_vehicleAI.VerticalTrailers();
                ushort prevId = leadingVehicleID;
                bool isReversed = (instance.m_vehicles.m_buffer[prevId].m_flags & Vehicle.Flags.Reversed) != (Vehicle.Flags)0;
                Vehicle.Frame lastFrameData = _this.GetLastFrameData();
                float zPos = (!hasVerticalTrailers) ? (info.m_generatedInfo.m_size.z * 0.5f) : 0f;
                zPos -= (((_this.m_flags & Vehicle.Flags.Inverted) == (Vehicle.Flags)0) ? info.m_attachOffsetBack : info.m_attachOffsetFront);
                Randomizer randomizer = new Randomizer(leadingVehicleID);

                for(int i = 0; i < amount; i++)
                {
                    VehicleInfo trailerInfo = info;
                    bool isInverted = false;
                    zPos += ((!hasVerticalTrailers) ? (trailerInfo.m_generatedInfo.m_size.z * 0.5f) : trailerInfo.m_generatedInfo.m_size.y);
                    zPos -= ((!isInverted) ? trailerInfo.m_attachOffsetFront : trailerInfo.m_attachOffsetBack);
                    Vector3 position = lastFrameData.m_position - lastFrameData.m_rotation * new Vector3(0f, (!hasVerticalTrailers) ? 0f : zPos, (!hasVerticalTrailers) ? zPos : 0f);
                    ushort trailerId;
                    if(instance.CreateVehicle(out trailerId, ref Singleton<SimulationManager>.instance.m_randomizer, trailerInfo, position, (TransferManager.TransferReason)_this.m_transferType, false, false))
                    {
                        instance.m_vehicles.m_buffer[prevId].m_trailingVehicle = trailerId;
                        instance.m_vehicles.m_buffer[trailerId].m_leadingVehicle = prevId;
                        if(isInverted)
                        {
                            Vehicle[] vehicleBuffer = instance.m_vehicles.m_buffer;
                            vehicleBuffer[trailerId].m_flags = (vehicleBuffer[trailerId].m_flags | Vehicle.Flags.Inverted);
                        }
                        if(isReversed)
                        {
                            Vehicle[] vehicleBuffer = instance.m_vehicles.m_buffer;
                            vehicleBuffer[trailerId].m_flags = (vehicleBuffer[trailerId].m_flags | Vehicle.Flags.Reversed);
                        }
                        instance.m_vehicles.m_buffer[trailerId].m_frame0.m_rotation = lastFrameData.m_rotation;
                        instance.m_vehicles.m_buffer[trailerId].m_frame1.m_rotation = lastFrameData.m_rotation;
                        instance.m_vehicles.m_buffer[trailerId].m_frame2.m_rotation = lastFrameData.m_rotation;
                        instance.m_vehicles.m_buffer[trailerId].m_frame3.m_rotation = lastFrameData.m_rotation;
                        trailerInfo.m_vehicleAI.FrameDataUpdated(trailerId, ref instance.m_vehicles.m_buffer[trailerId], ref instance.m_vehicles.m_buffer[trailerId].m_frame0);
                        instance.m_vehicles.m_buffer[trailerId].Spawn(trailerId);
                        prevId = trailerId;
                    }
                    zPos += ((!hasVerticalTrailers) ? (trailerInfo.m_generatedInfo.m_size.z * 0.5f) : 0f);
                    zPos -= ((!isInverted) ? trailerInfo.m_attachOffsetBack : trailerInfo.m_attachOffsetFront);
                }
            }

            /// <summary>
            /// Changes the trailers following the given trailer into a multi-trailer. Spawns extra trailers if required.
            /// </summary>
            /// <param name="firstTrailerID"></param>
            /// <param name="multiTrailer"></param>
            /// <returns></returns>
            private static ushort ForceMultiTrailer(ushort firstTrailerID, TrailerDefinition.Trailer multiTrailer)
            {
                var availableVehicles = VehicleManager.instance.m_vehicles.m_buffer[firstTrailerID].GetTrailerCount(firstTrailerID) + 1;
                if(availableVehicles < multiTrailer.SubTrailers.Count)
                {
                    SpawnTrailers(firstTrailerID, multiTrailer.SubTrailers.Count - availableVehicles);
                }

                ushort lastVehicleID = 0;
                int subTrailerIndex = 0;

                while(subTrailerIndex < multiTrailer.SubTrailers.Count)
                {
                    ChangeTrailer(firstTrailerID, multiTrailer.SubTrailers[subTrailerIndex]);
                    
                    lastVehicleID = firstTrailerID;
                    firstTrailerID = VehicleManager.instance.m_vehicles.m_buffer[firstTrailerID].m_trailingVehicle;
                    if(firstTrailerID == 0 && subTrailerIndex < multiTrailer.SubTrailers.Count - 1)
                    {
                        Util.LogError("Unexpectend end of train when spawning multi-trailer, last vehicle was [" + lastVehicleID + "]");
                        subTrailerIndex = 9999;
                    }
                    subTrailerIndex++;
                }

                return lastVehicleID;
            }

            /// <summary>
            /// Changes the given vehicle to the info and settings specified by the trailer data.
            /// </summary>
            /// <param name="vehicleID"></param>
            /// <param name="trailerDefinition"></param>
            private static void ChangeTrailer(ushort vehicleID, TrailerDefinition.Trailer trailerDefinition)
            {
                VehicleManager.instance.m_vehicles.m_buffer[vehicleID].Info = trailerDefinition.GetInfo();
                // TODO: Randomize this properly
                if(trailerDefinition.InvertProbability > 50)
                {
                    VehicleManager.instance.m_vehicles.m_buffer[vehicleID].m_flags |= Vehicle.Flags.Inverted;
                }
                else
                {
                    VehicleManager.instance.m_vehicles.m_buffer[vehicleID].m_flags &= ~Vehicle.Flags.Inverted;
                }
            }

            /// <summary>
            /// Resets the vehicle's trailers into their spawn position, should avoid trains getting stuck after a consist change
            /// </summary>
            /// <param name="leadingVehicleID">The id of the leading vehicle (engine)</param>
            private static void RepositionTrailers(ushort leadingVehicleID)
            {
                var instance = VehicleManager.instance;
                var _this = instance.m_vehicles.m_buffer[leadingVehicleID];
                var info = _this.Info;
                var trailerCount = _this.GetTrailerCount(leadingVehicleID);

                bool hasVerticalTrailers = info.m_vehicleAI.VerticalTrailers();
                ushort prevId = leadingVehicleID;
                Vehicle.Frame lastFrameData = _this.GetLastFrameData();
                float zPos = (!hasVerticalTrailers) ? (info.m_generatedInfo.m_size.z * 0.5f) : 0f;
                zPos -= (((_this.m_flags & Vehicle.Flags.Inverted) == (Vehicle.Flags)0) ? info.m_attachOffsetBack : info.m_attachOffsetFront);

                for(int i = 0; i < trailerCount; i++)
                {
                    ushort trailerId = instance.m_vehicles.m_buffer[prevId].m_trailingVehicle;

                    VehicleInfo trailerInfo = instance.m_vehicles.m_buffer[trailerId].Info;
                    bool isInverted = (instance.m_vehicles.m_buffer[trailerId].m_flags & Vehicle.Flags.Inverted) > 0;

                    zPos += ((!hasVerticalTrailers) ? (trailerInfo.m_generatedInfo.m_size.z * 0.5f) : trailerInfo.m_generatedInfo.m_size.y);
                    zPos -= ((!isInverted) ? trailerInfo.m_attachOffsetFront : trailerInfo.m_attachOffsetBack);
                    Vector3 position = lastFrameData.m_position - lastFrameData.m_rotation * new Vector3(0f, (!hasVerticalTrailers) ? 0f : zPos, (!hasVerticalTrailers) ? zPos : 0f);

                    // Remove from grid BEFORE changing position, otherwise it will mess things up horribly
                    instance.RemoveFromGrid(trailerId, ref instance.m_vehicles.m_buffer[trailerId], trailerInfo.m_isLargeVehicle);

                    instance.m_vehicles.m_buffer[trailerId].m_frame0 = new Vehicle.Frame(position, Quaternion.identity);
                    instance.m_vehicles.m_buffer[trailerId].m_frame1 = new Vehicle.Frame(position, Quaternion.identity);
                    instance.m_vehicles.m_buffer[trailerId].m_frame2 = new Vehicle.Frame(position, Quaternion.identity);
                    instance.m_vehicles.m_buffer[trailerId].m_frame3 = new Vehicle.Frame(position, Quaternion.identity);

                    instance.m_vehicles.m_buffer[trailerId].m_path = 0u;
                    instance.m_vehicles.m_buffer[trailerId].m_lastFrame = 0;
                    instance.m_vehicles.m_buffer[trailerId].m_pathPositionIndex = 0;
                    instance.m_vehicles.m_buffer[trailerId].m_lastPathOffset = 0;
                    instance.m_vehicles.m_buffer[trailerId].m_gateIndex = 0;
                    instance.m_vehicles.m_buffer[trailerId].m_waterSource = 0;

                    instance.m_vehicles.m_buffer[trailerId].m_frame0.m_rotation = lastFrameData.m_rotation;
                    instance.m_vehicles.m_buffer[trailerId].m_frame1.m_rotation = lastFrameData.m_rotation;
                    instance.m_vehicles.m_buffer[trailerId].m_frame2.m_rotation = lastFrameData.m_rotation;
                    instance.m_vehicles.m_buffer[trailerId].m_frame3.m_rotation = lastFrameData.m_rotation;

                    instance.m_vehicles.m_buffer[trailerId].m_targetPos0 = Vector4.zero;
                    instance.m_vehicles.m_buffer[trailerId].m_targetPos1 = Vector4.zero;
                    instance.m_vehicles.m_buffer[trailerId].m_targetPos2 = Vector4.zero;
                    instance.m_vehicles.m_buffer[trailerId].m_targetPos3 = Vector4.zero;
                    
                    instance.AddToGrid(trailerId, ref instance.m_vehicles.m_buffer[trailerId], trailerInfo.m_isLargeVehicle);


                    trailerInfo.m_vehicleAI.FrameDataUpdated(trailerId, ref instance.m_vehicles.m_buffer[trailerId], ref instance.m_vehicles.m_buffer[trailerId].m_frame0);

                    prevId = trailerId;

                    zPos += ((!hasVerticalTrailers) ? (trailerInfo.m_generatedInfo.m_size.z * 0.5f) : 0f);
                    zPos -= ((!isInverted) ? trailerInfo.m_attachOffsetBack : trailerInfo.m_attachOffsetFront);
                }
            }
        }
    }

    public static class VehicleExtensions
    {
        /// <summary>
        /// Returns the amount of trailing vehicles
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="vehicleID"></param>
        /// <returns></returns>
        public static int GetTrailerCount(this Vehicle _this, ushort vehicleID)
        {
            if(_this.m_trailingVehicle == 0)
            {
                return 0;
            }
            VehicleManager instance = Singleton<VehicleManager>.instance;
            ushort trailingVehicle = _this.m_trailingVehicle;
            int num = 0;
            while(trailingVehicle != 0)
            {
                vehicleID = trailingVehicle;
                trailingVehicle = instance.m_vehicles.m_buffer[vehicleID].m_trailingVehicle;
                if(++num > 16384)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
            return num;
        }
    }
}
