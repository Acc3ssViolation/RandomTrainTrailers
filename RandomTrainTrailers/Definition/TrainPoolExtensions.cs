using System.Collections;
using System.Collections.Generic;

namespace RandomTrainTrailers.Definition
{
    internal static class TrainPoolExtensions
    {
        public static void RemoveUnavailableAssets(this TrainPool pool)
        {
            pool.Locomotives.RemoveAll(l => l.VehicleInfo == null);
        }

        public static void RemoveUnavailableCollections(this TrainPool pool, IDictionary<string, TrailerCollection> collections)
        {
            foreach (var collectionRef in pool.TrailerCollections)
            {
                if (!collections.TryGetValue(collectionRef.Name, out var collection))
                    continue;
                collectionRef.FromCollection(collection);
            }
            pool.TrailerCollections.RemoveAll(c => c.TrailerCollection == null);
        }

        public static bool IsValid(this TrainPool pool)
        {
            if (pool.MaxLocomotiveCount < pool.MinLocomotiveCount)
                return false;
            if (pool.MinLocomotiveCount < 1)
                return false;
            if (pool.MaxLocomotiveCount > 100)
                return false;
            return true;
        }
    }
}
