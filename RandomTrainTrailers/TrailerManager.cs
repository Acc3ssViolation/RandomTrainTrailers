using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

namespace RandomTrainTrailers
{
    public static class TrailerManager
    {
        static Dictionary<string, TrailerDefinition.Vehicle> vehicleDict = new Dictionary<string, TrailerDefinition.Vehicle>();
        static Dictionary<string, TrailerDefinition.TrailerCollection> collectionDict = new Dictionary<string, TrailerDefinition.TrailerCollection>();

        //static HashSet<string> blacklist = new HashSet<string>();

        public static void Setup()
        {
            vehicleDict.Clear();
            collectionDict.Clear();
            //blacklist.Clear();

            var def = DefaultTrailerConfig.DefaultDefinition;
            ApplyDefinition(ref def);
            LoadUserDefinition();

            DumpToLog();
        }

        private static void LoadUserDefinition()
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

                    // Apply user definition
                    ApplyDefinition(ref definition);

                    Util.Log("Finished loading user definition from " + path);
                }
                else
                {
                    Util.Log("No user definition found at " + path + " creating empty definition file");

                    var definition = new TrailerDefinition();
                    using(StreamWriter sw = new StreamWriter(path))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(TrailerDefinition));
                        serializer.Serialize(sw, definition);
                    }
                }
            }
            catch(Exception e)
            {
                Util.LogError("Exception trying to load definition\r\n" + path + "\r\nException:\r\n" + e.Message + "\r\n" + e.StackTrace);
            }
        }

        /// <summary>
        /// Updates vehicle dictionary and blacklist
        /// </summary>
        /// <param name="definition">Definition to add. May be modified in the process!</param>
        private static void ApplyDefinition(ref TrailerDefinition definition)
        {
            /*foreach(var item in definition.Blacklist)
            {
                if(item.GetInfo() != null)
                {
                    blacklist.Add(item.AssetName);
                }
            }

            Util.Log(blacklist.Aggregate("Trailer blacklist:\r\n", (list, item) => list + item + "\r\n"));
            */

            Util.Log("Adding trailer collections...", true);
            // Add trailer collections
            foreach(var collection in definition.Collections)
            {
                var ct = collection.Trailers;
                CleanTrailerList(ref ct, new HashSet<string>());
                
                if(collection.Trailers.Count > 0)
                {
                    if(!collectionDict.ContainsKey(collection.Name))
                    {
                        collectionDict.Add(collection.Name, collection);
                    }
                    else
                    {
                        Util.LogError("Duplicate collection name! " + collection.Name);
                    }                   
                }
                else
                {
                    Util.LogWarning("Ignoring collection " + collection.Name + " due to it having 0 trailers\nYou may want to check if all referenced trailer assets are loaded!");
                }
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
                vehicle.m_trailerCollections = new List<TrailerDefinition.Vehicle.Collection>();
                // Make sure the vehicle's own trailer collection is added as the first collection
                var vehicleInlineCollection = new TrailerDefinition.TrailerCollection(vehicle.AssetName + " Collection");
                vehicle.m_trailerCollections.Add(new TrailerDefinition.Vehicle.Collection()
                {
                    m_trailerCollection = vehicleInlineCollection,
                    m_weight = 10
                });

                foreach(var trailerDef in vehicle.Trailers)
                {
                    if(trailerDef.IsCollection)
                    {
                        TrailerDefinition.TrailerCollection collection;
                        if(collectionDict.TryGetValue(trailerDef.AssetName, out collection))
                        {
                            vehicle.m_trailerCollections.Add(new TrailerDefinition.Vehicle.Collection()
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
                                vehicleInlineCollection.Trailers.Add(new TrailerDefinition.Trailer(info.m_trailers[i].m_info)
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
                var remove = v.m_trailerCollections[0].m_trailerCollection.Trailers.Count < 1;              // The first one is the vehicle's own trailer collection
                if(remove)
                    Util.LogWarning("Removing " + v.AssetName + " due to a lack of trailers!");
                return remove;
            });

            // All vehicles should now be valid
            // Add definition's vehicles to the dictionary
            foreach(var vehicle in definition.Vehicles)
            {
                if(!vehicleDict.ContainsKey(vehicle.AssetName))
                {
                    vehicleDict.Add(vehicle.AssetName, vehicle);
                }
                else
                {
                    Util.LogError("Duplicate trailer definition: " + vehicle.AssetName);
                }
            }

            Util.Log("Finished adding definition.", true);
        }

        private static void DumpToLog()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Vehicles:");
            foreach(var vehicle in vehicleDict.Values)
            {
                sb.AppendLine("\tName: " + vehicle.AssetName);
                sb.AppendLine("\tChance: " + vehicle.RandomTrailerChance);
                sb.AppendLine("\tStart: " + vehicle.StartOffset);
                sb.AppendLine("\tEnd: " + vehicle.EndOffset);
                sb.AppendLine("\tCollections:");
                foreach(var collection in vehicle.m_trailerCollections)
                {
                    sb.AppendLine("\t\tName: " + collection.m_trailerCollection.Name);
                    sb.AppendLine("\t\tWeight: " + collection.m_weight);
                    foreach(var trailer in collection.m_trailerCollection.Trailers)
                    {
                        sb.AppendLine("\t\t\tTrailer: " + trailer.AssetName);
                        sb.AppendLine("\t\t\tWeight: " + trailer.Weight);
                        
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
                sb.AppendLine();
            }

            Util.Log(sb.ToString());
        }

        /// <summary>
        /// Removes any invalid trailer entries from the list.
        /// ref is only used to indicate the list contents will be changed the reference itself will not be changed.
        /// </summary>
        /// <param name="trailers">The list of trailers to clean.</param>
        /// <param name="localBlacklist">HashSet containing the names of trailers to always remove.</param>
        private static void CleanTrailerList(ref List<TrailerDefinition.Trailer> trailers, HashSet<string> localBlacklist)
        {
            // Remove broken trailers
            trailers.RemoveAll((t) =>
            {
                var remove = false;
                if(t.IsMultiTrailer())
                {
                    remove = t.IsCollection || t.GetInfos() == null || t.SubTrailers.Count < 1 || localBlacklist.Contains(t.AssetName);

                    if(remove)
                        Util.Log("Removing multitrailer " + t.AssetName + " due to being a collection, missing infos, invalid trailer count or blacklist!");
                }
                else
                {
                    remove = t.IsCollection || t.GetInfo() == null || localBlacklist.Contains(t.AssetName);

                    if(remove)
                        Util.Log("Removing trailer " + t.AssetName + " due to being a collection, missing VehicleInfo or blacklist!");
                }
                return remove;
            });
        }

        /// <summary>
        /// Get config for asset, if any.
        /// </summary>
        /// <param name="assetName">The FULL name of the asset</param>
        public static TrailerDefinition.Vehicle GetVehicleConfig(string assetName)
        {
            TrailerDefinition.Vehicle vehicle = null;
            vehicleDict.TryGetValue(assetName, out vehicle);
            return vehicle;
        }
    }
}
