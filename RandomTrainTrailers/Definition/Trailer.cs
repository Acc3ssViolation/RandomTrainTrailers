using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace RandomTrainTrailers.Definition
{
    public class Trailer : IEnableable
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
        public int InvertProbability;

        /// <summary>
        /// Weight for the random selection, defaults to 10. (int)
        /// </summary>
        [XmlAttribute("weight"), DefaultValue(10)]
        public int Weight = 10;

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
        public CargoFlags CargoType { get; set; } = CargoFlags.None;

        /// <summary>
        /// If this trailer is enabled an can be used.
        /// </summary>
        [XmlAttribute("enabled")]
        public bool Enabled { get; set; } = true;

        [XmlIgnore]
        public bool IsMultiTrailer => SubTrailers.Count > 0;

        /// <summary>
        /// A list of vehicle infos for this trailer.
        /// Will be null if the info cannot be found or if this is a multi-trailer and any of the sub trailers cannot find their info.
        /// </summary>
        [XmlIgnore]
        public IList<VehicleInfo> VehicleInfos
        {
            get
            {
                if (m_infos == null)
                {
                    m_infos = new List<VehicleInfo>();
                    if (IsMultiTrailer)
                    {
                        foreach (var trailer in SubTrailers)
                        {
                            var info = trailer.VehicleInfos?[0];
                            if (info != null)
                            {
                                m_infos.Add(info);
                            }
                            else
                            {
                                m_infos = null;
                                break;
                            }
                        }
                    }
                    else
                    {
                        var info = Util.FindVehicle(AssetName, string.Empty);
                        if (info != null)
                            m_infos.Add(info);
                        else
                            m_infos = null;
                    }
                }
                return m_infos;
            }
        }

        private List<VehicleInfo> m_infos;

        public Trailer()
        {
            SubTrailers = new List<Trailer>();
        }

        public Trailer(VehicleInfo info) : this()
        {
            m_infos = new List<VehicleInfo> { info };
            AssetName = info.name;
        }

        public Trailer(string name) : this()
        {
            AssetName = name;
        }

        public Trailer Copy()
        {
            var copy = new Trailer
            {
                AssetName = AssetName,
                InvertProbability = InvertProbability,
                Weight = Weight,
                IsCollection = IsCollection,
                CargoType = CargoType,
                Enabled = Enabled
            };

            if (IsMultiTrailer)
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
