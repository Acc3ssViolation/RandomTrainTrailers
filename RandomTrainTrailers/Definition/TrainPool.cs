using System.Collections.Generic;
using System.Xml.Serialization;

namespace RandomTrainTrailers.Definition
{
    public class TrainPool
    {
        public class CollectionReference
        {
            /// <summary>
            /// Name of the collection
            /// </summary>
            public string Name { get; set; }

            [XmlIgnore]
            public TrailerCollection TrailerCollection { get; private set; }

            public void FromCollection(TrailerCollection collection)
            {
                Name = collection.Name;
                TrailerCollection = collection;
            }

            public CollectionReference Copy()
            {
                var copy = new CollectionReference
                {
                    Name = Name,
                    TrailerCollection = TrailerCollection,
                };
                return copy;
            }
        }

        /// <summary>
        /// The name of this pool
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The locomotives in this pool
        /// </summary>
        public List<Locomotive> Locomotives { get; set; }

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

        public TrainPool()
        {
            Name = string.Empty;
            Locomotives = new List<Locomotive>();
            TrailerCollections = new List<CollectionReference>();
        }

        public TrainPool Copy()
        {
            var copy = new TrainPool
            {
                Name = Name,
                MaxLocomotiveCount = MaxLocomotiveCount,
                MinLocomotiveCount = MinLocomotiveCount,
            };
            foreach (var item in Locomotives)
            {
                copy.Locomotives.Add(item.Copy());
            }
            foreach (var item in TrailerCollections)
            {
                copy.TrailerCollections.Add(item);
            }
            return copy;
        }
    }
}
