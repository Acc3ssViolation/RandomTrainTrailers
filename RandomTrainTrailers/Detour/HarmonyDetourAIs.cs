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
                        }
                    }
                }
            }
        }

//            /// <summary>
//            /// Modifies a vehicle's trailers to reflect their cargo type if certain conditions are met.
//            /// </summary>
//            /// <param name="vehicleID"></param>
//            /// <param name="vehicleData"></param>
//            public static void DoTrailerModification(ushort vehicleID, ref Vehicle vehicleData)
//            {
//                if(Mod.enableUseCargo && VehicleManager.instance.m_vehicles.m_buffer[vehicleID].m_leadingVehicle == 0)
//                {
//                    // Change trailers based on cargo
//                    // Thanks to Cargo Info
//                    var ai = VehicleManager.instance.m_vehicles.m_buffer[vehicleID].Info.m_vehicleAI;

//                    // Select random trailer collection to use
//                    var def = TrailerManager.GetVehicleConfig(ai.m_info.name);
//                    if(def != null && def.UseCargoContents)
//                    {
//                        var cargoData = def.GetRandomCollection().m_cargoData;
//                        if(cargoData != null)
//                        {
//                            var cargo = VehicleManager.instance.m_vehicles.m_buffer[vehicleID].m_firstCargo;
//                            var result = new float[CargoParcel.ResourceTypes.Length];
//                            var guard = 0;
//                            while(cargo != 0)
//                            {
//                                var parcel = new CargoParcel(0, false,
//                                    VehicleManager.instance.m_vehicles.m_buffer[cargo].m_transferType,
//                                    VehicleManager.instance.m_vehicles.m_buffer[cargo].m_transferSize,
//                                    VehicleManager.instance.m_vehicles.m_buffer[cargo].m_flags);
//                                result[parcel.ResourceType] += parcel.transferSize;

//                                cargo = VehicleManager.instance.m_vehicles.m_buffer[cargo].m_nextCargo;
//                                guard++;
//                                if(guard > ushort.MaxValue)
//                                {
//                                    Util.LogError("Invalid list detected!");
//                                    return;
//                                }
//                            }
//                            var total = result.Sum();
//                           /*Util.Log("Relative cargo contents for " + Util.GetVehicleDisplayName(ai.m_info.name) + @"
//Cargo types are:

//Oil,
//Petrol,
//Ore,
//Coal,
//Logs,
//Lumber,
//Grain,
//Food,
//Goods");*/
//                            if(total == 0.0f)
//                            {
//                                //Util.Log("Empty train!");
//                                return;
//                            }

//                            //Util.Log(string.Join(", ", result.Select(v => v / total).Select(f => f.ToString()).ToArray()));
//                            if(Math.Abs(total) < 1f) total = 1f;
//                            var normalizedResults = result.Select(v => v / total).ToArray();

//                            // Util.Log("Changing trailer composition to reflect cargo contents...");
//                            var vehicle = VehicleManager.instance.m_vehicles.m_buffer[vehicleID];

//                            // The amount of randomizable trailers
//                            var randomizedTrailerCount = vehicle.GetTrailerCount(vehicleID) - def.StartOffset - def.EndOffset;
//                            if(randomizedTrailerCount <= 0)
//                            {
//                                return;
//                            }

//                            //Util.Log("Trailer count: " + trailerCount);

//                            var trailerCounts = new int[normalizedResults.Length];
//                            for(int i = 0; i < trailerCounts.Length; i++)
//                            {
//                                trailerCounts[i] = (int)Math.Floor(normalizedResults[i] * randomizedTrailerCount);
//                            }

//                            // Ensure we have the original amount of trailers in total
//                            while(trailerCounts.Sum() < randomizedTrailerCount)
//                            {
//                                for(int i = 0; i < trailerCounts.Length; i++)
//                                {
//                                    if(trailerCounts[i] > 0)
//                                    {
//                                        trailerCounts[i]++;
//                                        break;
//                                    }
//                                }
//                            }

//                            //Util.Log("Trailer counts: " + string.Join(", ", trailerCounts.Select(i => i.ToString()).ToArray()));

