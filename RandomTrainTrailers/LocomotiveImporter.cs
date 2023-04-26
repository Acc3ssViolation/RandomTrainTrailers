using RandomTrainTrailers.Definition;

namespace RandomTrainTrailers
{
    internal class LocomotiveImporter
    {
        private const float OverheadLineThreshold = 4.85f;

        public Locomotive ImportFromAsset(VehicleInfo vehicleInfo)
        {
            var locomotive = new Locomotive
            {
                AssetName = vehicleInfo.name,
                Length = 1,
                Type = LocomotiveType.Diesel,
            };

            if (vehicleInfo.m_mesh.bounds.size.y >= OverheadLineThreshold)
                locomotive.Type = LocomotiveType.ElectricOverhead;

            return locomotive;
        }
    }
}
