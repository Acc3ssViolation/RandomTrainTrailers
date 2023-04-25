using ColossalFramework.UI;
using RandomTrainTrailers.Definition;
using UnityEngine;

namespace RandomTrainTrailers.UI
{
    public class UIBlacklistRow : UIPanel, IUIFastListRow
    {
        public const int HEIGHT = 60;

        private UILabel m_assetName;
        private UIButton m_buttonRemove;
        private BlacklistItem m_currentDataItem;

        public override void Start()
        {
            base.Start();

            width = parent.width;
            height = HEIGHT;

            CreateComponents();
        }

        private void CreateComponents()
        {
            if(m_assetName != null) return;

            m_assetName = AddUIComponent<UILabel>();
            m_assetName.relativePosition = new Vector3(10, 10);

            // Button for removing prefab
            m_buttonRemove = UIUtils.CreateButton(this);
            m_buttonRemove.size = new Vector2(30, 30);
            m_buttonRemove.normalBgSprite = "buttonclose";
            m_buttonRemove.hoveredBgSprite = "buttonclosehover";
            m_buttonRemove.pressedBgSprite = "buttonclosepressed";
            m_buttonRemove.relativePosition = new Vector3(UIMainPanel.main.BlacklistRowWidth - 35, 25);
            m_buttonRemove.eventClicked += (c, p) => {

                ConfirmPanel.ShowModal(Mod.name, "Are you sure you want to remove " + m_assetName.text + "?", delegate (UIComponent comp, int ret)
                {
                    if(ret == 1)
                    {
                        UIMainPanel.main.RemoveBlacklist(m_currentDataItem);
                    }
                });
            };
            m_buttonRemove.tooltip = "Removes the item from the blacklist.";
        }

        public void Display(object data, bool isRowOdd)
        {
            CreateComponents();

            var itemData = data as BlacklistItem;
            if(itemData == null)
                return;

            m_currentDataItem = itemData;
            m_assetName.text = itemData.AssetName;
            m_assetName.tooltip = m_assetName.text;

            if(isRowOdd)
            {
                backgroundSprite = "UnlockingItemBackground";
                color = new Color32(255, 255, 255, 255);
            }
            else
            {
                backgroundSprite = null;
            }
        }


        public void Deselect(bool isRowOdd)
        {
            if(isRowOdd)
            {
                backgroundSprite = "UnlockingItemBackground";
                color = new Color32(255, 255, 255, 255);
            }
            else
            {
                backgroundSprite = null;
            }
        }

        public void Select(bool isRowOdd)
        {
            backgroundSprite = "ListItemHighlight";
            color = new Color32(255, 255, 255, 255);
        }
    }
}
