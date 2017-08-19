using ColossalFramework.Globalization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomTrainTrailers
{
    public static class VehiclePrefabs
    {
        public class VehicleData
        {
            public string localeName;
            public VehicleInfo info;
            public bool isTrailer;
        }

        public enum VehicleType
        {
            PassengerTrain, CargoTrain, Metro, Tram, Unknown
        }

        public static VehicleData[] passengerTrains { get; private set; }
        public static VehicleData[] cargoTrains { get; private set; }
        public static VehicleData[] metros { get; private set; }
        public static VehicleData[] trams { get; private set; }

        public static void FindPrefabs()
        {
            var ptInfos = new List<VehicleData>();
            var ctInfos = new List<VehicleData>();
            var mInfos = new List<VehicleData>();
            var tInfos = new List<VehicleData>();

            for(int i = 0; i < PrefabCollection<VehicleInfo>.PrefabCount(); i++)
            {
                var prefab = PrefabCollection<VehicleInfo>.GetPrefab((uint)i);
                if(prefab != null)
                {
                    //Note: metro train is a subclass of passenger train, so make sure we check that first
                    var mtAI = prefab.m_vehicleAI as MetroTrainAI;
                    var ptAI = prefab.m_vehicleAI as PassengerTrainAI;
                    var ctAI = prefab.m_vehicleAI as CargoTrainAI;
                    var trAI = prefab.m_vehicleAI as TramAI;

                    VehicleData data = new VehicleData();
                    data.info = prefab;
                    data.localeName = Util.GetVehicleDisplayName(prefab.name);

                    string locale = Locale.GetUnchecked("VEHICLE_TITLE", prefab.name);
                    if(locale.StartsWith("VEHICLE_TITLE") || locale.StartsWith("Trailer") || locale.StartsWith("Wagon") || prefab.m_placementStyle == ItemClass.Placement.Procedural)
                    {
                        data.isTrailer = true;
                    }
                    else
                    {
                        data.isTrailer = false;
                    }

                    if(mtAI != null)
                    {
                        Util.Log("Added " + prefab.name + " to list of metro assets");
                        mInfos.Add(data);
                    }
                    else if(ptAI != null)
                    {
                        Util.Log("Added " + prefab.name + " to list of passenger train assets");
                        ptInfos.Add(data);
                    }
                    else if(ctAI != null)
                    {
                        Util.Log("Added " + prefab.name + " to list of cargo train assets");
                        ctInfos.Add(data);
                    }
                    else if(trAI != null)
                    {
                        Util.Log("Added " + prefab.name + " to list of tram assets");
                        tInfos.Add(data);
                    }
                }
            }

            passengerTrains = ptInfos.ToArray();
            cargoTrains = ctInfos.ToArray();
            metros = mInfos.ToArray();
            trams = tInfos.ToArray();
        }

        public static VehicleData[] GetPrefabs(VehicleType type)
        {
            switch(type)
            {
                case VehicleType.PassengerTrain:
                    return passengerTrains;
                case VehicleType.CargoTrain:
                    return cargoTrains;
                case VehicleType.Metro:
                    return metros;
                case VehicleType.Tram:
                    return trams;
                default:
                    return new VehicleData[0];
            }
        }
    }
}
