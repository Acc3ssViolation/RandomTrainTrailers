using ICities;
using ColossalFramework;
using ColossalFramework.UI;

namespace RandomTrainTrailers
{
    public class Mod : IUserMod
    {
        public const string name = "Random Train Trailers";
        public const string versionString = "1.1";
        public const string settingsFile = "RandomTrainTrailers";

        public string Description
        {
            get
            {
                return "Version " + versionString + ". Gives the option to have random trailers spawn for trains and other vehicles.";
            }
        }

        public string Name
        {
            get
            {
                return name;
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
