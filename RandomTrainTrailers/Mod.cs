using ICities;
using ColossalFramework;
using ColossalFramework.UI;
using UnityEngine;
using CitiesHarmony.API;

namespace RandomTrainTrailers
{
    public class Mod : IUserMod
    {
        public const string name = "Random Train Trailers";
        public const string versionString = "3.0.0";
        public const string settingsFile = "RandomTrainTrailers";
        public const string harmonyPackage = "com.github.accessviolation.rtt";

        public string Description
        {
            get
            {
                return "Configurable train consist randomization";
            }
        }

        public string Name
        {
            get
            {
                return name + " " + versionString;
            }
        }

        public Mod()
        {
            if(GameSettings.FindSettingsFileByName(settingsFile) == null)
            {
                GameSettings.AddSettingsFile(new SettingsFile
                {
                    fileName = settingsFile,
                });
            }
        }

        public void OnEnabled()
        {
            HarmonyHelper.EnsureHarmonyInstalled();
        }

        public void OnSettingsUI(UIHelperBase helper)
        {
            UIHelperBase group = helper.AddGroup(name);
            UITextField field = null;
            field = (UITextField)group.AddTextfield("Global trailer limit", TrailerManager.GlobalTrailerLimit.value.ToString(), (s) =>
            {
                int value = 0;
                if(int.TryParse(s, out value))
                {
                    field.textColor = Color.white;
                    TrailerManager.GlobalTrailerLimit.value = value;
                }
                else
                {
                    field.textColor = Color.red;
                }
            }, (s) => {
                int value = 0;
                if(int.TryParse(s, out value))
                {
                    field.textColor = Color.white;
                    TrailerManager.GlobalTrailerLimit.value = value;
                }
                else
                {
                    field.text = TrailerManager.GlobalTrailerLimit.value.ToString();
                }
            });

            /*UICheckBox checkBox = (UICheckBox)group.AddCheckbox("[Experimental] Enable cargo features", Mod.enableUseCargo, (b) => {
                Mod.enableUseCargo.value = b;
            });
            checkBox.tooltip = "Enables the ability to have trailers be decided based on the cargo contents of the train rather than being completely random.";*/

            UICheckBox checkBox = (UICheckBox)group.AddCheckbox("Enable full log", Util.enableLogs, (b) => {
                Util.enableLogs.value = b;
            });
            checkBox.tooltip = "Enables this mod's full debug log output which is only needed for debugging. Warnings and errors are always logged.";

            checkBox = (UICheckBox)group.AddCheckbox("Enable UI", ModLoadingExtension.enableUI, (b) => {
                ModLoadingExtension.enableUI.value = b;
            });
            checkBox.tooltip = "Toggles the in-game UI to change configs.";

        }        

        public static bool IsValidLoadMode(LoadMode mode)
        {
            return (mode == LoadMode.LoadGame || mode == LoadMode.NewGame);
        }
    }
}
