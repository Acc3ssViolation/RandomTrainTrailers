using System.Collections.Generic;

namespace RandomTrainTrailers.Definition
{
    internal static class TrailerExtensions
    {
        public static IList<VehicleRenderInfo> GetVehicleRenderInfos(this Trailer trailer)
        {
            var infos = trailer.VehicleInfos;
            if (infos == null)
                return null;
            if (!trailer.IsMultiTrailer)
                return new List<VehicleRenderInfo>() { new VehicleRenderInfo(trailer.VehicleInfos[0], trailer.InvertProbability >= 50) };

            var result = new List<VehicleRenderInfo>(infos.Count);
            foreach (var subTrailer in trailer.SubTrailers)
                result.Add(new VehicleRenderInfo(subTrailer.VehicleInfos[0], subTrailer.InvertProbability >= 50));
            return result;
        }
    }
}