//                            // Randomize cargo type order in the train
//                            int cargoIndex = 0;
//                            int orderIndex = 0;
//                            int[] cargoOrder = new int[trailerCounts.Length];
//                            for(int i = 0; i < cargoOrder.Length; i++)
//                            {
//                                cargoOrder[i] = i;
//                            }
//                            cargoOrder = cargoOrder.OrderBy(i => Util.Random.Next()).ToArray();

//                            cargoIndex = cargoOrder[orderIndex];

//                            // Change the trailers
//                            int trailerIndex = 0;
//                            while(vehicle.m_trailingVehicle != 0)
//                            {
//                                ushort nextVehicleId = vehicle.m_trailingVehicle;
//                                if(trailerIndex >= def.StartOffset && trailerIndex < def.StartOffset + randomizedTrailerCount)
//                                {
//                                    while(trailerCounts[cargoIndex] <= 0)
//                                    {
//                                        orderIndex++;
//                                        if(orderIndex > cargoOrder.Length)
//                                        {
//                                            Util.LogError("Too many trailers!");
//                                            return;
//                                        }
//                                        cargoIndex = cargoOrder[orderIndex];
//                                    }

//                                    TrailerDefinition.Trailer trailerDef = null;

//                                    int randomIndex = cargoData.GetRandomTrailerIndex(cargoIndex);
//                                    if(randomIndex >= 0)
//                                    {
//                                        trailerDef = cargoData.m_trailers[cargoIndex][randomIndex];
//                                    }
//                                    else
//                                    {
//                                        for(int i = 1; i < CargoParcel.ResourceFallback[cargoIndex].Length; i++)
//                                        {
//                                            int fallbackCargoIndex = CargoParcel.LowestFlagToIndex(CargoParcel.ResourceFallback[cargoIndex][i]);
//                                            randomIndex = cargoData.GetRandomTrailerIndex(fallbackCargoIndex);
//                                            if(randomIndex >= 0)
//                                            {
//                                                trailerDef = cargoData.m_trailers[fallbackCargoIndex][randomIndex];
//                                                break;
//                                            }
//                                        }

//                                        if(trailerDef == null)
//                                        {
//                                            Util.LogError("Could not find fallback cargo wagon for type " + CargoParcel.ResourceTypes[cargoIndex] + " this should not happen!");
//                                            return;
//                                        }
//                                    }

//                                    if(trailerDef.IsMultiTrailer())
//                                    {
//                                        nextVehicleId = ForceMultiTrailer(vehicle.m_trailingVehicle, trailerDef);
//                                        trailerCounts[cargoIndex] -= trailerDef.SubTrailers.Count;
//                                        trailerIndex += trailerDef.SubTrailers.Count - 1;   // 1 is always added at the bottom of the loop
//                                    }
//                                    else
//                                    {
//                                        ChangeTrailer(vehicle.m_trailingVehicle, trailerDef);
//                                        trailerCounts[cargoIndex]--;
//                                    }
//                                }
//                                vehicle = VehicleManager.instance.m_vehicles.m_buffer[nextVehicleId];
//                                trailerIndex++;
//                            }

//                            // Clean up trailer positions
//                            RepositionTrailers(vehicleID);
//                            TrainAI_Detour.PublicInitializePath(vehicleID, ref vehicleData);
//                        }
//                    }
//                }
//            }

//            /// <summary>
//            /// Spawns an amount of trailers behind the leading vehicle/trailer. The info of the leading vehicle is used!
//            /// </summary>
//            /// <param name="leadingVehicleID">Id of the leading vehicle</param>
//            /// <param name="amount">Amount of trailers to spawn</param>
//            private static void SpawnTrailers(ushort leadingVehicleID, int amount)
//            {
//                Util.Log("Spawning " + amount + " new trailers after [" + leadingVehicleID + "]");

//                List<ushort> spawnedIds = new List<ushort>();

//                var instance = VehicleManager.instance;
//                var _this = instance.m_vehicles.m_buffer[leadingVehicleID];
//                var info = _this.Info;

