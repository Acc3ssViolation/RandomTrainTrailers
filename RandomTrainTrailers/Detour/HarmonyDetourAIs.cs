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
//        }
    }

}
