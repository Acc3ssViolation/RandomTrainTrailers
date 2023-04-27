using ColossalFramework;
using RandomTrainTrailers.Definition;
using System;

namespace RandomTrainTrailers.UI
{
    internal class UIDataManager : Singleton<UIDataManager>
    {
        public TrailerDefinition AvailableDefinition => ConfigurationManager.instance.GetCombinedDefinition();
        public TrailerDefinition EditDefinition => ConfigurationManager.instance.GetDefinition(ConfigurationManager.User);

        public event Action EventEditDefinitionChanged;
        public event Action EventAvailableDefinitionChanged;

        public void Awake()
        {
            ConfigurationManager.instance.EventInvalidated += () =>
            {
                EventEditDefinitionChanged?.Invoke();
                EventAvailableDefinitionChanged?.Invoke();
            };
        }

        public void Invalidate(bool updateAvailable = true)
        {
            ConfigurationManager.instance.Invalidate();
            if (updateAvailable)
                ConfigurationManager.instance.GetCombinedDefinition();
        }
    }
}
