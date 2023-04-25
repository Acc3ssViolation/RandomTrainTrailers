using System.Collections.Generic;

namespace RandomTrainTrailers.Definition
{
    /// <summary>
    /// Contains vehicle trailer random selection data.
    /// </summary>
    public class TrailerDefinition
    {
        /// <summary>
        /// List of vehicles.
        /// </summary>
        public List<Vehicle> Vehicles { get; set; }

        /// <summary>
        /// List of locomotive pools.
        /// </summary>
        public List<TrainPool> TrainPools { get; set; }

        /// <summary>
        /// List of trailer collections. Can be shared among vehicles.
        /// </summary>
        public List<TrailerCollection> Collections { get; set; }

        public TrailerDefinition()
        {
            Vehicles = new List<Vehicle>();
            TrainPools = new List<TrainPool>();
            Collections = new List<TrailerCollection>();
        }

        /// <summary>
        /// Returns a deep copy of this TrailerDefinition.
        /// </summary>
        public TrailerDefinition Copy()
        {
            var copy = new TrailerDefinition();

            foreach (var pool in TrainPools)
            {
                copy.TrainPools.Add(pool.Copy());
            }

            foreach(var item in Vehicles)
            {
                copy.Vehicles.Add(item.Copy());
            }

            foreach(var item in Collections)
            {
                copy.Collections.Add(item.Copy());
            }

            return copy;
        }
    }
}
