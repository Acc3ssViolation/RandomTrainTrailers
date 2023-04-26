using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace RandomTrainTrailers.Definition
{
    public abstract class ItemReference<S, T> where S : ItemReference<S, T>, new()
    {
        public string Name { get; set; }

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

        public void Resolve(IReadOnlyDictionary<string, T> items)
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
        public int MinLocomotiveCount { get; set; } = 1;

        /// <summary>
        /// Maximum amount of locomotives for trains from this pool
        /// </summary>
        [XmlAttribute("maxLocoCount")]
        public int MaxLocomotiveCount { get; set; } = 1;

        /// <summary>
        /// Minimum total train length for trains from this pool
        /// </summary>
        [XmlAttribute("minLength")]
        public int MinTrainLength { get; set; } = 1;

        /// <summary>
        /// Maximum total train length for trains from this pool
        /// </summary>
        [XmlAttribute("maxLength")]
        public int MaxTrainLength { get; set; } = 12;

        /// <summary>
        /// Use cargo contents for trailer selection
        /// </summary>
        [XmlAttribute("useCargo"), DefaultValue(true)]
        public bool UseCargo { get; set; } = true;

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
