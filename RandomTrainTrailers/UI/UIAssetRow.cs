using ColossalFramework.UI;
using UnityEngine;

namespace RandomTrainTrailers.UI
{
    public class UIAssetRow : UIPanel, IUIFastListRow
    {
        public const int HEIGHT = 80;

        private UILabel m_localeName;
        private UILabel m_assetName;
        private UILabel m_isTrailer;

        public override void Start()
        {
            base.Start();

            width = parent.width;
            height = HEIGHT;

            CreateComponents();
        }

        private void CreateComponents()
        {
            if(m_localeName != null) return;

            m_assetName = AddUIComponent<UILabel>();
            m_assetName.relativePosition = new Vector3(10, 10);
            m_localeName = AddUIComponent<UILabel>();
            m_localeName.relativePosition = new Vector3(10, 30);
            m_localeName.textScale = 0.8f;
            m_isTrailer = AddUIComponent<UILabel>();
            m_isTrailer.relativePosition = new Vector3(10, 50);
        }

        public void Display(object data, bool isRowOdd)
        {
            CreateComponents();

            var itemData = data as VehiclePrefabs.VehicleData;
            if(itemData == null)
                return;

            m_localeName.text = itemData.localeName;
            if(itemData.info != null)
            {
                m_assetName.text = itemData.info.name;
                m_isTrailer.text = itemData.isTrailer ? "Trailer" : "Engine";
            }
            else
            {
                m_assetName.text = "";
                m_isTrailer.text = itemData.isTrailer ? "Default Collection" : "Collection";
            }

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
