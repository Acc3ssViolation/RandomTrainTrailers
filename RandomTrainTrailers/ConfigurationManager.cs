using ColossalFramework;
using RandomTrainTrailers.Definition;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RandomTrainTrailers
{
    internal class ConfigurationManager : Singleton<ConfigurationManager>
    {
        private readonly IList<TrailerDefinition> _loadedDefinitions = new List<TrailerDefinition>();
        private bool _invalidated = true;
        private TrailerDefinition _combinedDefinition;

        public const string User = "User";
        public const string Default = "Default";

        public event Action EventInvalidated;

        public TrailerDefinition GetCombinedDefinition()
        {
            if (_invalidated)
            {
                if (_combinedDefinition == null)
                {
                    _combinedDefinition = new TrailerDefinition
                    {
                        Name = "Merged"
                    };
                }

                _combinedDefinition.Vehicles.Clear();
                _combinedDefinition.Collections.Clear();
                _combinedDefinition.Locomotives.Clear();
                _combinedDefinition.TrainPools.Clear();
                _combinedDefinition.Trailers.Clear();

                foreach (var definition in _loadedDefinitions)
                    MergeInto(_combinedDefinition, definition);
            }

            return _combinedDefinition;
        }

        public IList<TrailerDefinition> GetDefinitions()
        {
            // Always return a copy of the list so it can't be modified
            return _loadedDefinitions.ToList();
        }

        public TrailerDefinition GetDefinition(string name)
        {
            return _loadedDefinitions.FirstOrDefault(d => d.Name == name);
        }

        public void Add(string name, TrailerDefinition definition)
        {
            definition.Name = name;
            _loadedDefinitions.Add(definition);
            Invalidate();
        }

        public void Reset()
        {
            _loadedDefinitions.Clear();
            Invalidate();
        }

        public void Invalidate()
        {
            _invalidated = true;
            EventInvalidated?.Invoke();
        }

        private void MergeInto(TrailerDefinition to, TrailerDefinition from)
        {
            MergeInto(to.Locomotives, from.Locomotives, (a, b) => a.AssetName == b.AssetName);
            MergeInto(to.Trailers, from.Trailers, (a, b) => a.AssetName == b.AssetName);
            MergeInto(to.TrainPools, from.TrainPools);
            MergeInto(to.Collections, from.Collections);
            MergeInto(to.Vehicles, from.Vehicles, (a, b) => a.AssetName == b.AssetName);
        }

        private void MergeInto<T>(List<T> to, List<T> from, Func<T, T, bool> equals)
        {
            foreach (var item in from)
            {
                var toItem = to.FindIndex(i => equals(i, item));
                if (toItem < 0)
                    to.Add(item);
                else
                    to[toItem] = item;
            }
        }

        private void MergeInto(List<TrailerCollection> to, List<TrailerCollection> from)
        {
            foreach (var item in from)
            {
                var toItem = to.FindIndex(i => i.Name == item.Name);
                if (toItem < 0)
                {
                    to.Add(item);
                }
                else
                {
                    MergeInto(to[toItem].Trailers, item.Trailers, (a, b) => a.AssetName == b.AssetName);
                }
            }
        }

        private void MergeInto(List<TrainPool> to, List<TrainPool> from)
        {
            foreach (var item in from)
            {
                var toItem = to.FindIndex(i => i.Name == item.Name);
                if (toItem < 0)
                {
                    to.Add(item);
                }
                else
                {
                    MergeInto(to[toItem].Locomotives, item.Locomotives, (a, b) => a.Name == b.Name);
                    MergeInto(to[toItem].Trailers, item.Trailers, (a, b) => a.Name == b.Name);
                    to[toItem].UseCargo = item.UseCargo;
                    to[toItem].Enabled = item.Enabled;
                    to[toItem].MinLocomotiveCount = item.MinLocomotiveCount;
                    to[toItem].MaxLocomotiveCount = item.MaxLocomotiveCount;
                    to[toItem].MinTrainLength = item.MinTrainLength;
                    to[toItem].MaxTrainLength = item.MaxTrainLength;
                }
            }
        }
    }
}
