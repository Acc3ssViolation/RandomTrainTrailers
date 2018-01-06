using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomTrainTrailers
{
    class SharedTrailerConfigLoader : ConfigLoader
    {
        public override string FileName
        {
            get
            {
                return "RTT-Definition.xml";
            }
        }

        public override void OnFileFound(string path, string name, bool isMod)
        {
            var config = XMLDeserialize<TrailerDefinition>(path);
            if(config == null)
            {
                Util.LogError("Unable to load RTT config for " + (isMod ? "mod ": "asset ") + name);
                return;
            }
            TrailerManager.ApplyDefinition(ref config);
        }

        public override void Prepare()
        {
        }
    }
}
