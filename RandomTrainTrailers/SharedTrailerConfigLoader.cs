using RandomTrainTrailers.Definition;

namespace RandomTrainTrailers
{
    class SharedTrailerConfigLoader : AbstractConfigLoader
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
            Util.Log("Loading RTT config from " + (isMod ? "mod " : "asset ") + name);
            TrailerManager.ApplyDefinition(ref config);
        }

        public override void Prepare()
        {
        }
    }
}
