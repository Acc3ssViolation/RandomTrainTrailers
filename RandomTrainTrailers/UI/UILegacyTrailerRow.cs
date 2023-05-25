using ColossalFramework.UI;
using RandomTrainTrailers.Definition;
using UnityEngine;

namespace RandomTrainTrailers.UI
{
    public class UILegacyTrailerRow : UIPanel, IUIFastListRow
    { 
        private UILegacyMainPanel mainPanel;

        private UILabel labelAssetName;
        private UIButton buttonRemove;
        private UIButton buttonUp;
        private UIButton buttonDown;
        private UIPanel settingsPanel;
        private UIIntField fieldInvert;
        private UIIntField fieldWeight;
        private UIButton buttonEditMulti;
        private UIButton buttonCargoType;

        private UIPanel upDownPanel;

        public const int HEIGHT = 80;

        private bool checkEvents = false;

        private Trailer m_currentDataItem;

        public override void Start()
        {
            base.Start();

            width = parent.width;
            height = HEIGHT;

            CreateComponents();
        }

        private void CreateComponents()
        {
            if(settingsPanel != null) return;

            mainPanel = UILegacyMainPanel.Main;

            settingsPanel = AddUIComponent<UIPanel>();
            settingsPanel.relativePosition = Vector3.zero;
            UIHelper settingsPanelHelper = new UIHelper(settingsPanel);

            upDownPanel = AddUIComponent<UIPanel>();
            upDownPanel.relativePosition = new Vector3(UILegacyMainPanel.Main.TrailerRowWidth - 70, 0);

            // Name label
            labelAssetName = AddUIComponent<UILabel>();
            labelAssetName.textScale = 0.8f;
            labelAssetName.relativePosition = new Vector3(10, 10);

            // Weight field
            fieldWeight = UIIntField.CreateField("Weight:", settingsPanel, false);
            fieldWeight.panel.relativePosition = new Vector3(10, 25);
            fieldWeight.textField.eventTextChanged += (c, t) => {
                if(!checkEvents) { return; }

                int val = m_currentDataItem.Weight;
                fieldWeight.IntFieldHandler(ref val);
                m_currentDataItem.Weight = val;
            };

            // Field for invert
            fieldInvert = UIIntField.CreateField("Invert:", settingsPanel, false);
            fieldInvert.panel.relativePosition = new Vector3(10, 50);
            fieldInvert.textField.eventTextChanged += (c, t) => {
                if(!checkEvents) { return; }

                int val = m_currentDataItem.InvertProbability;
                fieldInvert.IntFieldHandler(ref val);
                m_currentDataItem.InvertProbability = val;
            };

            // buttonEditMulti
            buttonEditMulti = UIUtils.CreateButton(settingsPanel);
            buttonEditMulti.text = "Edit MultiTrailer";
            buttonEditMulti.relativePosition = new Vector3(10, 50);
            buttonEditMulti.width = 130;
            buttonEditMulti.height -= 5;
            buttonEditMulti.eventClicked += (c, e) => {
                if(!checkEvents) { return; }

                UIMultiTrailerPanel.main.Show(m_currentDataItem);
            };

            // Button for removing prefab
            buttonRemove = UIUtils.CreateButton(settingsPanel);
            buttonRemove.size = new Vector2(30, 30);
            buttonRemove.normalBgSprite = "buttonclose";
            buttonRemove.hoveredBgSprite = "buttonclosehover";
            buttonRemove.pressedBgSprite = "buttonclosepressed";
            buttonRemove.relativePosition = new Vector3(UILegacyMainPanel.Main.TrailerRowWidth - 35, 25);
            buttonRemove.eventClicked += (c, p) => {
                if(!checkEvents) { return; }

                ConfirmPanel.ShowModal(Mod.name, "Are you sure you want to remove " + labelAssetName.text + "?", delegate (UIComponent comp, int ret)
                {
                    if(ret == 1)
                    {
                        mainPanel.RemoveTrailer(m_currentDataItem);
                    }
                });
            };
            buttonRemove.tooltip = "Removes the trailer.";

            // Button for changing cargo type
            buttonCargoType = UIUtils.CreateButton(settingsPanel);
            buttonCargoType.size = new Vector2(44, 30);
            buttonCargoType.atlas = UIUtils.GetAtlas("Thumbnails");
            buttonCargoType.normalBgSprite = "ZoningIndustrial";
            buttonCargoType.hoveredBgSprite = "ZoningIndustrialHovered";
            buttonCargoType.pressedBgSprite = "ZoningIndustrialPressed";
            buttonCargoType.relativePosition = new Vector3(135, 25);
            buttonCargoType.eventClicked += (c, p) => {
                if(!checkEvents) { return; }

                if(m_currentDataItem != null)
                {
                    UIFlagsPanel.Main.Content.Show(m_currentDataItem.CargoType, (flags) =>
                    {
                        if(m_currentDataItem != null)
                        {
                            m_currentDataItem.CargoType = flags;
                            Util.Log("Changed cargo type for " + m_currentDataItem.AssetName + " to " + flags.ToString());
                        }
                    });
                }
            };
            buttonCargoType.tooltip = "Change the wagon's cargo type.";

            // Button to move prefab up
            buttonUp = UIUtils.CreateButton(upDownPanel);
            buttonUp.size = new Vector2(30, 30);
            buttonUp.normalBgSprite = "IconDownArrow";
            buttonUp.hoveredBgSprite = "IconDownArrowHovered";
            buttonUp.pressedBgSprite = "IconDownArrowPressed";
            buttonUp.relativePosition = new Vector3(30, 50);
            buttonUp.transform.Rotate(Vector3.forward, 180);
            buttonUp.eventClicked += (c, p) => {
                if(!checkEvents) { return; }

                //itemData.trainData.prefabs.Swap(itemData.prefabIndex, itemData.prefabIndex - 1);
                //Update UI
                mainPanel.UpdatePanels();
            };
            buttonUp.tooltip = "Moves the trailer up.";

            // Button to move prefab down
            buttonDown = UIUtils.CreateButton(upDownPanel);
            buttonDown.size = new Vector2(30, 30);
            buttonDown.normalBgSprite = "IconDownArrow";
            buttonDown.hoveredBgSprite = "IconDownArrowHovered";
            buttonDown.pressedBgSprite = "IconDownArrowPressed";
            buttonDown.relativePosition = new Vector3(0, 50);
            buttonDown.eventClicked += (c, p) => {
                if(!checkEvents) { return; }

                //itemData.trainData.prefabs.Swap(itemData.prefabIndex, itemData.prefabIndex + 1);
                //Update UI
                mainPanel.UpdatePanels();
            };
            buttonDown.tooltip = "Moves the trailer down.";
        }

