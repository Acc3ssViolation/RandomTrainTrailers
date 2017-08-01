using ICities;
using ColossalFramework;
using ColossalFramework.UI;
using UnityEngine;

namespace RandomTrainTrailers
{
    public class Mod : IUserMod
    {
        public const string name = "Random Train Trailers";
        public const string versionString = "1.4.2";
        public const string settingsFile = "RandomTrainTrailers";
        public const string harmonyPackage = "com.github.accessviolation.rtt";

        public string Description
        {
            get
            {
                return "Now with UI! Gives the option to have random trailers spawn for trains and other vehicles.";
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

        public void OnSettingsUI(UIHelperBase helper)
        {
            UIHelperBase group = helper.AddGroup(name);
            UITextField field = null;
            field = (UITextField)group.AddTextfield("Global trailer limit", TrailerManager.globalTrailerLimit.value.ToString(), (s) =>
            {
                int value = 0;
                if(int.TryParse(s, out value))
                {
                    field.textColor = Color.white;
                    TrailerManager.globalTrailerLimit.value = value;
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
                    TrailerManager.globalTrailerLimit.value = value;
                }
                else
                {
                    field.text = TrailerManager.globalTrailerLimit.value.ToString();
                }
            });

            UICheckBox checkBox = (UICheckBox)group.AddCheckbox("Enable full log", Util.enableLogs, (b) => {
                Util.enableLogs.value = b;
            });
            checkBox.tooltip = "Enables this mod's full debug log output which is only needed for debugging. Warnings and errors are always logged.";

            // TODO: Make sure this works on mac
            group.AddButton("Open user config directory", () => {
                System.Diagnostics.Process.Start(ColossalFramework.IO.DataLocation.localApplicationData);
            });
            group.AddButton("Open default config directory", () => {
                System.Diagnostics.Process.Start(Util.ModDirectory);
            });
        }

        public static bool IsValidLoadMode(LoadMode mode)
        {
            return (mode == LoadMode.LoadGame || mode == LoadMode.NewGame);
        }
    }
}