//                ushort lastSpawnHookupId = _this.m_trailingVehicle;

//                bool hasVerticalTrailers = info.m_vehicleAI.VerticalTrailers();
//                ushort prevId = leadingVehicleID;
//                bool isReversed = (instance.m_vehicles.m_buffer[prevId].m_flags & Vehicle.Flags.Reversed) != (Vehicle.Flags)0;
//                Vehicle.Frame lastFrameData = _this.GetLastFrameData();
//                float zPos = (!hasVerticalTrailers) ? (info.m_generatedInfo.m_size.z * 0.5f) : 0f;
//                zPos -= (((_this.m_flags & Vehicle.Flags.Inverted) == (Vehicle.Flags)0) ? info.m_attachOffsetBack : info.m_attachOffsetFront);
//                Randomizer randomizer = new Randomizer(leadingVehicleID);

//                for(int i = 0; i < amount; i++)
//                {
//                    VehicleInfo trailerInfo = info;
//                    bool isInverted = false;
//                    zPos += ((!hasVerticalTrailers) ? (trailerInfo.m_generatedInfo.m_size.z * 0.5f) : trailerInfo.m_generatedInfo.m_size.y);
//                    zPos -= ((!isInverted) ? trailerInfo.m_attachOffsetFront : trailerInfo.m_attachOffsetBack);
//                    Vector3 position = lastFrameData.m_position - lastFrameData.m_rotation * new Vector3(0f, (!hasVerticalTrailers) ? 0f : zPos, (!hasVerticalTrailers) ? zPos : 0f);
//                    ushort trailerId;
//                    if(instance.CreateVehicle(out trailerId, ref Singleton<SimulationManager>.instance.m_randomizer, trailerInfo, position, (TransferManager.TransferReason)_this.m_transferType, false, false))
//                    {
//                        instance.m_vehicles.m_buffer[prevId].m_trailingVehicle = trailerId;
//                        instance.m_vehicles.m_buffer[trailerId].m_leadingVehicle = prevId;
//                        instance.m_vehicles.m_buffer[trailerId].m_gateIndex = instance.m_vehicles.m_buffer[prevId].m_gateIndex;
//                        if(isInverted)
//                        {
//                            Vehicle[] vehicleBuffer = instance.m_vehicles.m_buffer;
//                            vehicleBuffer[trailerId].m_flags = (vehicleBuffer[trailerId].m_flags | Vehicle.Flags.Inverted);
//                        }
//                        if(isReversed)
//                        {
//                            Vehicle[] vehicleBuffer = instance.m_vehicles.m_buffer;
//                            vehicleBuffer[trailerId].m_flags = (vehicleBuffer[trailerId].m_flags | Vehicle.Flags.Reversed);
//                        }
//                        instance.m_vehicles.m_buffer[trailerId].m_frame0.m_rotation = lastFrameData.m_rotation;
//                        instance.m_vehicles.m_buffer[trailerId].m_frame1.m_rotation = lastFrameData.m_rotation;
//                        instance.m_vehicles.m_buffer[trailerId].m_frame2.m_rotation = lastFrameData.m_rotation;
//                        instance.m_vehicles.m_buffer[trailerId].m_frame3.m_rotation = lastFrameData.m_rotation;
//                        trailerInfo.m_vehicleAI.FrameDataUpdated(trailerId, ref instance.m_vehicles.m_buffer[trailerId], ref instance.m_vehicles.m_buffer[trailerId].m_frame0);
//                        instance.m_vehicles.m_buffer[trailerId].Spawn(trailerId);
//                        prevId = trailerId;
//                        spawnedIds.Add(trailerId);
//                    }
//                    else
//                    {
//                        Util.LogError("Not able to spawn trailer!");
//                    }
//                    zPos += ((!hasVerticalTrailers) ? (trailerInfo.m_generatedInfo.m_size.z * 0.5f) : 0f);
//                    zPos -= ((!isInverted) ? trailerInfo.m_attachOffsetBack : trailerInfo.m_attachOffsetFront);
//                }