        public void Display(object data, bool isRowOdd)
        {
            CreateComponents();

            var itemData = data as Trailer;
            if(itemData == null)
                return;

            bool collection = itemData.IsCollection;
            bool multiTrailer = itemData.IsMultiTrailer;

            m_currentDataItem = itemData;

            upDownPanel.isVisible = false;

            // Name
            labelAssetName.text = itemData.AssetName;
            labelAssetName.tooltip = itemData.AssetName;
            if(collection)
            {
                labelAssetName.text = "(Collection) " + labelAssetName.text;
                labelAssetName.textColor = Color.white;
            }
            else
            {
                labelAssetName.textColor = (itemData.VehicleInfos == null) ? Color.red : Color.white;
            }
            // Settings
            fieldWeight.SetValue(itemData.Weight);
            fieldInvert.SetValue(itemData.InvertProbability);
            buttonCargoType.tooltip = itemData.CargoType.ToString();

            // Disable/enable components based on type
            fieldInvert.panel.isVisible = !multiTrailer && !collection;
            buttonEditMulti.isVisible = multiTrailer;
            buttonCargoType.isVisible = !collection;

            if(isRowOdd)
            {
                backgroundSprite = "UnlockingItemBackground";
                color = new Color32(255, 255, 255, 255);
            }
            else
            {
                backgroundSprite = null;
            }

            checkEvents = true;
        }


        public void Deselect(bool isRowOdd)
        {
        }

        public void Select(bool isRowOdd)
        {
        }
    }
}
