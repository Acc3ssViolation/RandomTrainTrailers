using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ICities;
using RandomTrainTrailers.Detour;
using ColossalFramework.UI;

namespace RandomTrainTrailers
{
    public class ModLoadingExtension : LoadingExtensionBase
    {
        private VehicleDetour m_detours;
        private GameObject m_gameObject; 

        public override void OnLevelLoaded(LoadMode mode)
        {
            if(Mod.IsValidLoadMode(mode))
            {
                m_detours = new VehicleDetour();
                m_detours.Deploy();
                //VehiclePrefabs.FindPrefabs();                     Uncomment this when reallowing "all trailers allowed" option
                TrailerManager.Setup();
                m_gameObject = new GameObject(Mod.name);
                m_gameObject.AddComponent<DebugBehaviour>();
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
        }
    }
}
