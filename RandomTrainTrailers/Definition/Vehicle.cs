using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace RandomTrainTrailers.Definition
{
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
        /// <remarks>
        /// This is always VehiclePrefabs.VehicleType.Unknown until that is somehow fixed
        /// </remarks>
        [XmlAttribute("type"), DefaultValue(VehiclePrefabs.VehicleType.Unknown)]
        public VehiclePrefabs.VehicleType VehicleType { get; set; }

        /// <summary>
        /// Chance of actually having random trailers assigned, percentage, defaults to 100.
        /// </summary>
        [XmlAttribute("chance"), DefaultValue(100)]
        public int RandomTrailerChance { get; set; } = 100;

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
                if (TrailerCountOverride == null)
                {
                    return -1;
                }
                return TrailerCountOverride.Min;
            }
            set
            {
                if (value >= 0)
                {
                    if (TrailerCountOverride == null)
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
                if (TrailerCountOverride == null)
                {
                    return -1;
                }
                return TrailerCountOverride.Max;
            }
            set
            {
                if (value >= 0)
                {
                    if (TrailerCountOverride == null)
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
            if (m_info == null)
            {
                m_info = Util.FindVehicle(AssetName, "");
                if (m_info == null)
                    Util.LogWarning(AssetName + " can not be found!");
                AssetName = m_info != null ? m_info.name : AssetName;
            }
            return m_info;
        }

        public Vehicle Copy()
        {
            var copy = new Vehicle();

            copy.AssetName = AssetName;
            foreach (var trailer in Trailers)
            {
                copy.Trailers.Add(trailer.Copy());
            }
            foreach (var blacklist in LocalBlacklist)
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
            if (m_trailerCollections.Count > 1)
            {
                if (m_collectionCDF == null)
                {
                    m_collectionCDF = new int[m_trailerCollections.Count];
                    for (int i = 0; i < m_trailerCollections.Count; i++)
                    {
                        m_collectionCDF[i] = m_trailerCollections[i].m_weight + (i > 0 ? m_collectionCDF[i - 1] : 0);
                    }
                }

                int colIndex = Array.BinarySearch(m_collectionCDF, Util.Random.Next(m_collectionCDF[m_collectionCDF.Length - 1] + 1));
                if (colIndex < 0)
                {
                    colIndex = ~colIndex;
                }
                if (colIndex < 0 || colIndex > m_trailerCollections.Count - 1)
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
}
