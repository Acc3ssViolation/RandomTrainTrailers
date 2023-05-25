using ColossalFramework.Globalization;
using ColossalFramework.UI;
using System.Reflection;
using UnityEngine;

namespace RandomTrainTrailers.UI
{
    internal class UIMainPanel : UIWindowPanel
    {
        public override float DefaultWidth => 270;

        public override float DefaultHeight => 170;

        public override string DefaultTitle => Mod.name;

        private UIWindowHandle<UITrainPoolPanel> _trainPoolWindow;
        private UIWindowHandle<UILocomotivesPanel> _locomotivePoolWindow;
        private UIWindowHandle<UITrailersPanel> _trailerPoolWindow;
        private UIWindowHandle<UILegacyMainPanel> _legacyWindow;

        public override void Start()
        {
            base.Start();
            CreateComponents();
            CreateToolbarButton();

            LoadUserDef();
            Window.Resizable = false;
            Window.CloseClicked += OnCloseClicked;
        }

        private void OnCloseClicked(UIWindow obj)
        {
            // TODO: This doesn't work
            var toolstripCloseButton = UIView.GetAView().FindUIComponent<UITabstrip>("MainToolstrip").closeButton;
            // Simulate a close click to clear out the toolstrip stuff
            toolstripCloseButton.SimulateClick();
        }

        public void LoadUserDef()
        {
            TrailerManager.Setup();
            var editDefinition = UIDataManager.instance.EditDefinition;
            _trainPoolWindow.Content.SetData(editDefinition);
            _locomotivePoolWindow.Content.SetData(editDefinition);
            _trailerPoolWindow.Content.SetData(editDefinition);
            _legacyWindow.Content.SetData(editDefinition);
        }

        public void SaveUserDef()
        {
            var editDefinition = UIDataManager.instance.EditDefinition;
            if (editDefinition != null)
            {
                TrailerManager.StoreUserDefinitionOnDisk(editDefinition);
            }
        }

        public void OnLevelUnloading()
        {
            SaveUserDef();
        }

