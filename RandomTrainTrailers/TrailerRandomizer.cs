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
                desiredTrailerCount = randomizer.Int32(config.TrailerCountOverride.Min, config.TrailerCountOverride.Max);   // The min/max variant of Int32 is probably inclusive from testing
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
                    var gateIdx = manager.m_vehicles.m_buffer[trailerId].m_gateIndex;
                    gateIndexes.Add(gateIdx);
                    //emptiesGateIndexCounts[Mathf.Clamp(gateIdx, 0, emptiesGateIndexCounts.Length)]++;
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


            // Determine cargo contents of train (if required by config)
            var debug_cargoContents = new List<String>();

            int[] cargoContents = new int[CargoParcel.ResourceTypes.Length];     // the index is the flag index, value is the amount
            int[] emptiesCount = new int[CargoParcel.ResourceTypes.Length];
            int assignedCargoWagons = 0;
            int availableCargoWagons = 0;
            if(config.UseCargoContents && info.m_vehicleAI is CargoTrainAI)
            {
                ushort cargoId = vehicle.m_firstCargo;
                while(cargoId != 0)
                {
                    // Turn the actual cargo type into our own cargo type. It's lossy but it works well enough.
                    var index = CargoParcel.LowestFlagToIndex(CargoParcel.TransferToFlags((TransferManager.TransferReason)manager.m_vehicles.m_buffer[cargoId].m_transferType));
                    if(index < 0)
                    {
                        Util.LogError("Invalid cargo index found for cargo vehicle id " + cargoId);
                        break;
                    }
                    if(index >= cargoContents.Length)
                    {
                        Util.LogError("Cargo Index " + index + " out of bounds for cargo vehicle "  + cargoId);
                    }
                    cargoContents[index]++;

                    debug_cargoContents.Add(string.Format("Cargo Type {0}, original was {1}", CargoParcel.FlagIndexToName(index), CargoParcel.TransferToName(manager.m_vehicles.m_buffer[cargoId].m_transferType)));

                    cargoId = manager.m_vehicles.m_buffer[cargoId].m_nextCargo;
                }

                // Turn cargoContents into the amount of wagons needed
                availableCargoWagons = desiredTrailerCount - config.EndOffset - config.StartOffset;
                if(availableCargoWagons > 0)
                {
                    for(int i = 0; i < cargoContents.Length; i++)
                    {
                        // TODO: Might needs some tweaking to get the values we want
                        cargoContents[i] = (cargoContents[i] * availableCargoWagons + ((CargoTrainAI)info.m_vehicleAI).m_cargoCapacity - 1) / ((CargoTrainAI)info.m_vehicleAI).m_cargoCapacity;
                        assignedCargoWagons += cargoContents[i];
                    }

                    // Assign what cargo types the empties should have
                    if(assignedCargoWagons > 0)
                    {
                        // Round-robin esque assignment of empties
                        var emptySlots = availableCargoWagons - assignedCargoWagons;
                        int k = 0;
                        while(emptySlots > 0)
                        {
                            while(cargoContents[k % cargoContents.Length] == 0) { k++; }
                            emptiesCount[k % emptiesCount.Length]++;
                            emptySlots--;
                            k++;
                        }
                    }
                    else
                    {
                        // All in generic goods, the above algorithm would run forever
                        emptiesCount[8] = availableCargoWagons;
                    }
                }
            }

            // Spawn new trailers
            var debug_spawnedTrailers = new List<string>();
            
            ushort prevVehicleId = id;
            int cargoTypeIndex = 0;
            for(int i = 0; i < desiredTrailerCount; i++)
            {
                ushort newTrailerId;

                // Check if we can randomize or if we should use the default m_trailers
                if(i >= config.StartOffset && i < desiredTrailerCount - config.EndOffset)
                {
                    if(config.UseCargoContents)
                    {
                        // Randomize based on cargo
                        // If we have cargo of the current type, spawn a wagon for it
                        while(cargoContents[cargoTypeIndex] <= 0 && emptiesCount[cargoTypeIndex] <= 0 && cargoTypeIndex < cargoContents.Length - 1) { cargoTypeIndex++; }

                        if(cargoContents[cargoTypeIndex] > 0)
                        {
                            // Spawn some cargo wagons
                            var spawnedTrailerCount = SpawnCargoTrailer(out newTrailerId, prevVehicleId,
                                trailerCollection,
                                randomizer,
                                false,   
                                cargoTypeIndex,
                                out int assignedCargoIndex);
                            if(spawnedTrailerCount > 0)
                            {
                                i += spawnedTrailerCount - 1;
                                cargoContents[cargoTypeIndex] -= spawnedTrailerCount;
                                assignedCargoWagons += spawnedTrailerCount - 1;
                                prevVehicleId = newTrailerId;
                                emptiesCount[cargoTypeIndex] -= spawnedTrailerCount - 1;        // Remove additionally spawned multi-trailers from our empties reserve

                                debug_spawnedTrailers.Add(string.Format("Spawned Filled Cargo Type: {0}, was supposed to be {1}",
                                    CargoParcel.FlagIndexToName(assignedCargoIndex),
                                    CargoParcel.FlagIndexToName(cargoTypeIndex)));
                            }
                        }
                        else if(emptiesCount[cargoTypeIndex] > 0)
                        {
                            // Spawn 'empty' wagons
                            var spawnedTrailerCount = SpawnCargoTrailer(out newTrailerId, prevVehicleId,
                                trailerCollection,
                                randomizer,
                                true,
                                cargoTypeIndex,
                                out int assignedCargoIndex);
                            if(spawnedTrailerCount > 0)
                            {
                                i += spawnedTrailerCount - 1;
                                prevVehicleId = newTrailerId;

                                emptiesCount[cargoTypeIndex] -= spawnedTrailerCount;

                                debug_spawnedTrailers.Add(string.Format("Spawned Empty Cargo Type: {0}, was supposed to be {1}",
                                    CargoParcel.FlagIndexToName(assignedCargoIndex),
                                    CargoParcel.FlagIndexToName(cargoTypeIndex)));
                            }
                        }
                    }
                    else
                    {
                        // Randomize
                        var spawnedTrailerCount = SpawnRandomTrailer(out newTrailerId, prevVehicleId, trailerCollection, randomizer, gateIndexes[Mathf.Clamp(i, 0, gateIndexes.Count - 1)]);
                        if(spawnedTrailerCount > 0)
                        {
                            i += spawnedTrailerCount - 1;
                            prevVehicleId = newTrailerId;

                            debug_spawnedTrailers.Add("Spawned Random");
                        }
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

                        debug_spawnedTrailers.Add("Spawned Default");
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

                        debug_spawnedTrailers.Add("Spawned Default");
                    }
                }
            }

            // And we're done
            if(debug_cargoContents.Count > 0)
            {
                Util.Log(string.Format("Cargo for {0} [{1}]\r\n", info.name, id) + debug_cargoContents.Aggregate((sequence, next) => sequence + "\r\n" + next));
            }
            if(debug_spawnedTrailers.Count > 0)
            {
                Util.Log(string.Format("Collection {2} was used for {0} [{1}]\r\n", info.name, id, trailerCollection.Name));

                Util.Log(string.Format("Spawned trailers for {0} [{1}]\r\n", info.name, id) +
                    debug_spawnedTrailers.Aggregate((sequence, next) => sequence + "\r\n" + next) + 
                    string.Format("\r\n{0} filled trailers were assigned out of {1} available", assignedCargoWagons, availableCargoWagons));
            }
        }

        /// <summary>
        /// Spawns a randomly selected (multi)trailer for a certain cargo type.
        /// </summary>
        /// <param name="lastTrailerId">Id of the last spawned trailer</param>
        /// <param name="prevVehicleId">The id of the vehicle to spawn behind</param>
        /// <param name="trailerCollection">The collection to use, should have m_cargoData set</param>
        /// <param name="randomizer">Randomizer to use</param>
        /// <param name="gateIndex">Gate index to assign to the spawned trailer(s)</param>
        /// <param name="cargoFlagIndex">Cargo index to use</param>
        /// <returns>The amount of spawned trailers</returns>
        private static int SpawnCargoTrailer(out ushort lastTrailerId, ushort prevVehicleId, TrailerDefinition.TrailerCollection trailerCollection, Randomizer randomizer, bool empty, int cargoFlagIndex, out int assignedCargoIndex)
        {
            if(trailerCollection.m_cargoData == null)
            {
                Util.LogError("Supposed to spawn cargo trailer but collection " + trailerCollection.Name + " does not have cargo data set!");
                lastTrailerId = 0;
                assignedCargoIndex = 0;
                return 0;
            }


            TrailerDefinition.Trailer trailer = GetTrailerForCargo(out assignedCargoIndex, trailerCollection, cargoFlagIndex);

            if(trailer == null)
            {
                Util.LogError("Unable to find trailer for cargo type " + cargoFlagIndex + " or its fallbacks in collection " + trailerCollection.Name);
                lastTrailerId = 0;
                return 0;
            }

            byte gateIndex = CargoParcel.FlagIndexToGateIndex(assignedCargoIndex);

            return SpawnTrailerDefinition(out lastTrailerId, prevVehicleId, trailer, randomizer, empty ? CargoParcel.GetEmptyGateIndex(gateIndex) : gateIndex);
        }

        private static TrailerDefinition.Trailer GetTrailerForCargo(out int assignedCargoIndex, TrailerDefinition.TrailerCollection collection, int cargoIndex)
        {
            for(int i = 0; i < CargoParcel.ResourceFallback[cargoIndex].Length; i++)
            {
                int attemptedCargoType = CargoParcel.LowestFlagToIndex(CargoParcel.ResourceFallback[cargoIndex][i]);
                int trailerIndex = collection.m_cargoData.GetRandomTrailerIndex(attemptedCargoType);
                if(trailerIndex >= 0)
                {
                    assignedCargoIndex = attemptedCargoType;
                    return collection.m_cargoData.m_trailers[attemptedCargoType][trailerIndex];
                }
            }

            assignedCargoIndex = 0;
            return null;
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
            var trailer = trailerCollection.GetRandomTrailer();

            return SpawnTrailerDefinition(out lastTrailerId, prevVehicleId, trailer, randomizer, gateIndex);
        }

        /// <summary>
        /// Spawns a trailer from the given trailer config
        /// </summary>
        /// <param name="lastTrailerId"></param>
        /// <param name="prevVehicleId"></param>
        /// <param name="trailer"></param>
        /// <param name="randomizer"></param>
        /// <param name="gateIndex"></param>
        /// <returns></returns>
        private static int SpawnTrailerDefinition(out ushort lastTrailerId, ushort prevVehicleId, TrailerDefinition.Trailer trailer, Randomizer randomizer, byte gateIndex)
        {
            lastTrailerId = 0;

            // Spawn the trailer
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
