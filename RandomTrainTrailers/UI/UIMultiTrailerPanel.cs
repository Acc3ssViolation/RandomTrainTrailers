using ColossalFramework.UI;
using RandomTrainTrailers.Definition;
using UnityEngine;

namespace RandomTrainTrailers.UI
{
    public class UIMultiTrailerPanel : UIPanel
    {
        public static UIMultiTrailerPanel main { get; private set; }

        private UIFastList m_trailerFastList;
        private UIPanel m_propertiesPanel;
        private UIPanel m_trailerPanel;
        private UIButton m_addTrailer;

        private UITextField m_nameField;

        public Trailer CurrentMultiTrailer { get { return m_selectedTrailer; } }
        private Trailer m_selectedTrailer;

        public const int HEIGHT = 550;
        public const int WIDTH = 550;

        public override void Awake()
        {
            if(main == null)
            {
                main = this;
            }
            base.Awake();
        }

        public override void Start()
        {
            base.Start();
            UIView view = UIView.GetAView();

            name = "RTTCollectionsEditor";
            backgroundSprite = "UnlockingPanel2";
            isVisible = false;
            canFocus = true;
            isInteractive = true;
            width = WIDTH;
            height = HEIGHT;
            relativePosition = new Vector3(Mathf.Floor((view.fixedWidth - width) / 2), Mathf.Floor((view.fixedHeight - height) / 2));

            CreateComponents();
        }

        private void CreateComponents()
        {
            float verticalOffset = 50;

            // header text
            UILabel label = AddUIComponent<UILabel>();
            label.text = "Multi Trailer Editor";
            label.relativePosition = new Vector3(WIDTH / 2 - label.width / 2, 13);

            // drag
            UIDragHandle handle = AddUIComponent<UIDragHandle>();
            handle.target = this;
            handle.constrainToScreen = true;
            handle.width = WIDTH;
            handle.height = 40;
            handle.relativePosition = Vector3.zero;

            // close button
            UIButton closeButton = UIUtils.CreateButton(this);
            closeButton.size = new Vector2(30, 30);
            closeButton.normalBgSprite = "buttonclose";
            closeButton.hoveredBgSprite = "buttonclosehover";
            closeButton.pressedBgSprite = "buttonclosepressed";
            closeButton.relativePosition = new Vector3(WIDTH - 35, 5);
            closeButton.eventClicked += (c, p) =>
            {
                Hide();
            };

            // Panels
            m_propertiesPanel = AddUIComponent<UIPanel>();
            m_propertiesPanel.relativePosition = new Vector3(10, verticalOffset + 60);
            m_propertiesPanel.width = (WIDTH - 25) / 2;
            m_propertiesPanel.height = HEIGHT - verticalOffset - 70;
            //vehiclePanel.backgroundSprite = "UnlockingPanel";
            label = AddUIComponent<UILabel>();
            label.text = "Settings";
            label.relativePosition = m_propertiesPanel.relativePosition + new Vector3(0, -20);

            m_trailerPanel = AddUIComponent<UIPanel>();
            m_trailerPanel.relativePosition = new Vector3(m_propertiesPanel.relativePosition.x + m_propertiesPanel.width + 5, verticalOffset + 60);
            m_trailerPanel.width = (WIDTH - 25) / 2;
            m_trailerPanel.height = m_propertiesPanel.height;
            //trailerPanel.backgroundSprite = "UnlockingItemBackground";
            label = AddUIComponent<UILabel>();
            label.text = "Trailers";
            label.relativePosition = m_trailerPanel.relativePosition + new Vector3(0, -20);

            m_addTrailer = UIUtils.CreateButton(m_trailerPanel);
            m_addTrailer.text = "Add trailer";
            m_addTrailer.relativePosition = new Vector3(0, m_trailerPanel.height - m_addTrailer.height);
            m_addTrailer.eventClicked += (c, m) =>
            {
                // Show panel and add trailer maybe idk
                UIFindAssetPanel.main.Show((data) =>
                {
                    if(m_selectedTrailer != null)
                    {
                        m_selectedTrailer.SubTrailers.Add(new Trailer()
                        {
                            AssetName = data.info.name,
                            IsCollection = false,
                        });
                        UpdatePanels();
                    }
                },
                UIFindAssetPanel.DisplayMode.Trailers);

            };

            // fastlist
            m_trailerFastList = UIFastList.Create<UISubTrailerRow>(m_trailerPanel);
            m_trailerFastList.backgroundSprite = "UnlockingPanel";
            m_trailerFastList.width = m_trailerPanel.width;
            m_trailerFastList.height = m_trailerPanel.height - 35;
            m_trailerFastList.canSelect = true;
            m_trailerFastList.relativePosition = Vector3.zero;

            // Settings
            float y = 0;
            float padding = 10;
            label = m_propertiesPanel.AddUIComponent<UILabel>();
            label.text = "Name";
            label.relativePosition = new Vector3(0, y);

            m_nameField = UIUtils.CreateTextField(m_propertiesPanel);
            m_nameField.relativePosition = new Vector3(label.width + 5, y);
            m_nameField.width = m_propertiesPanel.width - m_nameField.relativePosition.x - 5;
            m_nameField.eventTextChanged += (c, text) =>
            {
                if(m_selectedTrailer != null)
                {
                    if(!string.IsNullOrEmpty(text))
                    {
                        m_nameField.color = Color.white;
                    }
                    else
                    {
                        m_nameField.color = Color.red;
                    }
                }
            };
            m_nameField.eventTextSubmitted += (c, text) =>
            {
                m_selectedTrailer.AssetName = text;

                UIMainPanel.main.UpdatePanels();
                if(UICollectionsPanel.main.isVisible)
                {
                    UICollectionsPanel.main.UpdatePanels();
                }
            };
            y += padding + m_nameField.height;
        }

        /// <summary>
        /// Removes subtrailer from currently selected trailer.
        /// </summary>
        /// <param name="trailerDef"></param>
        /// <returns></returns>
        public bool RemoveTrailer(Trailer trailerDef)
        {
            if(m_selectedTrailer != null)
            {
                if(m_selectedTrailer.SubTrailers.Count > 1)
                {
                    var b = m_selectedTrailer.SubTrailers.Remove(trailerDef);
                    UpdatePanels();
                    return b;
                }
            }
            return false;
        }

        public void UpdatePanels()
        {
            //Util.Log("selectedTrainData: " + (selectedTrainData != null ? selectedTrainData.name : "NULL"));
            FastList<object> newRowsData = new FastList<object>();

            if(m_selectedTrailer != null)
            {
                m_propertiesPanel.isVisible = true;
                m_trailerPanel.isVisible = true;

                for(int i = 0; i < m_selectedTrailer.SubTrailers.Count; i++)
                {
                    newRowsData.Add(new UISubTrailerRow.SubTrailerData()
                    {
                        data = m_selectedTrailer.SubTrailers[i],
                        index = i,
                    });
                }

                m_nameField.text = m_selectedTrailer.AssetName;
            }
            else
            {
                m_propertiesPanel.isVisible = false;
                m_trailerPanel.isVisible = false;
            }

            m_trailerFastList.rowHeight = UISubTrailerRow.HEIGHT;
            m_trailerFastList.rowsData = newRowsData;
        }

        public void Show(Trailer multiTrailer)
        {
            m_selectedTrailer = multiTrailer;
            base.Show(true);
            UpdatePanels();
        }
    }
}
