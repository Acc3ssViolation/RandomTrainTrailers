using ColossalFramework;
using ColossalFramework.Packaging;
using RandomTrainTrailers.Definition;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace RandomTrainTrailers
{
    public static class TrailerManager
    {
        public static readonly SavedInt GlobalTrailerLimit = new SavedInt("globalTrailerLimit", Mod.settingsFile, -1, true);

        private static Dictionary<string, Definition.Vehicle> _vehicleDict = new Dictionary<string, Definition.Vehicle>();
        private static Dictionary<string, TrailerCollection> _collectionDict = new Dictionary<string, TrailerCollection>();
        private static Dictionary<string, Locomotive> _locomotiveMap = new Dictionary<string, Locomotive>();
        private static Dictionary<string, IList<TrainPool>> _leadVehicleToPool = new Dictionary<string, IList<TrainPool>>();

        private static HashSet<string> _removedTrailers = new HashSet<string>();
        private static HashSet<string> _removedCollections = new HashSet<string>();
        private static HashSet<string> _removedVehicles = new HashSet<string>();

        public static void Setup()
        {
            _removedTrailers.Clear();
            _removedCollections.Clear();
            _removedVehicles.Clear();
            _vehicleDict.Clear();
            _collectionDict.Clear();
            _leadVehicleToPool.Clear();
            _locomotiveMap.Clear();

            ConfigurationManager.instance.Reset();

            // Load default
            ConfigurationManager.instance.Add(ConfigurationManager.Default, DefaultTrailerConfig.DefaultDefinition);

            // Load mods and assets
            var loader = new SharedTrailerConfigLoader();
            var checkedPaths = new HashSet<string>();
            // Load definitions from mod folders
            foreach(var current in ColossalFramework.Plugins.PluginManager.instance.GetPluginsInfo())
            {
                if(current.isEnabled)
                {
                    var path = Path.Combine(current.modPath, loader.FileName);
                    // skip files which were already parsed
                    if(checkedPaths.Contains(path)) continue;
                    checkedPaths.Add(path);
                    if(!File.Exists(path)) continue;
                    loader.OnFileFound(path, current.name, true);
                }
            }

            // Load definitions from prefabs
            for(uint i = 0; i < PrefabCollection<VehicleInfo>.LoadedCount(); i++)
            {
                var prefab = PrefabCollection<VehicleInfo>.GetLoaded(i);

                // Check if asset is valid
                if(prefab == null) continue;

                var asset = PackageManager.FindAssetByName(prefab.name);

                var crpPath = asset?.package?.packagePath;
                if(crpPath == null) continue;

                var path = Path.Combine(Path.GetDirectoryName(crpPath) ?? "", loader.FileName);
                // skip files which were already parsed
                if(checkedPaths.Contains(path)) continue;
                checkedPaths.Add(path);
                if(!File.Exists(path)) continue;
                loader.OnFileFound(path, asset.package.packageName, false);
            }

            ConfigurationManager.instance.Add(ConfigurationManager.User, GetUserDefinitionFromDisk());

            // Create a copy of the combined one because ApplyDefinition makes edits to it to filter out unloaded assets
            var combined = ConfigurationManager.instance.GetCombinedDefinition().Copy();
            ApplyDefinition(ref combined);

            LogRemovedAssets();
            DumpToLog();
        }

        private static void LogRemovedAssets()
        {
            if(_removedTrailers.Count > 0) Util.Log("The following trailers were removed from 1 or more vehicles/collections:\r\n" + _removedTrailers.Aggregate((sequence, next) => sequence + "\r\n" + next));
            if(_removedCollections.Count > 0) Util.Log("The following collections were removed due to being invalid or having 0 loaded trailers:\r\n" + _removedCollections.Aggregate((sequence, next) => sequence + "\r\n" + next));
            if(_removedVehicles.Count > 0) Util.Log("The following vehicles were removed due to having no loaded collections or trailers:\r\n" + _removedVehicles.Aggregate((sequence, next) => sequence + "\r\n" + next));
        }

        private static TrailerDefinition GetUserDefinitionFromDisk()
        {
            // User definition is stored in the game app data
            string path = Path.Combine(ColossalFramework.IO.DataLocation.localApplicationData, "RTT-Definition.xml");
            try
            {
                if(File.Exists(path))
                {
                    TrailerDefinition definition;

                    using(StreamReader sr = new StreamReader(path))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(TrailerDefinition));
                        definition = (TrailerDefinition)serializer.Deserialize(sr);
                    }

                    return definition;
                }
            }
            catch(Exception e)
            {
                Util.LogError("Exception trying to load definition\r\n" + path + "\r\nException:\r\n" + e.Message + "\r\n" + e.StackTrace);
            }
            
            return new TrailerDefinition();
        }

        public static bool StoreUserDefinitionOnDisk(TrailerDefinition definition)
        {
            // User definition is stored in the game app data
            string path = Path.Combine(ColossalFramework.IO.DataLocation.localApplicationData, "RTT-Definition.xml");
            try
            {
                using(StreamWriter sw = new StreamWriter(path, false))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(TrailerDefinition));
                    serializer.Serialize(sw, definition);
                }
                return true;
            }
            catch(Exception e)
            {
                Util.LogError("Exception trying to save definition\r\n" + path + "\r\nException:\r\n" + e.Message + "\r\n" + e.StackTrace);
            }
            return false;
        }

        /// <summary>
        /// Updates vehicle dictionary and blacklist
        /// </summary>
        /// <param name="definition">Definition to add. May be modified in the process!</param>
        private static void ApplyDefinition(ref TrailerDefinition definition)
        {
            if(definition == null)
                return;

            Util.Log("Adding trailer collections...", true);
            // Add trailer collections
            foreach(var collection in definition.Collections)
            {
                if(!_collectionDict.ContainsKey(collection.Name))
                {
                    _collectionDict.Add(collection.Name, collection);
                }
                else
                {
                    Util.LogWarning("Duplicate collection name, overriding previous collection! " + collection.Name);
                    _collectionDict[collection.Name] = collection;
                }
            }
            // Extend and clean collections
            foreach(var collection in definition.Collections)
            {
                if(collection.BaseCollection != null)
                {
                    if(_collectionDict.TryGetValue(collection.BaseCollection, out var baseCollection))
                    {
                        foreach(var trailer in baseCollection.Trailers)
                        {
                            // Do it manually to prevent duplicates if a collection gets loaded multiple times
                            if(collection.Trailers.Contains(trailer)) { continue; }
                            collection.Trailers.Add(trailer);
                        }
                        
                    }
                    else
                    {
                        Util.LogWarning(string.Format("Base collection {0} used by {1} was not loaded", collection.BaseCollection, collection.Name));
                    }
                }

                var ct = collection.Trailers;
                CleanTrailerList(ref ct, new HashSet<string>());
                if(collection.Trailers.Count == 0)
                {
                    _removedCollections.Add(collection.Name);
                }
            }
            // Clean up the dict
            foreach(var entry in _removedCollections)
            {
                _collectionDict.Remove(entry);
            }

            Util.Log("Adding vehicle entries...", true);
            // Remove missing vehicles
            definition.Vehicles.RemoveAll((v) => {
                return v.GetInfo() == null;
            });

            // A vehicle's own blacklist
            var localBlacklist = new HashSet<string>();

            foreach(var vehicle in definition.Vehicles)
            {
                localBlacklist.Clear();

                foreach(var item in vehicle.LocalBlacklist)
                {
                    if(item.GetInfo() != null)
                    {
                        localBlacklist.Add(item.AssetName);
                    }
                }

                // Dump blacklist to output
                Util.Log(localBlacklist.Aggregate("Local trailer blacklist for " + vehicle.AssetName + ":\r\n", (list, item) => list + item + "\r\n"));


                // Convert trailer list to list of trailer collections
                vehicle.m_trailerCollections = new List<Definition.Vehicle.Collection>();
                // Make sure the vehicle's own trailer collection is added as the first collection
                var vehicleInlineCollection = new TrailerCollection(vehicle.AssetName + " Collection");
                vehicle.m_trailerCollections.Add(new Definition.Vehicle.Collection()
                {
                    m_trailerCollection = vehicleInlineCollection,
                    m_weight = 10
                });

                foreach(var trailerDef in vehicle.Trailers)
                {
                    if(trailerDef.IsCollection)
                    {
                        if(_collectionDict.TryGetValue(trailerDef.AssetName, out var collection))
                        {
                            vehicle.m_trailerCollections.Add(new Definition.Vehicle.Collection()
                            {
                                m_trailerCollection = collection,
                                m_weight = trailerDef.Weight
                            });
                        }
                        else
                        {
                            Util.LogWarning("Vehicle " + vehicle.AssetName + " needs unknown trailer collection " + trailerDef.AssetName);
                        }
                    }
                    else
                    {
                        vehicleInlineCollection.Trailers.Add(trailerDef);
                    }
                }

                if(vehicle.AllowDefaultTrailers)
                {
                    // Add default trailers to trailer list
                    var info = vehicle.GetInfo();
                    var names = new HashSet<string>();
                    if(info.m_trailers != null)
                    {
                        for(int i = 0; i < info.m_trailers.Length; i++)
                        {
                            // Make sure each trailer is only added once
                            if(!names.Contains(info.m_trailers[i].m_info.name))
                            {
                                vehicleInlineCollection.Trailers.Add(new Trailer(info.m_trailers[i].m_info)
                                {
                                    InvertProbability = info.m_trailers[i].m_invertProbability,
                                });
                                names.Add(info.m_trailers[i].m_info.name);
                            }
                        }
                    }
                }

                var vit = vehicleInlineCollection.Trailers;
                CleanTrailerList(ref vit, localBlacklist);
            }   // foreach vehicle

            // Remove vehicles that have no trailers
            definition.Vehicles.RemoveAll((v) =>
            {
                if(v.m_trailerCollections.Count == 1)
                {
                    // Only the inline trailer collection is present
                    var remove = v.m_trailerCollections[0].m_trailerCollection.Trailers.Count < 1;
                    if(remove)
                        _removedVehicles.Add(v.AssetName);
                    return remove;
                }
                else
                {
                    // There are other collections (and trailers), remove inline if it's empty
                    if(v.m_trailerCollections[0].m_trailerCollection.Trailers.Count == 0)
                    {
                        v.m_trailerCollections.RemoveAt(0);
                    }
                    return false;
                }
            });

            // Load locomotives
            foreach (var locomotive in definition.Locomotives)
            {
                if (locomotive.VehicleInfo == null)
                    continue;

                _locomotiveMap[locomotive.AssetName] = locomotive;
            }

            // Create a mapping from potential lead vehicles to the pools they are in
            foreach (var pool in definition.TrainPools)
            {
                pool.RemoveUnavailableLocomotives(_locomotiveMap);
                pool.RemoveUnavailableCollections(_collectionDict);
                if (!pool.IsValid())
                {
                    Util.LogError($"Pool '{pool.Name}' is invalid and will not be loaded");
                    continue;
                }

                foreach (var locomotiveRef in pool.Locomotives)
                {
                    var locomotive = locomotiveRef.Reference;
                    if (!locomotive.CanBeLeadVehicle)
                        continue;

                    if (!_leadVehicleToPool.TryGetValue(locomotive.AssetName, out var poolsForLocomotive))
                    {
                        poolsForLocomotive = new List<TrainPool>();
                        _leadVehicleToPool[locomotive.AssetName] = poolsForLocomotive;
                    }
                    poolsForLocomotive.Add(pool);
                }
            }

            // All vehicles should now be valid
            // Add definition's vehicles to the dictionary
            foreach(var vehicle in definition.Vehicles)
            {
                if(!_vehicleDict.ContainsKey(vehicle.AssetName))
                {
                    _vehicleDict.Add(vehicle.AssetName, vehicle);
                }
                else
                {
                    Util.LogWarning("Duplicate vehicle definition, overriding: " + vehicle.AssetName);
                    _vehicleDict[vehicle.AssetName] = vehicle;
                }

                // Cargo
                foreach(var collection in vehicle.m_trailerCollections)
                {
                    if(collection.m_trailerCollection.m_cargoData != null)
                    {
                        continue;
                    }

                    collection.m_trailerCollection.m_cargoData = new TrailerCollection.CargoData();

                    // Use lists in the adding process
                    var lists = new List<Trailer>[CargoParcel.ResourceTypes.Length];
                    for(int i = 0; i < lists.Length; i++)
                    {
                        lists[i] = new List<Trailer>();
                    }

                    foreach(var trailer in collection.m_trailerCollection.Trailers)
                    {
                        if(trailer.IsCollection) continue;

                        if(trailer.CargoType == CargoFlags.None)
                        {
                            // add to all
                            for(int i = 0; i < lists.Length; i++)
                            {
                                lists[i].Add(trailer);
                            }
                        }
                        else
                        {
                            // Add to flagged types
                            for(int i = 0; i < CargoParcel.ResourceTypes.Length; i++)
                            {
                                if((trailer.CargoType & CargoParcel.ResourceTypes[i]) > 0)
                                {
                                    lists[i].Add(trailer);
                                }
                            }
                        }
                    }

                    // Convert lists to arrays and store them

                    for(int i = 0; i < lists.Length; i++)
                    {
                        collection.m_trailerCollection.m_cargoData.m_trailers[i] = lists[i].ToArray();
                    }
                }
            }

            GC.Collect();

            Util.Log("Finished adding definition.", true);
        }

        /// <summary>
        /// Dumps the dictionary to log
        /// </summary>
        private static void DumpToLog()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Collections:");
            foreach(var collection in _collectionDict.Values)
            {
                sb.AppendLine("\tName: " + collection.Name);
                sb.AppendLine("\tBase: " + Convert.ToString(collection.BaseCollection));
                foreach(var trailer in collection.Trailers)
                {
                    sb.AppendLine("\t\t\tTrailer: " + trailer.AssetName);
                    sb.AppendLine("\t\t\tWeight: " + trailer.Weight);
                    sb.AppendLine("\t\t\tCargo: " + trailer.CargoType);

                    if(trailer.IsMultiTrailer())
                    {
                        foreach(var sub in trailer.SubTrailers)
                        {
                            sb.AppendLine("\t\t\t\tSubtrailer: " + sub.AssetName);
                        }
                    }
                    else
                    {
                        sb.AppendLine("\t\t\tInvert: " + trailer.InvertProbability);
                    }
                    sb.AppendLine();
                }
                sb.AppendLine();
            }

            sb.AppendLine("Vehicles:");
            foreach(var vehicle in _vehicleDict.Values)
            {
                sb.AppendLine("\tName: " + vehicle.AssetName);
                sb.AppendLine("\tChance: " + vehicle.RandomTrailerChance);
                sb.AppendLine("\tStart: " + vehicle.StartOffset);
                sb.AppendLine("\tEnd: " + vehicle.EndOffset);
                if(vehicle.TrailerCountOverride != null)
                {
                    sb.AppendLine("\tMin Trailers: " + vehicle.TrailerCountOverride.Min);
                    sb.AppendLine("\tMax Trailers: " + vehicle.TrailerCountOverride.Max);
                    if(!vehicle.TrailerCountOverride.IsValid)
                    {
                        sb.AppendLine("\tINVALID TRAILER COUNT OVERRIDE SETTINGS");
                    }
                }
                sb.AppendLine("\tUseCargo: " + vehicle.UseCargoContents.ToString());
                sb.AppendLine("\tCollections:");
                foreach(var collection in vehicle.m_trailerCollections)
                {
                    sb.AppendLine("\t\tName: " + collection.m_trailerCollection.Name);
                    sb.AppendLine("\t\tWeight: " + collection.m_weight);
                    if(!_collectionDict.ContainsKey(collection.m_trailerCollection.Name))
                    {
                        foreach(var trailer in collection.m_trailerCollection.Trailers)
                        {
                            sb.AppendLine("\t\t\tTrailer: " + trailer.AssetName);
                            sb.AppendLine("\t\t\tWeight: " + trailer.Weight);
                            sb.AppendLine("\t\t\tCargo: " + trailer.CargoType);

                            if(trailer.IsMultiTrailer())
                            {
                                foreach(var sub in trailer.SubTrailers)
                                {
                                    sb.AppendLine("\t\t\t\tSubtrailer: " + sub.AssetName);
                                }
                            }
                            else
                            {
                                sb.AppendLine("\t\t\tInvert: " + trailer.InvertProbability);
                            }
                            sb.AppendLine();
                        }
                    }
                    
                    sb.AppendLine();
                }
                sb.AppendLine();
            }

            sb.AppendLine("Locomotives:");
            foreach (var locomotive in _locomotiveMap.Values)
            {
                sb.AppendLine("\tName: " + locomotive.AssetName);
                sb.AppendLine("\tIs lead vehicle: " + locomotive.CanBeLeadVehicle);
                sb.AppendLine("\tType: " + locomotive.Type);
                sb.AppendLine();
            }

            var loggedPools = new HashSet<TrainPool>();
            sb.AppendLine("Train pools:");
            foreach (var pools in _leadVehicleToPool.Values)
            {
                foreach (var pool in pools)
                {
                    if (loggedPools.Contains(pool))
                        continue;
                    loggedPools.Add(pool);

                    sb.AppendLine("\tName: " + pool.Name);
                    sb.AppendLine("\tMin Locomotives: " + pool.MinLocomotiveCount);
                    sb.AppendLine("\tMax Locomotives: " + pool.MaxLocomotiveCount);
                    sb.AppendLine("\tLocomotives:");
                    foreach (var locomotive in pool.Locomotives)
                    {
                        sb.AppendLine("\t\t" + locomotive.Name);
                    }
                    sb.AppendLine("\tCollections:");
                    foreach (var trailerCollection in pool.TrailerCollections)
                    {
                        sb.AppendLine("\t\t" + trailerCollection.Name);
                    }
                    sb.AppendLine();
                }
            }

            Util.Log(sb.ToString());
        }

        /// <summary>
        /// Removes any invalid trailer entries from the list.
        /// </summary>
        /// <param name="trailers">The list of trailers to clean.</param>
        /// <param name="localBlacklist">HashSet containing the names of trailers to always remove.</param>
        private static void CleanTrailerList(ref List<Trailer> trailers, HashSet<string> localBlacklist)
        {
            // Remove broken trailers
            trailers.RemoveAll((t) =>
            {
                var remove = false;
                if(t.IsMultiTrailer())
                {
                    remove = t.IsCollection || t.GetInfos() == null || t.SubTrailers.Count < 1 || localBlacklist.Contains(t.AssetName);

                    //if(remove) removedTrailers.Add(t.AssetName);
                }
                else
                {
                    remove = t.IsCollection || t.GetInfo() == null || localBlacklist.Contains(t.AssetName);

                    //if(remove) removedTrailers.Add(t.AssetName);
                }
                return remove;
            });
        }

        /// <summary>
        /// Get config for asset, if any.
        /// </summary>
        /// <param name="assetName">The FULL name of the asset</param>
        public static Definition.Vehicle GetVehicleConfig(string assetName)
        {
            _vehicleDict.TryGetValue(assetName, out var vehicle);
            return vehicle;
        }

        public static IList<TrainPool> GetVehiclePools(string assetName)
        {
            _leadVehicleToPool.TryGetValue(assetName, out var vehiclePool);
            return vehiclePool;
        }

        public static Dictionary<string, Definition.Vehicle> GetVehicleDictionary()
        {
            return _vehicleDict;
        }

        public static int GetTrailerCountOverride()
        {
            return GlobalTrailerLimit;
        }
    }
}
