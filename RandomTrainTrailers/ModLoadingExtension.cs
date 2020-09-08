using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ICities;
using RandomTrainTrailers.Detour;
using ColossalFramework.UI;
using RandomTrainTrailers.UI;
using ColossalFramework;
using CitiesHarmony.API;

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
                VehiclePrefabs.FindPrefabs();
                TrailerManager.Setup();
                m_gameObject = new GameObject(Mod.name);
                m_gameObject.AddComponent<DebugBehaviour>();

                // Create UI
                if(enableUI == true)
                {
                    UIView view = UIView.GetAView();
                    UIObject = new GameObject("RandomTrainTrailers");
                    UIObject.transform.SetParent(view.transform);
                    UIObject.AddComponent<UIMainPanel>();
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
                UIMainPanel.main.OnLevelUnloading();
                GameObject.Destroy(UIObject);
            }
        }
    }
}
