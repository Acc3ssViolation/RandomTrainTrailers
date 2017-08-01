using System.Collections.Generic;
using UnityEngine;
using ColossalFramework;
using System.Reflection;
using ColossalFramework.Math;
using System;
using Harmony;

namespace RandomTrainTrailers.Detour
{
    ///// <summary>
    ///// Handles detouring of Vehicle methods
    ///// </summary>
    //public class VehicleDetour
    //{
    //    public void Deploy()
    //    {
    //        var harmony = HarmonyInstance.Create(Mod.harmonyPackage);
    //        Version currentVersion;
    //        if(harmony.VersionInfo(out currentVersion).ContainsKey(Mod.harmonyPackage))
    //        {
    //            Util.LogWarning("Harmony patches already present");
    //            return;
    //        }
    //        Util.Log("Harmony v" + currentVersion);

            
    //        var fuckmeonce = typeof(Vehicle).GetMethod("Spawn");
    //        var fuckmetwice = typeof(VehicleDetour).GetMethod("Spawn", BindingFlags.Static | BindingFlags.Public);
    //        var vehicleSpawnSource = typeof(Vehicle).GetMethod("Spawn");
    //        var vehicleSpawnPrefix = typeof(VehiclePatch_Spawn).GetMethod("Prefix", BindingFlags.Static | BindingFlags.Public);

    //        Util.Log("Patching Vehicle.Spawn...");
    //        //harmony.Patch(vehicleSpawnSource, new HarmonyMethod(vehicleSpawnPrefix), null);
    //        harmony.Patch(fuckmeonce, new HarmonyMethod(fuckmetwice), null);
    //        //harmony.PatchAll(Assembly.GetExecutingAssembly());
    //        Util.Log("Harmony patches applied");
    //    }

    //    public void Revert()
    //    {
    //        //Dunno?
    //    }

    //    public static bool Spawn(ushort vehicleID)
    //    {
    //        Util.Log("FUCK ME HARD NR " + vehicleID);
    //        return true;
    //    }

    //    public static bool CalculateTotalLength(ushort vehicleID)
    //    {
    //        Util.Log("DETOOURRRA: " + vehicleID);
    //        return true;
    //    }

    //    public static bool CanLeave(ushort vehicleID, ref Vehicle vehicleData)
    //    {
    //        Util.Log("DETOUUUUR: " + vehicleID + " ?? " + vehicleData.Info.name);
    //        return true;
    //    }

    //    /// <summary>
    //    /// Redirect for Vehicle.Spawn
    //    /// </summary>
    //    /// <param name="vehicleID"></param>
    //    /*public static void SpawnRedirect(ref Vehicle vehicle, ushort vehicleID)
    //    {
    //        Vehicle vehicle = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleID];
    //        Spawn(ref vehicle, vehicleID);
    //        ushort trailingVehicle = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleID].m_trailingVehicle;
    //        vehicle.m_trailingVehicle = trailingVehicle;
    //        Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleID] = vehicle;
    //    }*/

    //}

