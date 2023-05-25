using ColossalFramework.UI;
using RandomTrainTrailers.Definition;
using UnityEngine;

namespace RandomTrainTrailers.UI
{
    public class UISubTrailerRow : UIPanel, IUIFastListRow
    { 
        public class SubTrailerData
        {
            public int index;
            public Trailer data;
        }

        private UILabel labelAssetName;
        private UIButton buttonRemove;
        private UIButton buttonUp;
        private UIButton buttonDown;
        private UIPanel settingsPanel;
        private UIIntField fieldInvert;
        private UIButton buttonEditMulti;

        private UIPanel upDownPanel;

        public const int HEIGHT = 80;

        private bool checkEvents = false;

        private SubTrailerData m_currentDataItem;

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

            settingsPanel = AddUIComponent<UIPanel>();
            settingsPanel.relativePosition = Vector3.zero;
            UIHelper settingsPanelHelper = new UIHelper(settingsPanel);

            upDownPanel = AddUIComponent<UIPanel>();
            upDownPanel.relativePosition = new Vector3(UILegacyMainPanel.Main.TrailerRowWidth - 70, 0);
            upDownPanel.width = 30;

            // Name label
            labelAssetName = AddUIComponent<UILabel>();
            labelAssetName.textScale = 0.8f;
            labelAssetName.relativePosition = new Vector3(10, 10);

            // Field for invert
            fieldInvert = UIIntField.CreateField("Invert:", settingsPanel, false);
            fieldInvert.panel.relativePosition = new Vector3(10, 25);
            fieldInvert.textField.eventTextChanged += (c, t) => {
                if(!checkEvents) { return; }

                int val = m_currentDataItem.data.InvertProbability;
                fieldInvert.IntFieldHandler(ref val);
                m_currentDataItem.data.InvertProbability = val;
            };

            // buttonEditMulti
            buttonEditMulti = UIUtils.CreateButton(settingsPanel);
            buttonEditMulti.text = "Edit MultiTrailer";
            buttonEditMulti.relativePosition = new Vector3(10, 50);
            buttonEditMulti.width = 130;
            buttonEditMulti.height -= 5;
            buttonEditMulti.eventClicked += (c, e) => {
                if(!checkEvents) { return; }
            };

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

                if(UIMultiTrailerPanel.main.CurrentMultiTrailer != null)
                {
                    UIMultiTrailerPanel.main.CurrentMultiTrailer.SubTrailers.SwapChecked(m_currentDataItem.index, m_currentDataItem.index - 1);
                    //Update UI
                    UIMultiTrailerPanel.main.UpdatePanels();
                }
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
                
                if(UIMultiTrailerPanel.main.CurrentMultiTrailer != null)
                {
                    UIMultiTrailerPanel.main.CurrentMultiTrailer.SubTrailers.SwapChecked(m_currentDataItem.index, m_currentDataItem.index + 1);
                    //Update UI
                    UIMultiTrailerPanel.main.UpdatePanels();
                }
                
            };
            buttonDown.tooltip = "Moves the trailer down.";

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
                        UIMultiTrailerPanel.main.RemoveTrailer(m_currentDataItem.data);
                    }
                });
            };
            buttonRemove.tooltip = "Removes the trailer.";
        }

        public void Display(object data, bool isRowOdd)
        {
            CreateComponents();

            var itemData = data as SubTrailerData;
            if(itemData == null)
                return;

            bool collection = itemData.data.IsCollection;
            bool multiTrailer = itemData.data.IsMultiTrailer;

            m_currentDataItem = itemData;

            upDownPanel.isVisible = true;

            // Name
            labelAssetName.text = itemData.data.AssetName;
            labelAssetName.tooltip = itemData.data.AssetName;
            if(collection)
            {
                labelAssetName.text = "NOT ALLOWED (Collection) " + labelAssetName.text;
                labelAssetName.textColor = Color.red;
            }
            else
            {
                if(multiTrailer)
                {
                    labelAssetName.text = "NOT ALLOWED (Multi Trailer) " + labelAssetName.text;
                    labelAssetName.textColor = Color.red;
                }
                else
                {
                    labelAssetName.textColor = (itemData.data.VehicleInfos == null) ? Color.red : Color.white;
                }   
            }
            // Settings
            fieldInvert.SetValue(itemData.data.InvertProbability);

            // Disable/enable components based on type
            fieldInvert.panel.isVisible = !multiTrailer && !collection;
            buttonEditMulti.isVisible = false;

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
