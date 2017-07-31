using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ICities;
using RandomTrainTrailers.Detour;
using ColossalFramework.UI;
using RandomTrainTrailers.UI;

namespace RandomTrainTrailers
{
    public class ModLoadingExtension : LoadingExtensionBase
    {
        private VehicleDetour m_detours;
        private GameObject m_gameObject;

        public GameObject UIObject { get; private set; }

        public override void OnLevelLoaded(LoadMode mode)
        {
            if(Mod.IsValidLoadMode(mode))
            {
                m_detours = new VehicleDetour();
                m_detours.Deploy();
                VehiclePrefabs.FindPrefabs();
                TrailerManager.Setup();
                m_gameObject = new GameObject(Mod.name);
                m_gameObject.AddComponent<DebugBehaviour>();

                // Create UI
                UIView view = UIView.GetAView();
                UIObject = new GameObject("RandomTrainTrailers");
                UIObject.transform.SetParent(view.transform);
                UIObject.AddComponent<UIMainPanel>();
            }
        }

        public override void OnLevelUnloading()
        {
            if(m_detours != null)
            {
                m_detours.Revert();
            }
                
            if(m_gameObject != null)
            {
                GameObject.Destroy(m_gameObject.gameObject);
            }

            if(UIObject != null)
            {
                GameObject.Destroy(UIObject);
            }
        }
    }
}
