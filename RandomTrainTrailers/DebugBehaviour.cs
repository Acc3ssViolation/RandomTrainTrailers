using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

namespace RandomTrainTrailers
{
    public class DebugBehaviour : MonoBehaviour
    {
        void Update()
        {
            if(IsKeyDown(KeyCode.R, true, true))
            {
                TrailerManager.Setup();
                Debug.Log("Reloaded trailer definitions");
            }
        }

        bool IsKeyDown(KeyCode key, bool ctrl = false, bool alt = false)
        {
            bool result = Input.GetKeyDown(key);

            if(result)
            {
                result = (ctrl) ? (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) : true;
                if(result)
                {
                    result = (alt) ? (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) : true;
                }
            }

            return result;
        }
    }
}
