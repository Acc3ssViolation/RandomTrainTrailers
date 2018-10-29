using ColossalFramework;
using ColossalFramework.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RandomTrainTrailers
{
    /// <summary>
    /// Contains logic for randomizing a Vehicle's trailers
    /// </summary>
    static class TrailerRandomizer
    {
        /// <summary>
        /// Randomizes a vehicle's trailers based on the given config
        /// </summary>
        /// <param name="vehicle">Reference to the vehicle's struct</param>
        /// <param name="id">ID of the vehicle</param>
        /// <param name="config">The randomization config to use</param>
        public static void RandomizeTrailers(ref Vehicle vehicle, ushort id, TrailerDefinition.Vehicle config, Randomizer randomizer)
        {
            if(config == null || vehicle.Info.m_trailers == null || vehicle.Info.m_trailers.Length == 0) { return; }

            if(vehicle.Info.m_vehicleAI.VerticalTrailers())
            {
                Util.LogWarning("Trying to randomize trailers for vehicle with vertical trailers, not a supported usecase, vehicle may appear strangely");
            }

            var info = vehicle.Info;

            // Get trailer count we want
            var desiredTrailerCount = info.m_trailers.Length;
            if(config.TrailerCountOverride != null && config.TrailerCountOverride.IsValid)
            {
                desiredTrailerCount = randomizer.Int32(config.TrailerCountOverride.Min, config.TrailerCountOverride.Max + 1);
            }
            else if(info.m_maxTrailerCount > 0 && desiredTrailerCount > info.m_maxTrailerCount)
            {
                desiredTrailerCount = info.m_maxTrailerCount;
            }
            if(TrailerManager.globalTrailerLimit > 0 && desiredTrailerCount > TrailerManager.globalTrailerLimit)
            {
                desiredTrailerCount = TrailerManager.globalTrailerLimit;
            }

            // Select which collection to use
            var trailerCollection = config.GetRandomCollection();

            // Store the gate indexes (cargo type/variation per wagon)
            var gateIndexes = new List<byte>();
            var manager = Singleton<VehicleManager>.instance;
            {
                ushort trailerId = vehicle.m_trailingVehicle;
                while(trailerId != 0)
                {
                    gateIndexes.Add(manager.m_vehicles.m_buffer[trailerId].m_gateIndex);
                    trailerId = manager.m_vehicles.m_buffer[trailerId].m_trailingVehicle;
                }

                // Unhook first trailer from lead and remove it. This will remove all trailers.
                trailerId = vehicle.m_trailingVehicle;
                manager.m_vehicles.m_buffer[trailerId].m_leadingVehicle = 0;
                manager.m_vehicles.m_buffer[id].m_trailingVehicle = 0;
                manager.ReleaseVehicle(trailerId);
            }
            // Ensure we have a value in case this vehicle doesn't have trailers
            if(gateIndexes.Count == 0)
            {
                gateIndexes.Add(vehicle.m_gateIndex);
            }

            // Spawn new trailers
            ushort prevVehicleId = id;
            for(int i = 0; i < desiredTrailerCount; i++)
            {
                ushort newTrailerId;

                // Check if we can randomize or if we should use the default m_trailers
                if(i >= config.StartOffset && i < desiredTrailerCount - config.EndOffset)
                {
                    // Randomize
                    var spawnedTrailerCount = SpawnRandomTrailer(out newTrailerId, prevVehicleId, trailerCollection, randomizer, gateIndexes[Mathf.Clamp(i, 0, gateIndexes.Count - 1)]);
                    if(spawnedTrailerCount > 0)
                    {
                        i += spawnedTrailerCount - 1;
                        prevVehicleId = newTrailerId;
                    }
                }
                else if(i < config.StartOffset)
                {
                    // Use m_trailers due to Start offset
                    // Clamp index just in case someone tries to do anything funny with their config
                    int trailersIdx = Mathf.Clamp(i, 0, info.m_trailers.Length - 1);
                    ushort trailerId;
                    if(SpawnTrailer(out trailerId,
                        ref Singleton<VehicleManager>.instance.m_vehicles.m_buffer[prevVehicleId], 
                        prevVehicleId,
                        info.m_trailers[trailersIdx].m_info, 
                        randomizer.Int32(100u) < info.m_trailers[trailersIdx].m_invertProbability,
                        gateIndexes[Mathf.Clamp(i, 0, gateIndexes.Count - 1)]))
                    {
                        prevVehicleId = trailerId;
                    }
                }
                else
                {
                    // Use m_trailers due to Back offset
                    // Math should check out but just making sure
                    int trailersIdx = Mathf.Clamp(i - (desiredTrailerCount - info.m_trailers.Length), 0, info.m_trailers.Length);
                    ushort trailerId;
                    if(SpawnTrailer(out trailerId,
                        ref Singleton<VehicleManager>.instance.m_vehicles.m_buffer[prevVehicleId],
                        prevVehicleId,
                        info.m_trailers[trailersIdx].m_info,
                        randomizer.Int32(100u) < info.m_trailers[trailersIdx].m_invertProbability,
                        gateIndexes[Mathf.Clamp(i, 0, gateIndexes.Count - 1)]))
                    {
                        prevVehicleId = trailerId;
                    }
                }
            }

            // And we're done. Much better than that previous spaghetti.
        }

        /// <summary>
        /// Spawns a random trailer from the collection after the given previous vehicle
        /// </summary>
        /// <param name="lastTrailerId">The id of the last trailer that this method spawned on invocation</param>
        /// <param name="prevVehicleId">The id of the vehicle to put the new trailers behind</param>
        /// <param name="trailerCollection">The collection of trailers</param>
        /// <returns></returns>
        private static int SpawnRandomTrailer(out ushort lastTrailerId, ushort prevVehicleId, TrailerDefinition.TrailerCollection trailerCollection, Randomizer randomizer, byte gateIndex)
        {
            lastTrailerId = 0;

            var trailer = trailerCollection.GetRandomTrailer();
            if(trailer.IsMultiTrailer())
            {
                // Spawn all subtrailers
                var infos = trailer.GetInfos();
                for(int i = 0; i < infos.Count; i++)
                {
                    ushort trailerId;
                    if(SpawnTrailer(out trailerId, ref Singleton<VehicleManager>.instance.m_vehicles.m_buffer[prevVehicleId], prevVehicleId, infos[i], randomizer.Int32(100u) < trailer.InvertProbability, gateIndex))
                    {
                        prevVehicleId = trailerId;
                        lastTrailerId = trailerId;
                    }
                    else
                    {
                        // More or less an error condition, just return the amount we have already spawned
                        return i;
                    }

                }
                return infos.Count;
            }

            // Spawn single trailer
            var info = trailer.GetInfo();
            if(SpawnTrailer(out lastTrailerId, ref Singleton<VehicleManager>.instance.m_vehicles.m_buffer[prevVehicleId], prevVehicleId, info, randomizer.Int32(100u) < trailer.InvertProbability, gateIndex))
            {
                return 1;
            }
            return 0;
        }

        /// <summary>
        /// Spawns a new trailer behind a given vehicle
        /// </summary>
        /// <param name="prevVehicle">The vehicle to spawn behind</param>
        /// <param name="prevVehicleId">The id of the vehicle</param>
        /// <param name="trailerInfo">The info of the new trailer</param>
        /// <param name="inverted">If the new trailer should be inverted</param>
        /// <returns>Id of new trailer, 0 on fail</returns>
        private static bool SpawnTrailer(out ushort trailerId, ref Vehicle prevVehicle, ushort prevVehicleId, VehicleInfo trailerInfo, bool inverted, byte gateIndex)
        {
            trailerId = prevVehicle.CreateTrailer(prevVehicleId, trailerInfo, inverted);
            if(trailerId == 0) { return false; }

            Singleton<VehicleManager>.instance.m_vehicles.m_buffer[trailerId].m_gateIndex = gateIndex;
            Singleton<VehicleManager>.instance.m_vehicles.m_buffer[trailerId].Spawn(trailerId);
            return true;
        }
    }
}
