using UnityEngine;
using ICities;
using RandomTrainTrailers.Detour;
using ColossalFramework.UI;
using RandomTrainTrailers.UI;
using ColossalFramework;
using CitiesHarmony.API;
using RandomTrainTrailers.Loading;

namespace RandomTrainTrailers
{
    public class ModLoadingExtension : LoadingExtensionBase
    {
        public static SavedBool enableUI = new SavedBool("EnableUI", Mod.settingsFile, true, true);
        private GameObject m_gameObject;

        public static GameObject UIObject { get; private set; }

        public override void OnCreated(ILoading loading)
        {
            base.OnCreated(loading);

            if (HarmonyHelper.IsHarmonyInstalled) HarmonyDetourAIs.Deploy();
        }

        public override void OnReleased()
        {
            base.OnReleased();

            if (HarmonyHelper.IsHarmonyInstalled) HarmonyDetourAIs.Revert();
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            if(Mod.IsValidLoadMode(mode))
            {
                var loader = new PrefabLoading<VehicleInfo>();
                loader.AddHook(new EffectPatchHook());
                loader.AddHook(new VehiclePrefabs.VehiclePrefabHook());
                loader.Run();

                TrailerManager.Setup();
                m_gameObject = new GameObject(Mod.name);
                m_gameObject.AddComponent<DebugBehaviour>();

                // Create UI
                if(enableUI == true)
                {
                    UIView view = UIView.GetAView();
                    UIObject = new GameObject("RandomTrainTrailers");
                    UIObject.transform.SetParent(view.transform);
                    UIObject.AddComponent<UILegacyMainPanel>();

                    Util.Log("UI is enabled");
                }
                else
                {
                    Util.Log("UI is disabled");
                }
            }
        }

        public override void OnLevelUnloading()
        {                
            if(m_gameObject != null)
            {
                GameObject.Destroy(m_gameObject.gameObject);
            }

            if(UIObject != null)
            {
                UILegacyMainPanel.main.OnLevelUnloading();
                GameObject.Destroy(UIObject);
            }
        }
    }
}
