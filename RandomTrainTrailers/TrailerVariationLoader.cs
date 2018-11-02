using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICities;
using UnityEngine;

namespace TrailerVariationLoader
{
    public class TrailerVariationMod : LoadingExtensionBase, IUserMod
    {
        public string Name => "Trailer Variation Loader";
        public string Description => "Loads Trailer Variation parameters.";

        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);
            ApplyParams();
        }

        public override void OnLevelUnloading()
        {
            base.OnLevelUnloading();
        }

        private void ApplyParams()
        {
            for(uint i = 0; i < PrefabCollection<VehicleInfo>.LoadedCount(); i++)
            {
                var prefab = PrefabCollection<VehicleInfo>.GetLoaded(i);
                if(prefab == null) continue;

                if(prefab.m_subMeshes != null)
                {
                    foreach(var submesh in prefab.m_subMeshes)
                    {
                        if(submesh.m_subInfo.m_mesh.name.Contains("TrailerVariation"))
                        {
                            var values = submesh.m_subInfo.m_mesh.name.Split(' ');  // "TrailerVariation" "int(variationmask)"
                            submesh.m_variationMask = Convert.ToInt32(values[1]);
                        }
                    }
                }
            }
        }
    }
}