//                // Hook up entire train again if we are inserting trailers in the middle of one
//                if(lastSpawnHookupId != 0)
//                {
//                    instance.m_vehicles.m_buffer[prevId].m_trailingVehicle = lastSpawnHookupId;
//                    instance.m_vehicles.m_buffer[lastSpawnHookupId].m_leadingVehicle = prevId;
//                }

//                var sb = new StringBuilder();
//                sb.Append("Spawned trailer IDs: ");
//                foreach(var id in spawnedIds)
//                {
//                    sb.Append(id + " ");
//                }
//                Util.Log(sb.ToString());
//            }

//            /// <summary>
//            /// Changes the trailers following the given trailer into a multi-trailer. Spawns extra trailers if required.
//            /// </summary>
//            /// <param name="firstTrailerID"></param>
//            /// <param name="multiTrailer"></param>
//            /// <returns></returns>
//            private static ushort ForceMultiTrailer(ushort firstTrailerID, TrailerDefinition.Trailer multiTrailer)
//            {
//                var availableVehicles = VehicleManager.instance.m_vehicles.m_buffer[firstTrailerID].GetTrailerCount(firstTrailerID) + 1;
//                if(availableVehicles < multiTrailer.SubTrailers.Count)
//                {
//                    SpawnTrailers(firstTrailerID, multiTrailer.SubTrailers.Count - availableVehicles);
//                }

//                ushort lastVehicleID = 0;

//                for(int subTrailerIndex = 0; subTrailerIndex < multiTrailer.SubTrailers.Count; subTrailerIndex++)
//                {
//                    ChangeTrailer(firstTrailerID, multiTrailer.SubTrailers[subTrailerIndex]);
                    
//                    lastVehicleID = firstTrailerID;
//                    firstTrailerID = VehicleManager.instance.m_vehicles.m_buffer[firstTrailerID].m_trailingVehicle;
//                    if(firstTrailerID == 0 && subTrailerIndex < multiTrailer.SubTrailers.Count - 1)
//                    {
//                        Util.LogError("Unexpected end of train when spawning multi-trailer, last vehicle was [" + lastVehicleID + "] at subIndex [" + subTrailerIndex + "] out of total amount " + multiTrailer.SubTrailers.Count);
//                        subTrailerIndex = 9999;
//                    }
//                }

//                return lastVehicleID;
//            }

//            /// <summary>
//            /// Changes the given vehicle to the info and settings specified by the trailer data.
//            /// </summary>
//            /// <param name="vehicleID"></param>
//            /// <param name="trailerDefinition"></param>
//            private static void ChangeTrailer(ushort vehicleID, TrailerDefinition.Trailer trailerDefinition)
//            {
//                VehicleManager.instance.m_vehicles.m_buffer[vehicleID].Info = trailerDefinition.GetInfo();
//                // TODO: Randomize this properly
//                if(trailerDefinition.InvertProbability > 50)
//                {
//                    VehicleManager.instance.m_vehicles.m_buffer[vehicleID].m_flags |= Vehicle.Flags.Inverted;
//                }
//                else
//                {
//                    VehicleManager.instance.m_vehicles.m_buffer[vehicleID].m_flags &= ~Vehicle.Flags.Inverted;
//                }
//            }

//            /// <summary>
//            /// Resets the vehicle's trailers into their spawn position, should avoid trains getting stuck after a consist change
//            /// </summary>
//            /// <param name="leadingVehicleID">The id of the leading vehicle (engine)</param>
//            private static void RepositionTrailers(ushort leadingVehicleID)
//            {
//                var instance = VehicleManager.instance;
//                var _this = instance.m_vehicles.m_buffer[leadingVehicleID];
//                var info = _this.Info;
//                var trailerCount = _this.GetTrailerCount(leadingVehicleID);

//                bool hasVerticalTrailers = info.m_vehicleAI.VerticalTrailers();
//                ushort prevId = leadingVehicleID;
//                Vehicle.Frame lastFrameData = _this.GetLastFrameData();
//                float zPos = (!hasVerticalTrailers) ? (info.m_generatedInfo.m_size.z * 0.5f) : 0f;
//                zPos -= (((_this.m_flags & Vehicle.Flags.Inverted) == (Vehicle.Flags)0) ? info.m_attachOffsetBack : info.m_attachOffsetFront);

