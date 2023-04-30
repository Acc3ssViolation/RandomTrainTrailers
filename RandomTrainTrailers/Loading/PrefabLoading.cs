using System.Collections.Generic;

namespace RandomTrainTrailers.Loading
{
    internal interface IPrefabLoadingHook<T> where T : PrefabInfo
    {
        void BeforeRun();
        void OnPrefab(T prefab);
        void AfterRun();
    }

    internal class PrefabLoading<T> where T : PrefabInfo
    {
        private readonly List<IPrefabLoadingHook<T>> _hooks = new List<IPrefabLoadingHook<T>>();

        public void AddHook(IPrefabLoadingHook<T> hook)
        {
            _hooks.Add(hook);
        }

        public void Run()
        {
            foreach (var hook in _hooks)
                hook.BeforeRun();

            var count = PrefabCollection<T>.PrefabCount();
            for (var i = 0; i < count; i++)
            {
                var prefab = PrefabCollection<T>.GetPrefab((uint)i);
                if (prefab != null)
                {
                    foreach (var hook in _hooks)
                        hook.OnPrefab(prefab);
                }
            }

            foreach (var hook in _hooks)
                hook.AfterRun();
        }
    }
}
