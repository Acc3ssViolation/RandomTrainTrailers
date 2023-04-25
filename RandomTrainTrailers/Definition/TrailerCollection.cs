using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace RandomTrainTrailers.Definition
{
    /// <summary>
    /// Represents a collection of trailers.
    /// </summary>
    public class TrailerCollection
    {
        public class CargoData
        {
            public int[][] m_trailerCDF;
            public Trailer[][] m_trailers;

            public CargoData()
            {
                m_trailers = new Trailer[CargoParcel.ResourceTypes.Length][];
            }

            /// <summary>
            /// Returns a random, weighed trailer index for the given cargo type. -1 on failure.
            /// </summary>
            /// <param name="cargoIndex"></param>
            /// <returns></returns>
            public int GetRandomTrailerIndex(int cargoIndex)
            {
                // Compile CDF array for weighted random selection
                UpdateCDF();

                // -1 if we have no trailers
                if (m_trailerCDF[cargoIndex].Length == 0)
                {
                    return -1;
                }

                // Select random trailer index using the cdf array
                int randomTrailerIndex = Array.BinarySearch(m_trailerCDF[cargoIndex], Util.Random.Next(m_trailerCDF[cargoIndex][m_trailerCDF[cargoIndex].Length - 1] + 1));
                if (randomTrailerIndex < 0)
                {
                    randomTrailerIndex = ~randomTrailerIndex;
                }
                if (randomTrailerIndex < 0 || randomTrailerIndex > m_trailers[cargoIndex].Length - 1)
                {
                    Util.LogError("Index out of bounds! " + randomTrailerIndex);
                }

                return randomTrailerIndex;
            }

            public void UpdateCDF(bool force = false)
            {
                if (m_trailerCDF == null)
                {
                    m_trailerCDF = new int[CargoParcel.ResourceTypes.Length][];
                }

                if (m_trailerCDF[0] == null || force)
                {
                    for (int cargoIndex = 0; cargoIndex < CargoParcel.ResourceTypes.Length; cargoIndex++)
                    {
                        m_trailerCDF[cargoIndex] = new int[m_trailers[cargoIndex].Length];
                        for (int i = 0; i < m_trailers[cargoIndex].Length; i++)
                        {
                            m_trailerCDF[cargoIndex][i] = m_trailers[cargoIndex][i].Weight + (i > 0 ? m_trailerCDF[cargoIndex][i - 1] : 0);
                        }
                    }
                }
            }
        }

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
        private int[] m_trailerCDF;

        [XmlIgnore]
        public CargoData m_cargoData;

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

        public Trailer GetRandomTrailer()
        {
            return Trailers[GetRandomTrailerIndex()];
        }

        public int GetRandomTrailerIndex()
        {
            if (m_trailerCDF == null)
            {
                // Compile CDF array for weighted random selection
                m_trailerCDF = new int[Trailers.Count];
                for (int i = 0; i < Trailers.Count; i++)
                {
                    m_trailerCDF[i] = Trailers[i].Weight + (i > 0 ? m_trailerCDF[i - 1] : 0);
                }
            }

            // Select random trailer index using the cdf array
            int randomTrailerIndex = Array.BinarySearch(m_trailerCDF, Util.Random.Next(m_trailerCDF[m_trailerCDF.Length - 1] + 1));
            if (randomTrailerIndex < 0)
            {
                randomTrailerIndex = ~randomTrailerIndex;
            }
            if (randomTrailerIndex < 0 || randomTrailerIndex > Trailers.Count - 1)
            {
                Util.LogError("Index out of bounds! " + randomTrailerIndex);
            }

            return randomTrailerIndex;
        }
    }
}
