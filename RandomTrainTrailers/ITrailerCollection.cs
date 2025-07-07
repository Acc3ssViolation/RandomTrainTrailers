using ColossalFramework.Math;
using RandomTrainTrailers.Definition;

namespace RandomTrainTrailers
{
    internal interface IRandomTrailerCollection
    {
        string Name { get; }
        CargoFlags AvailableCargoTypes { get; }
        Trailer GetTrailer(Randomizer randomizer);
        Trailer GetTrailerForCargo(int cargoIndex, Randomizer randomizer);
    }
}
