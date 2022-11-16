using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace RandomTrainTrailers
{
    /// <summary>
    /// Contains vehicle trailer random selection data.
    /// </summary>
    public class TrailerDefinition
    {
        /// <summary>
        /// List of vehicles.
        /// </summary>
        public List<Vehicle> Vehicles { get; set; }

        /// <summary>
        /// List of globally blacklisted trailers.
        /// </summary>
        //public List<BlacklistItem> Blacklist { get; set; }

        /// <summary>
        /// List of trailer collections. Can be shared among vehicles.
        /// </summary>
        public List<TrailerCollection> Collections { get; set; }

        public TrailerDefinition()
        {
            Vehicles = new List<Vehicle>();
            //Blacklist = new List<BlacklistItem>();
            Collections = new List<TrailerCollection>();
        }

        /// <summary>
        /// Returns a deep copy of this TrailerDefinition.
        /// </summary>
        public TrailerDefinition Copy()
        {
            var copy = new TrailerDefinition();

            //foreach(var item in Blacklist)
            //{
            //    copy.Blacklist.Add(item.Copy());
            //}

            foreach(var item in Vehicles)
            {
                copy.Vehicles.Add(item.Copy());
            }

            foreach(var item in Collections)
            {
                copy.Collections.Add(item.Copy());
            }

            return copy;
        }

        /// <summary>
        /// Represents a collection of trailers.
        /// </summary>
        public class TrailerCollection
        {
            public class CargoData
            {
                public int[][] m_trailerCDF;
                public Trailer[][] m_trailers;

                public CargoData()
                {
                    m_trailers = new Trailer[CargoParcel.ResourceTypes.Length][];
                }

                /// <summary>
                /// Returns a random, weighed trailer index for the given cargo type. -1 on failure.
                /// </summary>
                /// <param name="cargoIndex"></param>
                /// <returns></returns>
                public int GetRandomTrailerIndex(int cargoIndex)
                {
                    // Compile CDF array for weighted random selection
                    UpdateCDF();

                    // -1 if we have no trailers
                    if(m_trailerCDF[cargoIndex].Length == 0)
                    {
                        return -1;
                    }

                    // Select random trailer index using the cdf array
                    int randomTrailerIndex = Array.BinarySearch(m_trailerCDF[cargoIndex], Util.Random.Next(m_trailerCDF[cargoIndex][m_trailerCDF[cargoIndex].Length - 1] + 1));
                    if(randomTrailerIndex < 0)
                    {
                        randomTrailerIndex = ~randomTrailerIndex;
                    }
                    if(randomTrailerIndex < 0 || randomTrailerIndex > m_trailers[cargoIndex].Length - 1)
                    {
                        Util.LogError("Index out of bounds! " + randomTrailerIndex);
                    }

                    return randomTrailerIndex;
                }

                public void UpdateCDF(bool force = false)
                {
                    if(m_trailerCDF == null)
                    {
                        m_trailerCDF = new int[CargoParcel.ResourceTypes.Length][];
                    }

                    if(m_trailerCDF[0] == null || force)
                    {
                        for(int cargoIndex = 0; cargoIndex < CargoParcel.ResourceTypes.Length; cargoIndex++)
                        {
                            m_trailerCDF[cargoIndex] = new int[m_trailers[cargoIndex].Length];
                            for(int i = 0; i < m_trailers[cargoIndex].Length; i++)
                            {
                                m_trailerCDF[cargoIndex][i] = m_trailers[cargoIndex][i].Weight + (i > 0 ? m_trailerCDF[cargoIndex][i - 1] : 0);
                            }
                        }
                    }
                }
            }

            /// <summary>
            /// Name of the collection used for identification.
            /// </summary>
            [XmlAttribute("name")]
            public string Name { get; set; }

            /// <summary>
            /// The collection that this collection extends.
            /// </summary>
            [XmlAttribute("base"), DefaultValue(null)]
            public string BaseCollection { get; set; }

            /// <summary>
            /// The trailers of this collection.
            /// </summary>
            public List<Trailer> Trailers { get; set; }

            [XmlIgnore]
            private int[] m_trailerCDF;

            [XmlIgnore]
            public CargoData m_cargoData;

            public TrailerCollection() : this("New Collection")
            {
            }

            public TrailerCollection(string name)
            {
                Name = name;
                BaseCollection = null;
                Trailers = new List<Trailer>();
            }

            /// <summary>
            /// Returns a deep copy of this trailer collection.
            /// </summary>
            public TrailerCollection Copy()
            {
                var copy = new TrailerCollection(Name);

                foreach(var trailer in Trailers)
                {
                    copy.Trailers.Add(trailer.Copy());
                }

                return copy;
            }

            public Trailer GetRandomTrailer()
            {
                return Trailers[GetRandomTrailerIndex()];
            }

            public int GetRandomTrailerIndex()
            {
                if(m_trailerCDF == null)
                {
                    // Compile CDF array for weighted random selection
                    m_trailerCDF = new int[Trailers.Count];
                    for(int i = 0; i < Trailers.Count; i++)
                    {
                        m_trailerCDF[i] = Trailers[i].Weight + (i > 0 ? m_trailerCDF[i - 1] : 0);
                    }
                }

                // Select random trailer index using the cdf array
                int randomTrailerIndex = Array.BinarySearch(m_trailerCDF, Util.Random.Next(m_trailerCDF[m_trailerCDF.Length - 1] + 1));
                if(randomTrailerIndex < 0)
                {
                    randomTrailerIndex = ~randomTrailerIndex;
                }
                if(randomTrailerIndex < 0 || randomTrailerIndex > Trailers.Count - 1)
                {
                    Util.LogError("Index out of bounds! " + randomTrailerIndex);
                }

                return randomTrailerIndex;
            }
        }

        /// <summary>
        /// Represents a blacklisted asset.
        /// </summary>
        public class BlacklistItem
        {
            /// <summary>
            /// Name of the asset to blacklist.
            /// </summary>
            [XmlAttribute("name")]
            public string AssetName { get; set; }

            private VehicleInfo m_info;

            public BlacklistItem()
            {
            }

            public BlacklistItem(string name)
            {
                AssetName = name;
            }


            public BlacklistItem(VehicleInfo info)
            {
                m_info = info;
                AssetName = info.name;
            }

            public VehicleInfo GetInfo()
            {
                if(m_info == null)
                {
                    m_info = Util.FindVehicle(AssetName, "");
                    if(m_info == null)
                        Util.LogWarning(AssetName + " can not be found!");
                    AssetName = m_info != null ? m_info.name : AssetName;
                }
                return m_info;
            }

            public BlacklistItem Copy()
            {
                var copy = new BlacklistItem();
                copy.AssetName = AssetName;
                return copy;
            }
        }

        public class Vehicle
        {
            public class TrailerCount
            {
                [XmlAttribute("min")]
                public int Min { get; set; }
                [XmlAttribute("max")]
                public int Max { get; set; }

                public TrailerCount()
                {
                    Min = Max = -1;
                }

                public bool IsValid
                {
                    get
                    {
                        return Min <= Max && Min >= 0;
                    }
                }
            }

            public struct Collection
            {
                public TrailerCollection m_trailerCollection;
                public int m_weight;
            }

            /// <summary>
            /// Name of the train asset.
            /// </summary>
            [XmlAttribute("name")]
            public string AssetName { get; set; }

            /// <summary>
            /// The type of train this is, only used for allowAny option.
            /// </summary>
            [XmlAttribute("type"), DefaultValue(VehiclePrefabs.VehicleType.Unknown)]
            public VehiclePrefabs.VehicleType VehicleType { get; set; }

            /// <summary>
            /// Chance of actually having random trailers assigned, percentage, defaults to 100.
            /// </summary>
            [XmlAttribute("chance"), DefaultValue(100)]
            public int RandomTrailerChance { get; set; }

            /// <summary>
            /// When true, adds the default trailers of this asset to the list of trailers for the picker. Defaults to true.
            /// </summary>
            [XmlAttribute("useDefault"), DefaultValue(true)]
            public bool AllowDefaultTrailers { get; set; }

            /// <summary>
            /// When true, allows the random trailer picker to pick any trailer valid for this train type, defaults to false.
            /// CURRENTLY NOT WORKING
            /// </summary>
            [XmlAttribute("allowAny"), DefaultValue(false)]
            public bool AllowAnyTrailer { get; set; }

            /// <summary>
            /// The first trailer index (starts at 0) at which we are allowed to randomize trailer selection, defaults to 0.
            /// </summary>
            [XmlAttribute("start"), DefaultValue(0)]
            public int StartOffset { get; set; }

            /// <summary>
            /// Last trailer that is randomized, counted from the rear of the train, defaults to 0.
            /// </summary>
            [XmlAttribute("end"), DefaultValue(0)]
            public int EndOffset { get; set; }

            /// <summary>
            /// Overrides the trailer count when not null.
            /// </summary>
            [DefaultValue(null), XmlElement("TrailerCount")]
            public TrailerCount TrailerCountOverride { get; set; }

            [DefaultValue(false), XmlAttribute("useCargo")]
            public bool UseCargoContents { get; set; }

            /// <summary>
            /// Trailers the picker may use for this train.
            /// </summary>
            public List<Trailer> Trailers { get; set; }

            /// <summary>
            /// Trailers in this local blacklist will never be allowed to spawn on this train, even if they are included in the Trailers list.
            /// </summary>
            public List<BlacklistItem> LocalBlacklist { get; set; }

            [XmlIgnore]
            public int _TrailerCountOverrideMin
            {
                get
                {
                    if(TrailerCountOverride == null)
                    {
                        return -1;
                    }
                    return TrailerCountOverride.Min;
                }
                set
                {
                    if(value >= 0)
                    {
                        if(TrailerCountOverride == null)
                        {
                            TrailerCountOverride = new TrailerCount();
                        }
                        TrailerCountOverride.Min = value;
                    }
                    else
                    {
                        TrailerCountOverride = null;
                    }
                }
            }

            [XmlIgnore]
            public int _TrailerCountOverrideMax
            {
                get
                {
                    if(TrailerCountOverride == null)
                    {
                        return -1;
                    }
                    return TrailerCountOverride.Max;
                }
                set
                {
                    if(value >= 0)
                    {
                        if(TrailerCountOverride == null)
                        {
                            TrailerCountOverride = new TrailerCount();
                        }
                        TrailerCountOverride.Max = value;
                    }
                    else
                    {
                        TrailerCountOverride = null;
                    }
                }
            }

            private VehicleInfo m_info;

            [XmlIgnore]
            public List<Collection> m_trailerCollections;
            [XmlIgnore]
            public int[] m_collectionCDF;

            public Vehicle()
            {
                VehicleType = VehiclePrefabs.VehicleType.Unknown;
                RandomTrailerChance = 100;
                AllowDefaultTrailers = true;
                AllowAnyTrailer = false;
                Trailers = new List<Trailer>();
                LocalBlacklist = new List<BlacklistItem>();
                EndOffset = 0;
                StartOffset = 0;
                UseCargoContents = false;
            }

            public VehicleInfo GetInfo()
            {
                if(m_info == null)
                {
                    m_info = Util.FindVehicle(AssetName, "");
                    if(m_info == null)
                        Util.LogWarning(AssetName + " can not be found!");
                    AssetName = m_info != null ? m_info.name : AssetName;
                }
                return m_info;
            }

            public Vehicle Copy()
            {
                var copy = new Vehicle();

                copy.AssetName = AssetName;
                foreach(var trailer in Trailers)
                {
                    copy.Trailers.Add(trailer.Copy());
                }
                foreach(var blacklist in LocalBlacklist)
                {
                    copy.LocalBlacklist.Add(blacklist.Copy());
                }
                copy.AllowAnyTrailer = AllowAnyTrailer;
                copy.AllowDefaultTrailers = AllowDefaultTrailers;
                copy.RandomTrailerChance = RandomTrailerChance;
                copy.VehicleType = VehicleType;
                copy.StartOffset = StartOffset;
                copy.EndOffset = EndOffset;
                copy.UseCargoContents = UseCargoContents;

                return copy;
            }

            public void CopyFrom(Vehicle vehicle)
            {
                if (AssetName != vehicle.AssetName)
                {
                    AssetName = vehicle.AssetName;
                    m_info = null;
                }
                
                Trailers.Clear();
                LocalBlacklist.Clear();
                foreach (var trailer in vehicle.Trailers)
                {
                    Trailers.Add(trailer.Copy());
                }
                foreach (var blacklist in vehicle.LocalBlacklist)
                {
                    LocalBlacklist.Add(blacklist.Copy());
                }
                AllowAnyTrailer = vehicle.AllowAnyTrailer;
                AllowDefaultTrailers = vehicle.AllowDefaultTrailers;
                RandomTrailerChance = vehicle.RandomTrailerChance;
                VehicleType = vehicle.VehicleType;
                StartOffset = vehicle.StartOffset;
                EndOffset = vehicle.EndOffset;
                UseCargoContents = vehicle.UseCargoContents;
            }

            public TrailerCollection GetRandomCollection()
            {
                // Select a collection
                if(m_trailerCollections.Count > 1)
                {
                    if(m_collectionCDF == null)
                    {
                        m_collectionCDF = new int[m_trailerCollections.Count];
                        for(int i = 0; i < m_trailerCollections.Count; i++)
                        {
                            m_collectionCDF[i] = m_trailerCollections[i].m_weight + (i > 0 ? m_collectionCDF[i - 1] : 0);
                        }
                    }
                   
                    int colIndex = Array.BinarySearch(m_collectionCDF, Util.Random.Next(m_collectionCDF[m_collectionCDF.Length - 1] + 1));
                    if(colIndex < 0)
                    {
                        colIndex = ~colIndex;
                    }
                    if(colIndex < 0 || colIndex > m_trailerCollections.Count - 1)
                    {
                        Util.LogError("Index out of bounds! " + colIndex);
                    }
                    return m_trailerCollections[colIndex].m_trailerCollection;
                }
                else
                {
                    return m_trailerCollections[0].m_trailerCollection;
                }
            }
        }

        public class Trailer
        {
            /// <summary>
            /// Name of the trailer asset. Not required for Multi Trailers.
            /// </summary>
            [XmlAttribute("name")]
            public string AssetName { get; set; }

            /// <summary>
            /// Chance that this trailer is inverted, percentage, defaults to 0.
            /// </summary>
            [XmlAttribute("invertProbability"), DefaultValue(0)]
            public int InvertProbability { get; set; }

            /// <summary>
            /// Weight for the random selection, defaults to 10. (int)
            /// </summary>
            [XmlAttribute("weight"), DefaultValue(10)]
            public int Weight { get; set; }

            /// <summary>
            /// List of sub trailers. When this has elements this trailer is considered to be a Multi Trailer.
            /// </summary>
            public List<Trailer> SubTrailers { get; set; }

            /// <summary>
            /// If this should be treated as a reference to a collection instead of an actual trailer item.
            /// When used as a reference Name should be set to the name of the collection.
            /// </summary>
            [XmlAttribute("collection"), DefaultValue(false)]
            public bool IsCollection { get; set; }

            /// <summary>
            /// The cargo types mask that this wagon can carry. Used for cargo-based trailer selection.
            /// </summary>
            [DefaultValue(CargoFlags.None)]
            public CargoFlags CargoType { get; set; }

            private VehicleInfo m_info;
            private List<VehicleInfo> m_infos;

            public Trailer()
            {
                InvertProbability = 0;
                Weight = 10;

                SubTrailers = new List<Trailer>();
            }

            public Trailer(VehicleInfo info) : this()
            {
                m_info = info;
                AssetName = info.name;
            }

            public Trailer(string name) : this()
            {
                AssetName = name;
            }

            public VehicleInfo GetInfo()
            {
                if(m_info == null)
                {
                    m_info = Util.FindVehicle(AssetName, "");
                    if(m_info == null)
                        Util.LogWarning(AssetName + " can not be found!");
                    AssetName = m_info != null ? m_info.name : AssetName;
                }
                return m_info;
            }

            public List<VehicleInfo> GetInfos()
            {
                if(m_infos == null)
                {
                    m_infos = new List<VehicleInfo>();
                    foreach(var trailer in SubTrailers)
                    {
                        var info = trailer.GetInfo();
                        if(info != null)
                        {
                            m_infos.Add(info);
                        }
                        else
                        {
                            m_infos = null;
                            return null;
                        }
                    }
                }
                return m_infos;
            }

            public bool IsMultiTrailer()
            {
                return SubTrailers.Count > 0;
            }

            public Trailer Copy()
            {
                var copy = new Trailer();

                copy.AssetName = AssetName;
                copy.InvertProbability = InvertProbability;
                copy.Weight = Weight;
                copy.IsCollection = IsCollection;
                copy.CargoType = CargoType;

                if(IsMultiTrailer())
                {
                    foreach(var trailer in SubTrailers)
                    {
                        copy.SubTrailers.Add(trailer.Copy());
                    }
                }

                return copy;
            }
        }
    }
}
