using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace RandomTrainTrailers.Definition
{
    public abstract class ItemReference
    {
        public string Name { get; set; }
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

        public void Resolve(IDictionary<string, T> items)
        {
            items.TryGetValue(Name, out var reference);
            Reference = reference;
        }
    }

    public class TrainPool
    {
        public class CollectionReference : ItemReference<CollectionReference, TrailerCollection>
        {
        }

        public class LocomotiveReference : ItemReference<LocomotiveReference, Locomotive>
        {
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
        /// The names of trailer collections to use for this pool
        /// </summary>
        public List<CollectionReference> TrailerCollections { get; set; }

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
            TrailerCollections = new List<CollectionReference>();
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
            foreach (var item in TrailerCollections)
            {
                copy.TrailerCollections.Add(item);
            }
            return copy;
        }
    }
}
