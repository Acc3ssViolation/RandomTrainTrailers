using ColossalFramework.Math;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace RandomTrainTrailers.Definition
{
    /// <summary>
    /// Represents a collection of trailers.
    /// </summary>
    public class TrailerCollection : IRandomTrailerCollection
    {
        /// <summary>
        /// Name of the collection used for identification.
        /// </summary>
        [XmlAttribute("name")]
        public string Name { get; set; }

        /// <summary>
        /// The collection that this collection extends.
        /// </summary>
        [XmlAttribute("base"), DefaultValue(null)]
        public string BaseCollection { get; set; }

        /// <summary>
        /// The trailers of this collection.
        /// </summary>
        public List<Trailer> Trailers { get; set; }

        [XmlIgnore]
        private WeightedTrailerDistribution _trailerDistribution;

        public TrailerCollection() : this("New Collection")
        {
        }

        public TrailerCollection(string name)
        {
            Name = name;
            BaseCollection = null;
            Trailers = new List<Trailer>();
        }

        /// <summary>
        /// Returns a deep copy of this trailer collection.
        /// </summary>
        public TrailerCollection Copy()
        {
            var copy = new TrailerCollection(Name);

            foreach (var trailer in Trailers)
            {
                copy.Trailers.Add(trailer.Copy());
            }

            return copy;
        }

        public void BuildDistribution(bool force = false)
        {
            if (!force && _trailerDistribution != null)
                return;

            _trailerDistribution = new WeightedTrailerDistribution(Trailers);
        }

        public Trailer GetTrailer(Randomizer randomizer)
        {
            return _trailerDistribution?.GetTrailer(randomizer);
        }

        public Trailer GetTrailerForCargo(int cargoIndex, Randomizer randomizer)
        {
            return _trailerDistribution?.GetTrailerForCargo(cargoIndex, randomizer);
        }
    }
}
