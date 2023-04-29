using RandomTrainTrailers.Definition;
using System.Collections.Generic;
using static VehicleInfo;

namespace RandomTrainTrailers
{
    internal class TrailerImporter
    {
        private static readonly Dictionary<string, Trailer> Empty = new Dictionary<string, Trailer>();

        private IDictionary<string, Trailer> _trailers = Empty;

        public void SetTrailers(TrailerDefinition trailerDefinition)
        {
            if (_trailers == Empty)
                _trailers = new Dictionary<string, Trailer>();
            else
                _trailers.Clear();

            foreach (var collection in trailerDefinition.Collections)
            {
                foreach (var trailer in collection.Trailers)
                {
                    if (trailer.IsCollection)
                        continue;

                    if (_trailers.ContainsKey(trailer.AssetName))
                        Util.LogWarning($"Duplicate trailer definition '{trailer.AssetName}'");
                    _trailers[trailer.AssetName] = trailer;
                }
            }

            foreach (var vehicle in trailerDefinition.Vehicles)
            {
                foreach (var trailer in vehicle.Trailers)
                {
                    if (trailer.IsCollection)
                        continue;

                    if (_trailers.ContainsKey(trailer.AssetName))
                        Util.LogWarning($"Duplicate trailer definition '{trailer.AssetName}'");
                    _trailers[trailer.AssetName] = trailer;
                }
            }
        }

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
            if (_trailers.TryGetValue(vehicleInfo.name, out var trailerDef))
            {
                if (trailerDef.CargoType != CargoFlags.None)
                {
                    Util.Log($"Imported cargo type for '{vehicleInfo.name}' from existing trailer definition");
                    return trailerDef.CargoType;
                }
            }

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
