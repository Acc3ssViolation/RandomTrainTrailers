using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace RandomTrainTrailers.Definition
{
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
        public int Weight { get; set; } = 10;

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