    //[HarmonyPatch(typeof(Vehicle), "Spawn")]
    public static class VehiclePatch_Spawn
    {
        public static void Spawn_Imp(ref Vehicle __instance, ushort vehicleID)
        {
            VehicleInfo info = __instance.Info;

            //Util.Log("Spawning vehicle " + info.name);

            if((__instance.m_flags & Vehicle.Flags.Spawned) == (Vehicle.Flags)0)
            {
                __instance.m_flags |= Vehicle.Flags.Spawned;
                Singleton<VehicleManager>.instance.AddToGrid(vehicleID, ref __instance, info.m_isLargeVehicle);
            }
            if(__instance.m_leadingVehicle == 0 && __instance.m_trailingVehicle == 0 && info.m_trailers != null)
            {
                bool hasVerticalTrailers = info.m_vehicleAI.VerticalTrailers();
                ushort prevId = vehicleID;
                bool isReversed = (Singleton<VehicleManager>.instance.m_vehicles.m_buffer[(int)prevId].m_flags & Vehicle.Flags.Reversed) != (Vehicle.Flags)0;
                Vehicle.Frame lastFrameData = __instance.GetLastFrameData();
                float zPos = (!hasVerticalTrailers) ? (info.m_generatedInfo.m_size.z * 0.5f) : 0f;
                zPos -= (((__instance.m_flags & Vehicle.Flags.Inverted) == (Vehicle.Flags)0) ? info.m_attachOffsetBack : info.m_attachOffsetFront);
                Randomizer randomizer = new Randomizer((int)vehicleID);

                // Mod begin
                int trailerCount = info.m_trailers.Length;
                TrailerDefinition.TrailerCollection trailerCollection = null;
                var vehicleDef = TrailerManager.GetVehicleConfig(info.name);
                var random = new System.Random();
                int[] trailerCDF = null;
                if(vehicleDef != null)
                {
                    //Util.Log("Vehicle " + info.name + " has definition");
                    if(randomizer.Int32(100u) < vehicleDef.RandomTrailerChance)
                    {
                        //Util.Log("Vehicle " + info.name + " will be randomized");
                        // Use random trailers
                        // Select a collection
                        if(vehicleDef.m_trailerCollections.Count > 1)
                        {
                            int[] colCDF = new int[vehicleDef.m_trailerCollections.Count];
                            for(int i = 0; i < vehicleDef.m_trailerCollections.Count; i++)
                            {
                                colCDF[i] = vehicleDef.m_trailerCollections[i].m_weight + (i > 0 ? colCDF[i - 1] : 0);
                            }
                            int colIndex = Array.BinarySearch(colCDF, random.Next(colCDF[colCDF.Length - 1] + 1));
                            if(colIndex < 0)
                            {
                                colIndex = ~colIndex;
                            }
                            if(colIndex < 0 || colIndex > vehicleDef.m_trailerCollections.Count - 1)
                            {
                                Util.LogError("Index out of bounds! " + colIndex);
                            }
                            trailerCollection = vehicleDef.m_trailerCollections[colIndex].m_trailerCollection;
                        }
                        else
                        {
                            trailerCollection = vehicleDef.m_trailerCollections[0].m_trailerCollection;
                        }

                        // Compile CDF array for weighted random selection
                        trailerCDF = new int[trailerCollection.Trailers.Count];
                        for(int i = 0; i < trailerCollection.Trailers.Count; i++)
                        {
                            trailerCDF[i] = trailerCollection.Trailers[i].Weight + (i > 0 ? trailerCDF[i - 1] : 0);
                        }

                        // Randomize trailer count
                        if(vehicleDef.TrailerCountOverride != null && vehicleDef.TrailerCountOverride.IsValid)
                        {
                            trailerCount = random.Next(vehicleDef.TrailerCountOverride.Min, vehicleDef.TrailerCountOverride.Max + 1);
                        }
                    }
                    else
                    {
                        // Use default trailers
                        vehicleDef = null;
                        //Util.Log("Vehicle " + info.name + " will be DEFAULT");
                    }
                }

                // Apply global trailer limit
                int globalMaxTrailerCount = TrailerManager.GetTrailerCountOverride();
                if(globalMaxTrailerCount > 0)
                {
                    trailerCount = Math.Min(trailerCount, globalMaxTrailerCount);
                }
                // Mod end

                for(int i = 0; i < trailerCount; i++)
                {
                    if(randomizer.Int32(100u) < info.m_trailers[i % info.m_trailers.Length].m_probability)
                    {
                        VehicleInfo trailerInfo;
                        bool isInverted;

                        // Mod start
                        if(vehicleDef != null &&
                            i >= vehicleDef.StartOffset &&
                            i < trailerCount - vehicleDef.EndOffset)
                        {
                            // We may randomize this trailer

                            // Select random trailer index using the cdf array
                            int randomTrailerIndex = Array.BinarySearch(trailerCDF, random.Next(trailerCDF[trailerCDF.Length - 1] + 1));
                            if(randomTrailerIndex < 0)
                            {
                                randomTrailerIndex = ~randomTrailerIndex;
                            }
                            if(randomTrailerIndex < 0 || randomTrailerIndex > trailerCollection.Trailers.Count - 1)
                            {
                                Util.LogError("Index out of bounds! " + randomTrailerIndex);
                            }

                            if(trailerCollection.Trailers[randomTrailerIndex].IsMultiTrailer())
                            {
                                // Spawn all multi trailer sub trailers
                                for(int subTrailerIndex = 0; subTrailerIndex < trailerCollection.Trailers[randomTrailerIndex].SubTrailers.Count; subTrailerIndex++)
                                {
                                    trailerInfo = trailerCollection.Trailers[randomTrailerIndex].SubTrailers[subTrailerIndex].GetInfo();
                                    isInverted = randomizer.Int32(100u) < trailerCollection.Trailers[randomTrailerIndex].SubTrailers[subTrailerIndex].InvertProbability;

                                    // Copy of default spawn code section below
                                    zPos += ((!hasVerticalTrailers) ? (trailerInfo.m_generatedInfo.m_size.z * 0.5f) : trailerInfo.m_generatedInfo.m_size.y);
                                    zPos -= ((!isInverted) ? trailerInfo.m_attachOffsetFront : trailerInfo.m_attachOffsetBack);
                                    Vector3 position2 = lastFrameData.m_position - lastFrameData.m_rotation * new Vector3(0f, (!hasVerticalTrailers) ? 0f : zPos, (!hasVerticalTrailers) ? zPos : 0f);
                                    ushort trailerId2;
                                    if(Singleton<VehicleManager>.instance.CreateVehicle(out trailerId2, ref Singleton<SimulationManager>.instance.m_randomizer, trailerInfo, position2, (TransferManager.TransferReason)__instance.m_transferType, false, false))
                                    {
                                        Singleton<VehicleManager>.instance.m_vehicles.m_buffer[(int)prevId].m_trailingVehicle = trailerId2;
                                        Singleton<VehicleManager>.instance.m_vehicles.m_buffer[(int)trailerId2].m_leadingVehicle = prevId;
                                        if(isInverted)
                                        {
                                            Vehicle[] expr_24A_cp_0 = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;
                                            ushort expr_24A_cp_1 = trailerId2;
                                            expr_24A_cp_0[(int)expr_24A_cp_1].m_flags = (expr_24A_cp_0[(int)expr_24A_cp_1].m_flags | Vehicle.Flags.Inverted);
                                        }
                                        if(isReversed)
                                        {
                                            Vehicle[] expr_270_cp_0 = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;
                                            ushort expr_270_cp_1 = trailerId2;
                                            expr_270_cp_0[(int)expr_270_cp_1].m_flags = (expr_270_cp_0[(int)expr_270_cp_1].m_flags | Vehicle.Flags.Reversed);
                                        }
                                        Singleton<VehicleManager>.instance.m_vehicles.m_buffer[(int)trailerId2].m_frame0.m_rotation = lastFrameData.m_rotation;
                                        Singleton<VehicleManager>.instance.m_vehicles.m_buffer[(int)trailerId2].m_frame1.m_rotation = lastFrameData.m_rotation;
                                        Singleton<VehicleManager>.instance.m_vehicles.m_buffer[(int)trailerId2].m_frame2.m_rotation = lastFrameData.m_rotation;
                                        Singleton<VehicleManager>.instance.m_vehicles.m_buffer[(int)trailerId2].m_frame3.m_rotation = lastFrameData.m_rotation;
                                        trailerInfo.m_vehicleAI.FrameDataUpdated(trailerId2, ref Singleton<VehicleManager>.instance.m_vehicles.m_buffer[(int)trailerId2], ref Singleton<VehicleManager>.instance.m_vehicles.m_buffer[(int)trailerId2].m_frame0);
                                        Singleton<VehicleManager>.instance.m_vehicles.m_buffer[(int)trailerId2].Spawn(trailerId2);
                                        prevId = trailerId2;
                                    }
                                    zPos += ((!hasVerticalTrailers) ? (trailerInfo.m_generatedInfo.m_size.z * 0.5f) : 0f);
                                    zPos -= ((!isInverted) ? trailerInfo.m_attachOffsetBack : trailerInfo.m_attachOffsetFront);

                                    // The first sub trailer is accounted for by the normal loop,
                                    // but for others we must increment the loop counter to prevent each multi trailer counting as 1 vehicle
                                    // (which would lead to trains that are too long)
                                    if(subTrailerIndex > 0)
                                        i++;
                                }

                                continue;   //for(int i = 0; i < info.m_trailers.Length; i++)
                            }
                            else
                            {
                                // Just select the trailer
                                trailerInfo = trailerCollection.Trailers[randomTrailerIndex].GetInfo();
                                isInverted = randomizer.Int32(100u) < trailerCollection.Trailers[randomTrailerIndex].InvertProbability;
                            }
                        }
                        else
                        {
                            // Get default trailer
                            trailerInfo = info.m_trailers[i].m_info;
                            isInverted = randomizer.Int32(100u) < info.m_trailers[i].m_invertProbability;
                        }
                        // Mod end

                        // Default spawn code
                        zPos += ((!hasVerticalTrailers) ? (trailerInfo.m_generatedInfo.m_size.z * 0.5f) : trailerInfo.m_generatedInfo.m_size.y);
                        zPos -= ((!isInverted) ? trailerInfo.m_attachOffsetFront : trailerInfo.m_attachOffsetBack);
                        Vector3 position = lastFrameData.m_position - lastFrameData.m_rotation * new Vector3(0f, (!hasVerticalTrailers) ? 0f : zPos, (!hasVerticalTrailers) ? zPos : 0f);
                        ushort trailerId;
                        if(Singleton<VehicleManager>.instance.CreateVehicle(out trailerId, ref Singleton<SimulationManager>.instance.m_randomizer, trailerInfo, position, (TransferManager.TransferReason)__instance.m_transferType, false, false))
                        {
                            Singleton<VehicleManager>.instance.m_vehicles.m_buffer[(int)prevId].m_trailingVehicle = trailerId;
                            Singleton<VehicleManager>.instance.m_vehicles.m_buffer[(int)trailerId].m_leadingVehicle = prevId;
                            if(isInverted)
                            {
                                Vehicle[] expr_24A_cp_0 = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;
                                ushort expr_24A_cp_1 = trailerId;
                                expr_24A_cp_0[(int)expr_24A_cp_1].m_flags = (expr_24A_cp_0[(int)expr_24A_cp_1].m_flags | Vehicle.Flags.Inverted);
                            }
                            if(isReversed)
                            {
                                Vehicle[] expr_270_cp_0 = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;
                                ushort expr_270_cp_1 = trailerId;
                                expr_270_cp_0[(int)expr_270_cp_1].m_flags = (expr_270_cp_0[(int)expr_270_cp_1].m_flags | Vehicle.Flags.Reversed);
                            }
                            Singleton<VehicleManager>.instance.m_vehicles.m_buffer[(int)trailerId].m_frame0.m_rotation = lastFrameData.m_rotation;
                            Singleton<VehicleManager>.instance.m_vehicles.m_buffer[(int)trailerId].m_frame1.m_rotation = lastFrameData.m_rotation;
                            Singleton<VehicleManager>.instance.m_vehicles.m_buffer[(int)trailerId].m_frame2.m_rotation = lastFrameData.m_rotation;
                            Singleton<VehicleManager>.instance.m_vehicles.m_buffer[(int)trailerId].m_frame3.m_rotation = lastFrameData.m_rotation;
                            trailerInfo.m_vehicleAI.FrameDataUpdated(trailerId, ref Singleton<VehicleManager>.instance.m_vehicles.m_buffer[(int)trailerId], ref Singleton<VehicleManager>.instance.m_vehicles.m_buffer[(int)trailerId].m_frame0);
                            Singleton<VehicleManager>.instance.m_vehicles.m_buffer[(int)trailerId].Spawn(trailerId);
                            prevId = trailerId;
                        }
                        zPos += ((!hasVerticalTrailers) ? (trailerInfo.m_generatedInfo.m_size.z * 0.5f) : 0f);
                        zPos -= ((!isInverted) ? trailerInfo.m_attachOffsetBack : trailerInfo.m_attachOffsetFront);
                    }
                }
            }
        }

        public static bool Prefix(ref Vehicle __instance, ushort vehicleID)
        {
            Spawn_Imp(ref __instance, vehicleID);
            return true;
        }
    }
}
