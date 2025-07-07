using RandomTrainTrailers.Definition;

namespace RandomTrainTrailers
{
    class SharedTrailerConfigLoader : AbstractConfigLoader
    {
        public override string FileName
        {
            get
            {
                return Constants.DefinitionFileName;
            }
        }

        public override void OnFileFound(string path, string name, bool isMod)
        {
            var config = Util.XMLDeserialize<TrailerDefinition>(path);
            if(config == null)
            {
                Util.LogError("Unable to load RTT config for " + (isMod ? "mod ": "asset ") + name);
                return;
            }
            Util.Log("Loading RTT config from " + (isMod ? "mod " : "asset ") + name);
            ConfigurationManager.instance.Add(path, config);
        }

        public override void Prepare()
        {
        }
    }
}
