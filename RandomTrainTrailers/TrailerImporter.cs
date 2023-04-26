using RandomTrainTrailers.Definition;
using static VehicleInfo;

namespace RandomTrainTrailers
{
    internal class TrailerImporter
    {
        public Trailer ImportFromAsset(ref VehicleTrailer trailer)
            => ImportFromAsset(trailer.m_info, trailer.m_invertProbability);

        public Trailer ImportFromAsset(VehicleInfo vehicleInfo, int invertProbability = 0)
        {
            var trailer = new Trailer
            {
                AssetName = vehicleInfo.name,
                InvertProbability = invertProbability,
                IsCollection = false,
                CargoType = GuessCargoType(vehicleInfo),
            };

            return trailer;
        }

        private CargoFlags GuessCargoType(VehicleInfo vehicleInfo)
        {
            if (vehicleInfo.m_subMeshes == null)
                return CargoFlags.None;

            var flags = CargoFlags.None;
            for (var i = 0; i < vehicleInfo.m_subMeshes.Length; i++)
            {
                var info = vehicleInfo.m_subMeshes[i];
                flags |= GetFlagsForVariationMask((VariationMask)info.m_variationMask);
            }

            return flags;
        }

        private CargoFlags GetFlagsForVariationMask(VariationMask variationMask)
        {
            var flags = CargoFlags.None;
            if ((variationMask & (VariationMask.Goods | VariationMask.GoodsEmpty)) != VariationMask.None)
                flags |= CargoFlags.Goods;
            if ((variationMask & VariationMask.Grain) != VariationMask.None)
                flags |= CargoFlags.Grain;
            if ((variationMask & VariationMask.AnimalProducts) != VariationMask.None)
                flags |= CargoFlags.AnimalProducts;
            if ((variationMask & (VariationMask.Logs | VariationMask.LogsEmpty)) != VariationMask.None)
                flags |= CargoFlags.Logs;
            if ((variationMask & VariationMask.OilProducts) != VariationMask.None)
                flags |= CargoFlags.Oil;
            if ((variationMask & (VariationMask.Ore | VariationMask.OreEmpty)) != VariationMask.None)
                flags |= CargoFlags.Ore;
            return flags;
        }
    }
}