        private void CreateToolbarButton()
        {
            // TODO: Perhaps we should move to a different kind of button to open the editor window?

            // Adding main button
            var view = UIView.GetAView();
            UITabstrip toolStrip = view.FindUIComponent<UITabstrip>("MainToolstrip");
            var toolbarButton = toolStrip.AddUIComponent<UIButton>();

            toolbarButton.normalBgSprite = "SubBarPublicTransportTrain";
            toolbarButton.focusedFgSprite = "ToolbarIconGroup6Focused";
            toolbarButton.hoveredFgSprite = "ToolbarIconGroup6Hovered";

            toolbarButton.size = new Vector2(43f, 49f);
            toolbarButton.name = Mod.name + " Manager";
            toolbarButton.tooltip = toolbarButton.name;
            toolbarButton.relativePosition = new Vector3(0, 5);

            toolbarButton.eventButtonStateChanged += (c, s) =>
            {
                if (s == UIButton.ButtonState.Focused)
                {
                    if (!Window.isVisible)
                        Window.Open();
                }
                else
                {
                    Window.Close();
                    toolbarButton.Unfocus();
                }
            };

            // Locale entries for the tutorial popup
            Locale locale = (Locale)typeof(LocaleManager).GetField("m_Locale", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(LocaleManager.instance);
            Locale.Key key = new Locale.Key
            {
                m_Identifier = "TUTORIAL_ADVISER_TITLE",
                m_Key = toolbarButton.name
            };
            if (!locale.Exists(key))
            {
                locale.AddLocalizedString(key, toolbarButton.name);
            }
            key = new Locale.Key
            {
                m_Identifier = "TUTORIAL_ADVISER",
                m_Key = toolbarButton.name
            };
            if (!locale.Exists(key))
            {
                // TODO: Would be nice to update this a bit to explain things a bit better
                locale.AddLocalizedString(key, "The " + Mod.name + " editor allows you to edit the configurations for the RTT mod. This allows you to decide which trains have their trailers randomized and gives control over this randomization. Check the workshop page and the discussion pages for more info.");
            }

            // The tab container expects a tab, create an empty one so we don't get exceptions thrown in our face when the button is clicked
            view.FindUIComponent<UITabContainer>("TSContainer").AddUIComponent<UIPanel>().color = new Color32(0, 0, 0, 0);
        }

        private void CreateComponents()
        {
            // Create windows for the editors
            _trainPoolWindow = UIWindow.Create<UITrainPoolPanel>();
            _locomotivePoolWindow = UIWindow.Create<UILocomotivesPanel>();
            _trailerPoolWindow = UIWindow.Create<UITrailersPanel>();
            _legacyWindow = UIWindow.Create<UILegacyMainPanel>();
            var buttonWidth = 120;

            // Buttons to open the windows
            var trainPoolButton = UIUtils.CreateButton(this);
            trainPoolButton.text = "Trains";
            trainPoolButton.relativePosition = new Vector3();
            trainPoolButton.eventClicked += (c, m) =>
            {
                _trainPoolWindow.Open();
            };
            trainPoolButton.tooltip = "Open the train edit window.";
            trainPoolButton.width = buttonWidth;

            var locomotivesButton = UIUtils.CreateButton(this);
            locomotivesButton.text = "Locomotives";
            locomotivesButton.relativePosition = UIUtils.RightOf(trainPoolButton);
            locomotivesButton.eventClicked += (c, m) =>
            {
                _locomotivePoolWindow.Open();
            };
            locomotivesButton.tooltip = "Open the locomotive edit window.";
            locomotivesButton.width = buttonWidth;

            var trailersButton = UIUtils.CreateButton(this);
            trailersButton.text = "Trailers";
            trailersButton.relativePosition = UIUtils.Below(trainPoolButton);
            trailersButton.eventClicked += (c, m) =>
            {
                _trailerPoolWindow.Open();
            };
            trailersButton.tooltip = "Open the trailer edit window.";
            trailersButton.width = buttonWidth;

            var legacyButton = UIUtils.CreateButton(this);
            legacyButton.text = "Legacy";
            legacyButton.relativePosition = UIUtils.RightOf(trailersButton);
            legacyButton.eventClicked += (c, m) =>
            {
                _legacyWindow.Open();
            };
            legacyButton.tooltip = "Open the legacy UI from before version 3.0";
            legacyButton.width = buttonWidth;

            var atlasButton = UIUtils.CreateButton(this);
            atlasButton.text = "Atlas";
            atlasButton.relativePosition = UIUtils.RightOf(trailersButton);
            atlasButton.eventClicked += (c, m) =>
            {
                var windowHandle = UIWindow.Create<UIAtlasViewer>();
                windowHandle.Window.DestroyOnClose = true;
                windowHandle.Content.SetData(UIDataManager.instance.EditDefinition);
                windowHandle.Open();
            };
            atlasButton.width = buttonWidth;
            //Disable atlas button by default since it's just a debug tool
            atlasButton.Hide();

            var saveButton = UIUtils.CreateButton(this);
            saveButton.text = "Save";
            saveButton.relativePosition = UIUtils.Below(trailersButton);
            saveButton.anchor = UIAnchorStyle.Left | UIAnchorStyle.Bottom;
            saveButton.eventClicked += (c, m) =>
            {
                SaveUserDef();
                TrailerManager.Setup();
            };
            saveButton.tooltip = "Saves config to disk and applies it to the current game.";
            saveButton.width = buttonWidth;

            var loadButton = UIUtils.CreateButton(this);
            loadButton.text = "Load";
            loadButton.relativePosition = UIUtils.RightOf(saveButton);
            loadButton.anchor = UIAnchorStyle.Left | UIAnchorStyle.Bottom;
            loadButton.eventClicked += (c, m) =>
            {
                LoadUserDef();
            };
            loadButton.tooltip = "Loads config from disk and applies it to the current game.";
            loadButton.width = buttonWidth;
        }
    }
}
