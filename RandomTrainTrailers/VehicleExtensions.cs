using ColossalFramework;
using System;

namespace RandomTrainTrailers
{

    public static class VehicleExtensions
    {
        /// <summary>
        /// Returns the amount of trailing vehicles
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="vehicleID"></param>
        /// <returns></returns>
        public static int GetTrailerCount(this Vehicle _this, ushort vehicleID)
        {
            if(_this.m_trailingVehicle == 0)
            {
                return 0;
            }
            VehicleManager instance = Singleton<VehicleManager>.instance;
            ushort trailingVehicle = _this.m_trailingVehicle;
            int num = 0;
            while(trailingVehicle != 0)
            {
                trailingVehicle = instance.m_vehicles.m_buffer[trailingVehicle].m_trailingVehicle;
                num++;
                if(num > 16384)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
            return num;
        }
    }
}
