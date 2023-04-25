using System.Xml.Serialization;

namespace RandomTrainTrailers.Definition
{
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
            if (m_info == null)
            {
                m_info = Util.FindVehicle(AssetName, "");
                if (m_info == null)
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
}
