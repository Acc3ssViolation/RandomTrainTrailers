using ColossalFramework.Globalization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomTrainTrailers
{
    public static class VehiclePrefabs
    {
        public enum VehicleType
        {
            PassengerTrain, CargoTrain, Metro, Tram, Unknown
        }

        public static VehicleInfo[] passengerTrains { get; private set; }
        public static VehicleInfo[] cargoTrains { get; private set; }
        public static VehicleInfo[] metros { get; private set; }
        public static VehicleInfo[] trams { get; private set; }

        public static void FindPrefabs()
        {
            List<VehicleInfo> ptInfos = new List<VehicleInfo>();
            List<VehicleInfo> ctInfos = new List<VehicleInfo>();
            List<VehicleInfo> mInfos = new List<VehicleInfo>();
            List<VehicleInfo> tInfos = new List<VehicleInfo>();

            for(int i = 0; i < PrefabCollection<VehicleInfo>.PrefabCount(); i++)
            {
                var prefab = PrefabCollection<VehicleInfo>.GetPrefab((uint)i);
                if(prefab != null)
                {
                    string locale = Locale.GetUnchecked("VEHICLE_TITLE", prefab.name);
                    if(locale.StartsWith("VEHICLE_TITLE") || locale.StartsWith("Trailer") || locale.StartsWith("Wagon"))    // I tend to use Wagon prefixes for locally saved trailers
                    {
                        //Note: metro train is a subclass of passenger train, so make sure we check that first
                        var mtAI = prefab.m_vehicleAI as MetroTrainAI;
                        var ptAI = prefab.m_vehicleAI as PassengerTrainAI;
                        var ctAI = prefab.m_vehicleAI as CargoTrainAI;
                        var trAI = prefab.m_vehicleAI as TramAI;
                        if(mtAI != null)
                        {
                            Util.Log("Added " + prefab.name + " to list of metro assets");
                            mInfos.Add(prefab);
                        }
                        else if(ptAI != null)
                        {
                            Util.Log("Added " + prefab.name + " to list of passenger train assets");
                            ptInfos.Add(prefab);
                        }
                        else if(ctAI != null)
                        {
                            Util.Log("Added " + prefab.name + " to list of cargo train assets");
                            ctInfos.Add(prefab);
                        }
                        else if(trAI != null)
                        {
                            Util.Log("Added " + prefab.name + " to list of tram assets");
                            tInfos.Add(prefab);
                        }
                    }
                }
            }

            passengerTrains = ptInfos.ToArray();
            cargoTrains = ctInfos.ToArray();
            metros = mInfos.ToArray();
            trams = tInfos.ToArray();
        }

        public static VehicleInfo[] GetPrefabs(VehicleType type)
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
                    return new VehicleInfo[0];
            }
        }
    }
}
