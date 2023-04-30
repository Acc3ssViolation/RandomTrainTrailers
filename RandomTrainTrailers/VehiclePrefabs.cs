using ColossalFramework.Globalization;
using RandomTrainTrailers.Loading;
using System.Collections.Generic;

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

        public static VehicleData[] PassengerTrains { get; private set; }
        public static VehicleData[] CargoTrains { get; private set; }
        public static VehicleData[] Metros { get; private set; }
        public static VehicleData[] Trams { get; private set; }

        public class VehiclePrefabHook : IPrefabLoadingHook<VehicleInfo>
        {
            private List<VehicleData> ptInfos;
            private List<VehicleData> ctInfos;
            private List<VehicleData> mInfos;
            private List<VehicleData> tInfos;

            public void BeforeRun()
            {
                ptInfos = new List<VehicleData>();
                ctInfos = new List<VehicleData>();
                mInfos = new List<VehicleData>();
                tInfos = new List<VehicleData>();
            }

            public void OnPrefab(VehicleInfo prefab)
            {
                //Note: metro train is a subclass of passenger train, so make sure we check that first
                var mtAI = prefab.m_vehicleAI as MetroTrainAI;
                var ptAI = prefab.m_vehicleAI as PassengerTrainAI;
                var ctAI = prefab.m_vehicleAI as CargoTrainAI;
                var trAI = prefab.m_vehicleAI as TramAI;

                VehicleData data = new VehicleData
                {
                    info = prefab,
                    localeName = Util.GetVehicleDisplayName(prefab.name)
                };

                string locale = Locale.GetUnchecked("VEHICLE_TITLE", prefab.name);
                if (locale.StartsWith("VEHICLE_TITLE") || locale.StartsWith("Trailer") || locale.StartsWith("Wagon") || prefab.m_placementStyle == ItemClass.Placement.Procedural)
                {
                    data.isTrailer = true;
                }
                else
                {
                    data.isTrailer = false;
                }

                if (mtAI != null)
                {
                    Util.Log("Added " + prefab.name + " to list of metro assets");
                    mInfos.Add(data);
                }
                else if (ptAI != null)
                {
                    Util.Log("Added " + prefab.name + " to list of passenger train assets");
                    ptInfos.Add(data);
                }
                else if (ctAI != null)
                {
                    Util.Log("Added " + prefab.name + " to list of cargo train assets");
                    ctInfos.Add(data);
                }
                else if (trAI != null)
                {
                    Util.Log("Added " + prefab.name + " to list of tram assets");
                    tInfos.Add(data);
                }
            }

            public void AfterRun()
            {
                PassengerTrains = ptInfos.ToArray();
                CargoTrains = ctInfos.ToArray();
                Metros = mInfos.ToArray();
                Trams = tInfos.ToArray();
            }
        }

        public static VehicleData[] GetPrefabs(VehicleType type)
        {
            switch(type)
            {
                case VehicleType.PassengerTrain:
                    return PassengerTrains;
                case VehicleType.CargoTrain:
                    return CargoTrains;
                case VehicleType.Metro:
                    return Metros;
                case VehicleType.Tram:
                    return Trams;
                default:
                    return new VehicleData[0];
            }
        }
    }
}
