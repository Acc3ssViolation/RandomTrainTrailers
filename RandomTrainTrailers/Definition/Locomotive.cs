﻿using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace RandomTrainTrailers.Definition
{
    public class Locomotive
    {
        /// <summary>
        /// Name of the train asset.
        /// </summary>
        [XmlAttribute("asset")]
        public string AssetName { get; set; }

        /// <summary>
        /// The type of locomotive this is. May be used to select which locomotives to use.
        /// </summary>
        [XmlAttribute("type"), DefaultValue(LocomotiveType.Unknown)]
        public LocomotiveType Type { get; set; }

        [XmlIgnore]
        public bool CanBeLeadVehicle => _info != null ? _info.m_placementStyle == ItemClass.Placement.Automatic : false;

        [XmlIgnore]
        public VehicleInfo VehicleInfo
        {
            get
            {
                if (_info == null)
                {
                    _info = Util.FindVehicle(AssetName, "");
                    if (_info == null)
                        Util.LogWarning(AssetName + " can not be found!");
                    AssetName = _info != null ? _info.name : AssetName;
                }
                return _info;
            }
        }

        private VehicleInfo _info = null;

        public Locomotive()
        {
            AssetName = string.Empty;
            Type = LocomotiveType.Unknown;
        }

        public Locomotive Copy()
        {
            var copy = new Locomotive
            {
                AssetName = AssetName,
                Type = Type
            };
            return copy;
        }
    }

    public enum LocomotiveType
    {
        Unknown,
        Diesel,
        Steam,
        ElectricOverhead,
        ElectricThirdRail,
    }
}
