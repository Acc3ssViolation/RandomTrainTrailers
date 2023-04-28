using System.Collections;
using System.Collections.Generic;

namespace RandomTrainTrailers.Definition
{
    internal static class TrainPoolExtensions
    {
        public static void RemoveUnavailableLocomotives(this TrainPool pool, IDictionary<string, Locomotive> locomotives)
        {
            foreach (var locomotiveRef in pool.Locomotives)
                locomotiveRef.Resolve(locomotives);
            pool.Locomotives.RemoveAll(c => c.Reference == null);
        }

        public static void RemoveUnavailableTrailers(this TrainPool pool, IDictionary<string, Trailer> trailers)
        {
            foreach (var collectionRef in pool.Trailers)
                collectionRef.Resolve(trailers);
            pool.Trailers.RemoveAll(c => c.Reference == null);
        }

        public static bool IsValid(this TrainPool pool)
        {
            if (pool.MaxLocomotiveCount < pool.MinLocomotiveCount)
                return false;
            if (pool.MinLocomotiveCount < 1)
                return false;
            if (pool.MaxLocomotiveCount > 100)
                return false;

            if (pool.MaxTrainLength < pool.MinTrainLength)
                return false;
            if (pool.MinTrainLength < 1)
                return false;
            if (pool.MaxTrainLength > 100)
                return false;

            return true;
        }
    }
}
