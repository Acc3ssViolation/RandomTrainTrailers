using ColossalFramework.Math;
using RandomTrainTrailers.Definition;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RandomTrainTrailers
{
    internal class WeightedTrailerDistribution : IRandomTrailerCollection
    {
        static readonly int CargoTypeCount = CargoParcel.ResourceTypes.Length;

        private Trailer[] _trailers;
        private List<Trailer>[] _trailersPerCargoType;

        public WeightedTrailerDistribution(IEnumerable<Trailer> trailers)
        {
            Initialize(trailers);
        }

        public string Name => throw new NotImplementedException();

        public Trailer GetTrailer(Randomizer randomizer)
        {
            return _trailers[randomizer.Int32((uint)_trailers.Length)];
        }

        public Trailer GetTrailerForCargo(int cargoIndex, Randomizer randomizer)
        {
            if (cargoIndex < 0 || cargoIndex >= _trailers.Length)
                return null;

            var list = _trailersPerCargoType[cargoIndex];
            if (list == null)
                return null;

            return list[randomizer.Int32((uint)list.Count)];
        }

        private void Initialize(IEnumerable<Trailer> trailers)
        {
            _trailersPerCargoType = new List<Trailer>[CargoTypeCount];
            _trailers = trailers.ToArray();

            foreach (var trailer in trailers)
            {
                for (var cargoIndex = 0; cargoIndex < CargoTypeCount; cargoIndex++)
                {
                    if (((int)trailer.CargoType & (1 << cargoIndex)) == 0)
                        continue;

                    var list = _trailersPerCargoType[cargoIndex];
                    if (list == null)
                    {
                        list = new List<Trailer>();
                        _trailersPerCargoType[cargoIndex] = list;
                    }
                    // TODO: Optimize this if possible to reduce memory usage
                    // Divide the weights by the Highest Common Divisor of all trailers of this type
                    for (var i = 0; i < trailer.Weight; i++)
                        list.Add(trailer);
                }
            }

            // Fix for distributions without any cargo settings
            if (_trailersPerCargoType.All(l => l == null))
            {
                // Workaround for ArrayTypeMismatchException on CS's Mono version, we can't just assign _trailers to an IList<>
                var trailerList = new List<Trailer>(_trailers);
                for (var cargoIndex = 0; cargoIndex < CargoTypeCount; cargoIndex++)
                    _trailersPerCargoType[cargoIndex] = trailerList;
            }
        }
    }
}
