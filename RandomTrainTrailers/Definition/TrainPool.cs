using ColossalFramework.Math;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace RandomTrainTrailers.Definition
{
    public abstract class ItemReference
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlIgnore]
        public abstract string DisplayName { get; }

        [XmlIgnore]
        public abstract bool IsAvailable { get; }
    }

    public abstract class ItemReference<S, T>: ItemReference where S : ItemReference<S, T>, new()
    {
        [XmlIgnore]
        public T Reference { get; private set; }

        public ItemReference<S, T> Copy()
        {
            var copy = new S()
            {
                Name = Name,
                Reference = Reference,
            };
            return copy;
        }

        public ItemReference()
        {
        }

        public ItemReference(string name, T reference)
        {
            Name = name;
            Reference = reference;
        }

        public void Resolve(IDictionary<string, T> items)
        {
            items.TryGetValue(Name, out var reference);
            Reference = reference;
        }

        public override bool IsAvailable => Reference != null;
    }

    public class TrainPool : IEnableable, IRandomTrailerCollection
    {
        public class TrailerReference : ItemReference<TrailerReference, Trailer>
        {
            public override string DisplayName => Util.GetVehicleDisplayName(Name);
            public override bool IsAvailable => Reference?.VehicleInfos != null;

            public TrailerReference()
            {
            }

            public TrailerReference(Trailer trailer) : base(trailer.AssetName, trailer)
            { 
            }
        }

        public class LocomotiveReference : ItemReference<LocomotiveReference, Locomotive>
        {
            public override string DisplayName => Util.GetVehicleDisplayName(Name);
            public override bool IsAvailable => Reference?.VehicleInfo != null;

            public LocomotiveReference()
            {
            }

            public LocomotiveReference(Locomotive locomotive) : base(locomotive.AssetName, locomotive)
            {
            }
        }

        /// <summary>
        /// The name of this pool
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The locomotives in this pool
        /// </summary>
        public List<LocomotiveReference> Locomotives { get; set; }

        /// <summary>
        /// The names of trailers to use for this pool
        /// </summary>
        public List<TrailerReference> Trailers { get; set; }

        /// <summary>
        /// Minimum amount of locomotives for trains from this pool
        /// </summary>
        [XmlAttribute("minLocoCount")]
        public int MinLocomotiveCount = 1;

        /// <summary>
        /// Maximum amount of locomotives for trains from this pool
        /// </summary>
        [XmlAttribute("maxLocoCount")]
        public int MaxLocomotiveCount = 1;

        /// <summary>
        /// Minimum total train length for trains from this pool
        /// </summary>
        [XmlAttribute("minLength")]
        public int MinTrainLength = 12;

        /// <summary>
        /// Maximum total train length for trains from this pool
        /// </summary>
        [XmlAttribute("maxLength")]
        public int MaxTrainLength = 12;

        /// <summary>
        /// Use cargo contents for trailer selection
        /// </summary>
        [XmlAttribute("useCargo")]
        public bool UseCargo { get; set; } = true;

        /// <summary>
        /// Use this pool for train generation
        /// </summary>
        [XmlAttribute("enabled")]
        public bool Enabled { get; set; } = true;

        public TrainPool()
        {
            Name = string.Empty;
            Locomotives = new List<LocomotiveReference>();
            Trailers = new List<TrailerReference>();
        }

        public TrainPool Copy()
        {
            var copy = new TrainPool
            {
                Name = Name,
                MaxLocomotiveCount = MaxLocomotiveCount,
                MinLocomotiveCount = MinLocomotiveCount,
                MaxTrainLength = MaxTrainLength,
                MinTrainLength = MinTrainLength,
                Enabled = Enabled,
                UseCargo = UseCargo,
            };
            foreach (var item in Locomotives)
            {
                copy.Locomotives.Add(item);
            }
            foreach (var item in Trailers)
            {
                copy.Trailers.Add(item);
            }
            return copy;
        }

        public Trailer GetTrailer(Randomizer randomizer)
        {
            return Trailers[randomizer.Int32((uint)Trailers.Count)].Reference;
        }

        public Trailer GetTrailerForCargo(int cargoIndex, Randomizer randomizer)
        {
            // TODO: Implement properly
            return GetTrailer(randomizer);
        }
    }
}
