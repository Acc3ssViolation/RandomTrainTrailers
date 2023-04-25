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
        /// List of globally blacklisted trailers.
        /// </summary>
        //public List<BlacklistItem> Blacklist { get; set; }

        /// <summary>
        /// List of trailer collections. Can be shared among vehicles.
        /// </summary>
        public List<TrailerCollection> Collections { get; set; }

        public TrailerDefinition()
        {
            Vehicles = new List<Vehicle>();
            //Blacklist = new List<BlacklistItem>();
            Collections = new List<TrailerCollection>();
        }

        /// <summary>
        /// Returns a deep copy of this TrailerDefinition.
        /// </summary>
        public TrailerDefinition Copy()
        {
            var copy = new TrailerDefinition();

            //foreach(var item in Blacklist)
            //{
            //    copy.Blacklist.Add(item.Copy());
            //}

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
