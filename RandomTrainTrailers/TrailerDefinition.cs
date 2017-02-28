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
            /// <summary>
            /// Name of the collection used for identification.
            /// </summary>
            [XmlAttribute("name")]
            public string Name { get; set; }

            /// <summary>
            /// The trailers of this collection.
            /// </summary>
            public List<Trailer> Trailers { get; set; }

            public TrailerCollection() : this("New Collection")
            {
            }

            public TrailerCollection(string name)
            {
                Name = name;
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

            /// <summary>
            /// Trailers the picker may use for this train.
            /// </summary>
            public List<Trailer> Trailers { get; set; }

            /// <summary>
            /// Trailers in this local blacklist will never be allowed to spawn on this train, even if they are included in the Trailers list.
            /// </summary>
            public List<BlacklistItem> LocalBlacklist { get; set; }

            private VehicleInfo m_info;

            [XmlIgnore]
            public List<Collection> m_trailerCollections;

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

                return copy;
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
                        Util.LogError(AssetName + " can not be found!");
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
                            m_infos.Add(info);
                        else
                            return null;
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
