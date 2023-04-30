namespace RandomTrainTrailers.Loading
{
    internal class EffectPatchHook : IPrefabLoadingHook<VehicleInfo>
    {
        public void BeforeRun()
        {
        }

        public void OnPrefab(VehicleInfo prefab)
        {
            // Patch the light effects to ensure they are hidden when the NoLights flag is set
            var effectCount = prefab.m_effects?.Length ?? 0;
            for (var i = 0; i < effectCount; i++)
            {
                if (prefab.m_effects[i].m_effect is LightEffect)
                    prefab.m_effects[i].m_vehicleFlagsForbidden2 |= (Vehicle.Flags2)ExtendedVehicleFlags.NoLights;
            }
        }

        public void AfterRun()
        {
        }
    }
}
