using System.Collections.Generic;
using System.Xml.Serialization;

namespace RandomTrainTrailers.Definition
{
    /// <summary>
    /// Contains vehicle trailer random selection data.
    /// </summary>
    public class TrailerDefinition
    {
        /// <summary>
        /// List of train pools.
        /// </summary>
        public List<TrainPool> TrainPools { get; set; }

        /// <summary>
        /// List of all locomotives. Can be shared among pools.
        /// </summary>
        public List<Locomotive> Locomotives { get; set; }

        /// <summary>
        /// List of all trailers. Can be shared among pools.
        /// </summary>
        public List<Trailer> Trailers { get; set; }

        /// <summary>
        /// List of vehicles.
        /// </summary>
        public List<Vehicle> Vehicles { get; set; }

        /// <summary>
        /// List of trailer collections. Can be shared among vehicles.
        /// </summary>
        public List<TrailerCollection> Collections { get; set; }

        /// <summary>
        /// Name used at runtime to trace where this definition came from.
        /// </summary>
        [XmlIgnore]
        public string Name { get; set; }

        public TrailerDefinition()
        {
            Vehicles = new List<Vehicle>();
            Collections = new List<TrailerCollection>();

            TrainPools = new List<TrainPool>();
            Locomotives = new List<Locomotive>();
            Trailers = new List<Trailer>();
        }

        /// <summary>
        /// Returns a deep copy of this TrailerDefinition.
        /// </summary>
        public TrailerDefinition Copy()
        {
            var copy = new TrailerDefinition();

            foreach (var item in Vehicles)
            {
                copy.Vehicles.Add(item.Copy());
            }

            foreach (var item in Collections)
            {
                copy.Collections.Add(item.Copy());
            }

            foreach (var pool in TrainPools)
            {
                copy.TrainPools.Add(pool.Copy());
            }

            foreach (var item in Locomotives)
            {
                copy.Locomotives.Add(item.Copy());
            }

            foreach (var item in Trailers)
            {
                copy.Trailers.Add(item.Copy());
            }

            return copy;
        }
    }
}
