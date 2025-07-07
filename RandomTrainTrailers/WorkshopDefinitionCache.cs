using System.Collections.Generic;
using System.Linq;

namespace RandomTrainTrailers.UI
{
    internal class WorkshopDefinitionCache
    {
        private List<TrailerDefinition> _workshopDefinitions;

        public IEnumerable<TrailerDefinition.TrailerCollection> GetTrailerCollections()
        {
            if (_workshopDefinitions == null)
            {
                var defs = new List<TrailerDefinition>();
                Util.IterateModsAndAssets(Constants.DefinitionFileName, (path, name, isMod) =>
                {
                    var config = Util.XMLDeserialize<TrailerDefinition>(path);
                    if (config != null)
                        defs.Add(config);
                });
                _workshopDefinitions = defs;
            }

            return _workshopDefinitions.SelectMany(d => d.Collections);
        }
    }
}