//                for(int i = 0; i < trailerCount; i++)
//                {
//                    ushort trailerId = instance.m_vehicles.m_buffer[prevId].m_trailingVehicle;

//                    VehicleInfo trailerInfo = instance.m_vehicles.m_buffer[trailerId].Info;
//                    bool isInverted = (instance.m_vehicles.m_buffer[trailerId].m_flags & Vehicle.Flags.Inverted) > 0;

//                    zPos += ((!hasVerticalTrailers) ? (trailerInfo.m_generatedInfo.m_size.z * 0.5f) : trailerInfo.m_generatedInfo.m_size.y);
//                    zPos -= ((!isInverted) ? trailerInfo.m_attachOffsetFront : trailerInfo.m_attachOffsetBack);
//                    Vector3 position = lastFrameData.m_position - lastFrameData.m_rotation * new Vector3(0f, (!hasVerticalTrailers) ? 0f : zPos, (!hasVerticalTrailers) ? zPos : 0f);

//                    // Remove from grid BEFORE changing position, otherwise it will mess things up horribly
//                    instance.RemoveFromGrid(trailerId, ref instance.m_vehicles.m_buffer[trailerId], trailerInfo.m_isLargeVehicle);

//                    instance.m_vehicles.m_buffer[trailerId].m_frame0 = new Vehicle.Frame(position, Quaternion.identity);
//                    instance.m_vehicles.m_buffer[trailerId].m_frame1 = new Vehicle.Frame(position, Quaternion.identity);
//                    instance.m_vehicles.m_buffer[trailerId].m_frame2 = new Vehicle.Frame(position, Quaternion.identity);
//                    instance.m_vehicles.m_buffer[trailerId].m_frame3 = new Vehicle.Frame(position, Quaternion.identity);

//                    instance.m_vehicles.m_buffer[trailerId].m_path = 0u;
//                    instance.m_vehicles.m_buffer[trailerId].m_lastFrame = 0;
//                    instance.m_vehicles.m_buffer[trailerId].m_pathPositionIndex = 0;
//                    instance.m_vehicles.m_buffer[trailerId].m_lastPathOffset = 0;
//                    instance.m_vehicles.m_buffer[trailerId].m_gateIndex = 0;
//                    instance.m_vehicles.m_buffer[trailerId].m_waterSource = 0;

//                    instance.m_vehicles.m_buffer[trailerId].m_frame0.m_rotation = lastFrameData.m_rotation;
//                    instance.m_vehicles.m_buffer[trailerId].m_frame1.m_rotation = lastFrameData.m_rotation;
//                    instance.m_vehicles.m_buffer[trailerId].m_frame2.m_rotation = lastFrameData.m_rotation;
//                    instance.m_vehicles.m_buffer[trailerId].m_frame3.m_rotation = lastFrameData.m_rotation;

//                    instance.m_vehicles.m_buffer[trailerId].m_targetPos0 = Vector4.zero;
//                    instance.m_vehicles.m_buffer[trailerId].m_targetPos1 = Vector4.zero;
//                    instance.m_vehicles.m_buffer[trailerId].m_targetPos2 = Vector4.zero;
//                    instance.m_vehicles.m_buffer[trailerId].m_targetPos3 = Vector4.zero;
                    
//                    instance.AddToGrid(trailerId, ref instance.m_vehicles.m_buffer[trailerId], trailerInfo.m_isLargeVehicle);


//                    trailerInfo.m_vehicleAI.FrameDataUpdated(trailerId, ref instance.m_vehicles.m_buffer[trailerId], ref instance.m_vehicles.m_buffer[trailerId].m_frame0);

//                    prevId = trailerId;

//                    zPos += ((!hasVerticalTrailers) ? (trailerInfo.m_generatedInfo.m_size.z * 0.5f) : 0f);
//                    zPos -= ((!isInverted) ? trailerInfo.m_attachOffsetBack : trailerInfo.m_attachOffsetFront);
//                }
//            }
//        }
    }

}
