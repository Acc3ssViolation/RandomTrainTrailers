using ColossalFramework;
using ColossalFramework.Math;
using RandomTrainTrailers.Definition;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RandomTrainTrailers
{
    /// <summary>
    /// Contains logic for randomizing a Vehicle's trailers
    /// </summary>
    static class TrailerRandomizer
    {
        private static DeferredLogger _logger;

        private static void BeginLogger()
        {
            if (_logger == null)
                _logger = new DeferredLogger();
            else
                _logger.Clear();
        }

        /// <summary>
        /// Randomizes a vehicle's trailers based on the given config
        /// </summary>
        /// <param name="vehicle">Reference to the vehicle's struct</param>
        /// <param name="id">ID of the vehicle</param>
        /// <param name="config">The randomization config to use</param>
        public static void RandomizeTrailers(ref Vehicle vehicle, ushort id, Definition.Vehicle config, Randomizer randomizer)
        {
            if (config == null || vehicle.Info.m_trailers == null || vehicle.Info.m_trailers.Length == 0) { return; }

            BeginLogger();

            if (vehicle.Info.m_vehicleAI.VerticalTrailers())
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
            if(TrailerManager.GlobalTrailerLimit > 0 && desiredTrailerCount > TrailerManager.GlobalTrailerLimit)
            {
                desiredTrailerCount = TrailerManager.GlobalTrailerLimit;
            }

            // Store the gate indexes (cargo type/variation per wagon)
            var gateIndexes = GetGateIndexes(ref vehicle);
            // Ensure we have a value in case this vehicle doesn't have trailers
            if(gateIndexes.Count == 0)
            {
                gateIndexes.Add(vehicle.m_gateIndex);
            }

            // Remove existing trailers
            RemoveTrailers(ref vehicle, id);

            // Determine cargo contents of train (if required by config)
            var debug_cargoContents = new List<String>();

            int[] cargoContents = new int[CargoParcel.ResourceTypes.Length];     // the index is the flag index, value is the amount
            int[] emptiesCount = new int[CargoParcel.ResourceTypes.Length];
            int assignedCargoWagons = 0;
            int availableCargoWagons = 0;
            var manager = Singleton<VehicleManager>.instance;
            if (config.UseCargoContents && info.m_vehicleAI is CargoTrainAI cargoTrainAi)
            {
                cargoContents = GetCargoContents(ref vehicle);
                availableCargoWagons = desiredTrailerCount - config.EndOffset - config.StartOffset;
                var trailerCounts = CalculateTrailerCount(cargoContents, availableCargoWagons, cargoTrainAi.m_cargoCapacity);

                // TODO: Clean this up
                cargoContents = trailerCounts.LoadedTrailers;
                emptiesCount = trailerCounts.EmptyTrailers;
                assignedCargoWagons = trailerCounts.LoadedTrailers.Sum();
            }

            // Spawn new trailers
            // Select which collection to use
            var trailerCollection = config.GetRandomCollection();

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

                                _logger.Add(string.Format("Spawned Filled Cargo Type: {0}, was supposed to be {1}",
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

                                _logger.Add(string.Format("Spawned Empty Cargo Type: {0}, was supposed to be {1}",
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

                            _logger.Add("Spawned Random");
                        }
                    }

                }
                else if(i < config.StartOffset)
                {
                    // Use m_trailers due to Start offset
                    // Clamp index just in case someone tries to do anything funny with their config
                    int trailersIdx = Mathf.Clamp(i, 0, info.m_trailers.Length - 1);
                    if (SpawnTrailer(out ushort trailerId,
                        prevVehicleId,
                        info.m_trailers[trailersIdx].m_info,
                        randomizer.Int32(100u) < info.m_trailers[trailersIdx].m_invertProbability,
                        gateIndexes[Mathf.Clamp(i, 0, gateIndexes.Count - 1)]))
                    {
                        prevVehicleId = trailerId;

                        _logger.Add("Spawned Default");
                    }
                }
                else
                {
                    // Use m_trailers due to Back offset
                    // Math should check out but just making sure
                    int trailersIdx = Mathf.Clamp(i - (desiredTrailerCount - info.m_trailers.Length), 0, info.m_trailers.Length);
                    if (SpawnTrailer(out ushort trailerId,
                        prevVehicleId,
                        info.m_trailers[trailersIdx].m_info,
                        randomizer.Int32(100u) < info.m_trailers[trailersIdx].m_invertProbability,
                        gateIndexes[Mathf.Clamp(i, 0, gateIndexes.Count - 1)]))
                    {
                        prevVehicleId = trailerId;

                        _logger.Add("Spawned Default");
                    }
                }
            }

            // Unset MiddleTrailer on last trailer
            ClearFlagsFromVehicle(prevVehicleId, 0, Vehicle.Flags2.MiddleTrailer);

            // And we're done
            if (debug_cargoContents.Count > 0)
            {
                Util.Log(string.Format("Cargo for {0} [{1}]\r\n", info.name, id) + debug_cargoContents.Aggregate((sequence, next) => sequence + "\r\n" + next));
            }
            if(_logger.Length > 0)
            {
                Util.Log(string.Format("Collection {2} was used for {0} [{1}]\r\n", info.name, id, trailerCollection.Name));

                Util.Log(string.Format("Spawned trailers for {0} [{1}]: {0} filled trailers were assigned out of {1} available", info.name, id, assignedCargoWagons, availableCargoWagons));
                _logger.Log();
            }
        }

        public static void GenerateTrain(ref Vehicle vehicle, ushort id, TrainPool pool, Locomotive leadLocomotive, Randomizer randomizer)
        {
            BeginLogger();

            var trainLength = randomizer.Int32(pool.MinTrainLength, pool.MaxTrainLength);
            if (TrailerManager.GlobalTrailerLimit > 0 && trainLength > TrailerManager.GlobalTrailerLimit.value)
                trainLength = TrailerManager.GlobalTrailerLimit.value;
            var locomotiveCount = randomizer.Int32(pool.MinLocomotiveCount, pool.MaxLocomotiveCount);
            if (locomotiveCount > trainLength)
                locomotiveCount = trainLength;

            var savedGateIndexes = GetGateIndexes(ref vehicle);
            RemoveTrailers(ref vehicle, id);

            var prevVehicleId = id;

            var totalSpawnedCount = 1;
            // Spawn leading locomotives
            for (var i = 0; i < locomotiveCount; i++)
            {
                var isFirst = i == 0;
                var locomotive = isFirst ? leadLocomotive : pool.Locomotives[randomizer.Int32((uint)pool.Locomotives.Count)].Reference;
                var spawnedCount = SpawnLocomotive(out var trailerId, prevVehicleId, locomotive, randomizer, isFirst);

                // Prevent spawning too many locomotives
                if ((isFirst || spawnedCount > 1) && !locomotive.IsSingleUnit)
                    i += (spawnedCount - 1);

                if (spawnedCount == 0)
                    continue;

                totalSpawnedCount += spawnedCount;
                prevVehicleId = trailerId;
            }

            // Spawn rest of the train
            var wagonCount = trainLength - locomotiveCount;
            
            if (pool.UseCargo && vehicle.Info.m_vehicleAI is CargoTrainAI cargoTrainAI)
            {
                // TODO: I have the suspicion this doesn't really work properly based on the trains I'm getting in-game
                var cargoCounts = GetCargoContents(ref vehicle);
                var trailerCounts = CalculateTrailerCount(cargoCounts, wagonCount, cargoTrainAI.m_cargoCapacity);
                var cargoIndex = 0;
                var spawnEmpties = false;
                for (var i = 0; i < wagonCount && totalSpawnedCount < trainLength; i++)
                {
                    var trailers = spawnEmpties ? trailerCounts.EmptyTrailers : trailerCounts.LoadedTrailers;
                    // Skip ahead to the next cargoIndex that has trailers!
                    cargoIndex = FindNextCargoIndexWithTailers(trailers, cargoIndex);
                    while (cargoIndex < 0)
                    {
                        if (!spawnEmpties)
                        {
                            // Switch to spawning empty trailers
                            spawnEmpties = true;
                            trailers = trailerCounts.EmptyTrailers;
                            cargoIndex = FindNextCargoIndexWithTailers(trailers, cargoIndex);
                        }
                        else
                        {
                            // TODO: This branch seems to be taken a lot?
                            // Ran out of trailers... spawn generic goods
                            cargoIndex = 8;
                        }
                    }

                    var spawnedCount = SpawnCargoTrailer(out var trailerId, prevVehicleId, pool, randomizer, spawnEmpties, cargoIndex, out var assignedCargoIndex);

                    if (spawnedCount == 0)
                        continue;

                    _logger.Add($"Trailer: Empty = {spawnEmpties}, Desired cargo {cargoIndex}, got cargo {assignedCargoIndex}");

                    // Subtract the spawned trailers from the available trailer count for this cargo type
                    trailers[cargoIndex] -= spawnedCount;

                    totalSpawnedCount += spawnedCount;
                    prevVehicleId = trailerId;
                }
            }
            else
            {
                for (var i = 0; i < wagonCount && totalSpawnedCount < trainLength; i++)
                {
                    var gateIndex = (byte)Mathf.Clamp(i, 0, savedGateIndexes.Count - 1);
                    var spawnedCount = SpawnRandomTrailer(out var trailerId, prevVehicleId, pool, randomizer, gateIndex);

                    if (spawnedCount == 0)
                        continue;

                    _logger.Add($"Trailer: Gate index {gateIndex}");

                    totalSpawnedCount += spawnedCount;
                    prevVehicleId = trailerId;
                }
            }

            // Unset MiddleTrailer on last trailer
            ClearFlagsFromVehicle(prevVehicleId, 0, Vehicle.Flags2.MiddleTrailer);

            Util.Log($"Spawned {locomotiveCount} locomotives for a total train length of {trainLength} for {vehicle.Info.name} [{id}] using pool {pool.Name}");
            _logger.Log();
        }

        private static int FindNextCargoIndexWithTailers(int[] trailers, int cargoIndex)
        {
            if (cargoIndex < 0)
                cargoIndex = 0;

            while (cargoIndex < trailers.Length && trailers[cargoIndex] <= 0)
                cargoIndex++;
            if (cargoIndex >= trailers.Length)
                return -1;
            return cargoIndex;
        }

        private static int[] GetCargoContents(ref Vehicle vehicle)
        {
            var cargoContents = new int[CargoParcel.ResourceTypes.Length];
            var cargoId = vehicle.m_firstCargo;
            var manager = VehicleManager.instance;
            while (cargoId != 0)
            {
                // Turn the actual cargo type into our own cargo type. It's lossy but it works well enough.
                var index = CargoParcel.LowestFlagToIndex(CargoParcel.TransferToFlags((TransferManager.TransferReason)manager.m_vehicles.m_buffer[cargoId].m_transferType));
                if (index < 0)
                {
                    Util.LogError("Invalid cargo index found for cargo vehicle id " + cargoId);
                    break;
                }
                if (index >= cargoContents.Length)
                {
                    Util.LogError("Cargo Index " + index + " out of bounds for cargo vehicle " + cargoId);
                }
                cargoContents[index]++;

                // debug_cargoContents.Add(string.Format("Cargo Type {0}, original was {1}", CargoParcel.FlagIndexToName(index), CargoParcel.TransferToName(manager.m_vehicles.m_buffer[cargoId].m_transferType)));

                cargoId = manager.m_vehicles.m_buffer[cargoId].m_nextCargo;
            }

            return cargoContents;
        }

        private struct TrailerCount
        {
            public int[] LoadedTrailers;
            public int[] EmptyTrailers;

            public static TrailerCount Create()
            {
                return new TrailerCount
                {
                    LoadedTrailers = new int[CargoParcel.ResourceTypes.Length],
                    EmptyTrailers = new int[CargoParcel.ResourceTypes.Length],
                };
            }
        }

        private static TrailerCount CalculateTrailerCount(int[] cargoContents, int totalTrailerCount, int cargoCapacity)
        {
            var log = new DeferredLogger();

            var result = TrailerCount.Create();
            var assignedCargoWagons = 0;
            if (totalTrailerCount > 0)
            {
                for (int i = 0; i < cargoContents.Length; i++)
                {
                    result.LoadedTrailers[i] = (cargoContents[i] * totalTrailerCount + cargoCapacity - 1) / cargoCapacity;
                    assignedCargoWagons += result.LoadedTrailers[i];
                    log.Add($"{result.LoadedTrailers[i]} trailers for cargo index {i}");
                }

                // Assign what cargo types the empties should have
                if (assignedCargoWagons > 0)
                {
                    // Round-robin esque assignment of empties
                    var emptySlots = totalTrailerCount - assignedCargoWagons;
                    int k = 0;
                    while (emptySlots > 0)
                    {
                        while (result.LoadedTrailers[k % result.LoadedTrailers.Length] == 0) { k++; }
                        result.EmptyTrailers[k % result.EmptyTrailers.Length]++;
                        emptySlots--;
                        k++;
                    }
                }
                else
                {
                    // All in generic goods, the above algorithm would run forever
                    result.EmptyTrailers[8] = totalTrailerCount;
                }

                for (var i = 0; i < result.EmptyTrailers.Length; i++)
                    log.Add($"{result.EmptyTrailers[i]} empty trailers for cargo index {i}");
            }

            log.Log();

            return result;
        }

        private static int SpawnLocomotive(out ushort lastId, ushort previousId, Locomotive locomotive, Randomizer randomizer, bool skipFirst = false)
        {
            lastId = 0;

            var trailerCount = Mathf.Clamp(locomotive.Length - 1, 0, locomotive.VehicleInfo.m_trailers?.Length ?? 0);
            var spawnedCount = 0;

            // Spawn the leading part. This may be skipped
            ushort trailerId;
            if (!skipFirst)
            {
                if (!SpawnTrailer(out trailerId, previousId, locomotive.VehicleInfo, false, 0))
                    return 0;
                spawnedCount++;
                previousId = trailerId;
            }

            // Spawn the 'trailers' that make up the rest of the locomotive
            for(var i = 0; i < trailerCount; i++)
            {
                var trailer = locomotive.VehicleInfo.m_trailers[i];

                if (SpawnTrailer(out trailerId, previousId, trailer.m_info, randomizer.Int32(100u) < trailer.m_invertProbability, 0))
                {
                    previousId = trailerId;
                    spawnedCount++;
                }
            }

            lastId = previousId;

            _logger.Add($"Locomotive: {locomotive.AssetName}");

            return spawnedCount;
        }

        private static IList<byte> GetGateIndexes(ref Vehicle vehicle)
        {
            var gateIndexes = new List<byte>();
            var manager = Singleton<VehicleManager>.instance;
            ushort trailerId = vehicle.m_trailingVehicle;
            while (trailerId != 0)
            {
                var gateIdx = manager.m_vehicles.m_buffer[trailerId].m_gateIndex;
                gateIndexes.Add(gateIdx);
                trailerId = manager.m_vehicles.m_buffer[trailerId].m_trailingVehicle;
            }

            return gateIndexes;
        }

        private static void RemoveTrailers(ref Vehicle vehicle, ushort id)
        {
            var manager = Singleton<VehicleManager>.instance;
            // Unhook first trailer from lead and remove it. This will remove all trailers.
            var trailerId = vehicle.m_trailingVehicle;
            manager.m_vehicles.m_buffer[trailerId].m_leadingVehicle = 0;
            manager.m_vehicles.m_buffer[id].m_trailingVehicle = 0;
            manager.ReleaseVehicle(trailerId);
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
        private static int SpawnCargoTrailer(out ushort lastTrailerId, ushort prevVehicleId, IRandomTrailerCollection trailerCollection, Randomizer randomizer, bool empty, int cargoFlagIndex, out int assignedCargoIndex)
        {
            var trailer = GetTrailerForCargo(out assignedCargoIndex, trailerCollection, cargoFlagIndex, randomizer);

            if(trailer == null)
            {
                Util.LogError("Unable to find trailer for cargo type " + cargoFlagIndex + " or its fallbacks in collection " + trailerCollection.Name);
                lastTrailerId = 0;
                return 0;
            }

            byte gateIndex = CargoParcel.FlagIndexToGateIndex(assignedCargoIndex);
            return SpawnTrailerDefinition(out lastTrailerId, prevVehicleId, trailer, randomizer, empty ? CargoParcel.GetEmptyGateIndex(gateIndex) : gateIndex);
        }

        private static Trailer GetTrailerForCargo(out int assignedCargoIndex, IRandomTrailerCollection collection, int cargoIndex, Randomizer randomizer)
        {
            for(int i = 0; i < CargoParcel.ResourceFallback[cargoIndex].Length; i++)
            {
                var attemptedCargoType = CargoParcel.LowestFlagToIndex(CargoParcel.ResourceFallback[cargoIndex][i]);
                var trailer = collection.GetTrailerForCargo(attemptedCargoType, randomizer);
                if (trailer != null)
                {
                    assignedCargoIndex = attemptedCargoType;
                    return trailer;
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
        private static int SpawnRandomTrailer(out ushort lastTrailerId, ushort prevVehicleId, IRandomTrailerCollection trailerCollection, Randomizer randomizer, byte gateIndex)
        {
            var trailer = trailerCollection.GetTrailer(randomizer);

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
        private static int SpawnTrailerDefinition(out ushort lastTrailerId, ushort prevVehicleId, Trailer trailer, Randomizer randomizer, byte gateIndex)
        {
            lastTrailerId = 0;

            // Spawn the trailer
            if(trailer.IsMultiTrailer)
            {
                // Spawn all subtrailers
                var infos = trailer.VehicleInfos;
                for(int i = 0; i < infos.Count; i++)
                {
                    if (SpawnTrailer(out ushort trailerId, prevVehicleId, infos[i], randomizer.Int32(100u) < trailer.SubTrailers[i].InvertProbability, gateIndex))
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
            var info = trailer.VehicleInfos[0];
            if(SpawnTrailer(out lastTrailerId, prevVehicleId, info, randomizer.Int32(100u) < trailer.InvertProbability, gateIndex))
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
        private static bool SpawnTrailer(out ushort trailerId, ushort prevVehicleId, VehicleInfo trailerInfo, bool inverted, byte gateIndex)
        {
            ref Vehicle prevVehicle = ref Singleton<VehicleManager>.instance.m_vehicles.m_buffer[prevVehicleId];
            trailerId = prevVehicle.CreateTrailer(prevVehicleId, trailerInfo, inverted);
            if(trailerId == 0) { return false; }

            Singleton<VehicleManager>.instance.m_vehicles.m_buffer[trailerId].m_gateIndex = gateIndex;
            Singleton<VehicleManager>.instance.m_vehicles.m_buffer[trailerId].Spawn(trailerId);
            Singleton<VehicleManager>.instance.m_vehicles.m_buffer[trailerId].m_flags2 |= (Vehicle.Flags2)ExtendedVehicleFlags.NoLights;
            Singleton<VehicleManager>.instance.m_vehicles.m_buffer[trailerId].m_flags2 |= Vehicle.Flags2.MiddleTrailer;

            _logger.Add($"Created vehicle [{trailerId}] '{trailerInfo.name}', Inverted = {inverted}, Gate Index = {gateIndex}");

            return true;
        }

        private static void ClearFlagsFromVehicle(ushort vehicleId, Vehicle.Flags flags, Vehicle.Flags2 flags2)
        {
            ref Vehicle vehicle = ref Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicleId];
            vehicle.m_flags &= ~flags;
            vehicle.m_flags2 &= ~flags2;
        }
    }
}
