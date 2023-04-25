using ColossalFramework.UI;
using RandomTrainTrailers.Definition;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RandomTrainTrailers.UI
{
    public class UICollectionsPanel : UIPanel
    {
        public static UICollectionsPanel main { get; private set; }

        private UIDropDown m_collectionDropdown;
        private UIFastList m_trailerFastList;
        private UIPanel m_collectionPanel;
        private UIPanel m_trailerPanel;
        private UILabel m_labelNoCol;
        private UIButton m_addTrailer;
        private UIButton m_addMultiTrailer;

        private UITextField m_nameField;

        private TrailerCollection m_selectedCollection;

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
            label.text = "Collection Editor";
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
            closeButton.eventClicked += (c, p) => {
                Hide();
            };

            // dropdown
            label = AddUIComponent<UILabel>();
            label.text = "Collection: ";
            label.textScale = 0.8f;
            label.padding = new RectOffset(0, 0, 8, 0);
            label.relativePosition = new Vector3(10f, verticalOffset);

            m_collectionDropdown = UIUtils.CreateDropDown(this);
            m_collectionDropdown.relativePosition = new Vector3(95f, verticalOffset);
            m_collectionDropdown.width = 250f;
            m_collectionDropdown.eventSelectedIndexChanged += VehicleDropdown_eventSelectedIndexChanged;
            m_collectionDropdown.tooltip = "???";

            // new button
            UIButton button = UIUtils.CreateButton(this);
            button.text = "Add new";
            button.relativePosition = new Vector3(355f, verticalOffset);
            button.eventClicked += (c, p) =>
            {
                if(UIMainPanel.main.UserDefinition != null)
                {
                    int number = 1;
                    bool added = false;
                    do
                    {
                        string name = "New Trailer Collection " + number;
                        if(UIMainPanel.main.UserDefinition.Collections.Where((collection) => {
                            return collection.Name == name;
                        }).Count() > 0)
                        {
                            number++;
                        }
                        else
                        {
                            UIMainPanel.main.UserDefinition.Collections.Add(new TrailerCollection(name));
                            added = true;
                        }
                    } while(!added);

                    UpdateCollectionList();
                    DisplayLast();
                }
            };
            button.tooltip = "Adds a new trailer collection.";

            // delete button
            button = UIUtils.CreateButton(this);
            button.text = "Delete";
            button.relativePosition = new Vector3(450f, verticalOffset);
            button.eventClicked += (c, p) =>
            {
                if(m_selectedCollection != null && UIMainPanel.main.UserDefinition != null)
                {
                    ConfirmPanel.ShowModal(Mod.name, "Are you sure you want to delete the collection " + m_selectedCollection.Name + "?", delegate (UIComponent comp, int ret)
                    {
                        if(ret == 1)
                        {
                            UIMainPanel.main.UserDefinition.Collections.Remove(m_selectedCollection);
                            UpdateCollectionList();
                        }
                    });
                }
            };
            button.tooltip = "Deletes the selected config.";

            // Panels
            m_collectionPanel = AddUIComponent<UIPanel>();
            m_collectionPanel.relativePosition = new Vector3(10, verticalOffset + 60);
            m_collectionPanel.width = (WIDTH - 25) / 2;
            m_collectionPanel.height = HEIGHT - verticalOffset - 70;
            //vehiclePanel.backgroundSprite = "UnlockingPanel";
            label = AddUIComponent<UILabel>();
            label.text = "Settings";
            label.relativePosition = m_collectionPanel.relativePosition + new Vector3(0, -20);

            m_trailerPanel = AddUIComponent<UIPanel>();
            m_trailerPanel.relativePosition = new Vector3(m_collectionPanel.relativePosition.x + m_collectionPanel.width + 5, verticalOffset + 60);
            m_trailerPanel.width = (WIDTH - 25) / 2;
            m_trailerPanel.height = m_collectionPanel.height;
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
                    if(m_selectedCollection != null)
                    {
                        m_selectedCollection.Trailers.Add(new Trailer()
                        {
                            AssetName = data.info.name,
                            IsCollection = false,
                        });
                        UpdatePanels();
                    }
                },
                UIFindAssetPanel.DisplayMode.Trailers);
            };

            m_addMultiTrailer = UIUtils.CreateButton(m_trailerPanel);
            m_addMultiTrailer.text = "Add multi";
            m_addMultiTrailer.relativePosition = new Vector3(m_addTrailer.relativePosition.x + m_addTrailer.width + 10, m_trailerPanel.height - m_addMultiTrailer.height);
            m_addMultiTrailer.eventClicked += (c, m) =>
            {
                // Show panel and add multi trailer based on the single trailer
                UIFindAssetPanel.main.Show((data) =>
                {
                    if(m_selectedCollection != null)
                    {
                        m_selectedCollection.Trailers.Add(new Trailer()
                        {
                            // Is multi trailer because it has a subtrailer
                            AssetName = "New Multi Trailer",
                            IsCollection = false,
                            SubTrailers = new List<Trailer>() {
                                new Trailer(data.info)
                            }
                        });
                        UpdatePanels();
                    }
                },
                UIFindAssetPanel.DisplayMode.Trailers);
            };
            m_addMultiTrailer.tooltip = "Adds a new Multi Trailer with the trailer you select as the first subtrailer.";

            // fastlist
            m_trailerFastList = UIFastList.Create<UITrailerRow>(m_trailerPanel);
            m_trailerFastList.backgroundSprite = "UnlockingPanel";
            m_trailerFastList.width = m_trailerPanel.width;
            m_trailerFastList.height = m_trailerPanel.height - 35;
            m_trailerFastList.canSelect = true;
            m_trailerFastList.relativePosition = Vector3.zero;

            // Settings
            float y = 0;
            float padding = 10;
            label = m_collectionPanel.AddUIComponent<UILabel>();
            label.text = "Name";
            label.relativePosition = new Vector3(0, y);

            m_nameField = UIUtils.CreateTextField(m_collectionPanel);
            m_nameField.relativePosition = new Vector3(label.width + 5, y);
            m_nameField.width = m_collectionPanel.width - m_nameField.relativePosition.x - 5;
            m_nameField.eventTextChanged += (c, text) =>
            {
                if(m_selectedCollection != null)
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
            m_nameField.eventTextSubmitted += (c, text) => {
                UpdateCurrentCollectionName(text);
            };
            y += padding + m_nameField.height;

            // No vehicle text
            m_labelNoCol = AddUIComponent<UILabel>();
            m_labelNoCol.textScale = 1.5f;
            m_labelNoCol.text = "Press Add New to add a new trailer collection";
            m_labelNoCol.relativePosition = new Vector3((WIDTH - m_labelNoCol.width) / 2, HEIGHT / 2);
        }

        private void UpdateCurrentCollectionName(string name)
        {
            var def = UIMainPanel.main.UserDefinition;
            if(def != null && m_selectedCollection != null)
            {
                foreach(var vehicle in def.Vehicles)
                {
                    foreach(var trailer in vehicle.Trailers)
                    {
                        if(trailer.IsCollection && trailer.AssetName == m_selectedCollection.Name)
                        {
                            trailer.AssetName = name;
                        }
                    }
                }

                m_selectedCollection.Name = name;

                UIMainPanel.main.UpdatePanels();
                UpdateCollectionList();
            }
        }

        private void VehicleDropdown_eventSelectedIndexChanged(UIComponent component, int value)
        {
            m_selectedCollection = null;
            if(UIMainPanel.main.UserDefinition != null)
            {
                if(value >= 0 && value < UIMainPanel.main.UserDefinition.Collections.Count)
                {
                    m_selectedCollection = UIMainPanel.main.UserDefinition.Collections[value];
                }
            }
            m_collectionDropdown.tooltip = m_selectedCollection != null ? m_selectedCollection.Name : "???";
            UpdatePanels();
        }

        /// <summary>
        /// Removes trailer from currently selected vehicle.
        /// </summary>
        /// <param name="trailerDef"></param>
        /// <returns></returns>
        public bool RemoveTrailer(Trailer trailerDef)
        {
            if(m_selectedCollection != null)
            {
                var b = m_selectedCollection.Trailers.Remove(trailerDef);
                UpdatePanels();
                return b;
            }
            return false;
        }

        public void UpdateCollectionList()
        {
            m_collectionDropdown.items = new string[0];
            if(UIMainPanel.main.UserDefinition != null)
            {
                foreach(var collection in UIMainPanel.main.UserDefinition.Collections)
                {
                    m_collectionDropdown.AddItem(collection.Name);
                }
            }
            m_collectionDropdown.selectedIndex = 0;
            VehicleDropdown_eventSelectedIndexChanged(null, 0);
            UpdatePanels();
        }

        public void DisplayLast()
        {
            m_collectionDropdown.selectedIndex = m_collectionDropdown.items.Length - 1;
        }

        public void UpdatePanels()
        {
            //Util.Log("Selected index for fastlist: " + m_collectionDropdown.selectedIndex);
            //Util.Log("selectedTrainData: " + (selectedTrainData != null ? selectedTrainData.name : "NULL"));
            FastList<object> newRowsData = new FastList<object>();

            if(m_selectedCollection != null)
            {
                m_labelNoCol.isVisible = false;
                m_collectionPanel.isVisible = true;
                m_trailerPanel.isVisible = true;

                foreach(var trailer in m_selectedCollection.Trailers)
                {
                    newRowsData.Add(trailer);
                }

                m_nameField.text = m_selectedCollection.Name;
            }
            else
            {
                m_labelNoCol.isVisible = true;
                m_collectionPanel.isVisible = false;
                m_trailerPanel.isVisible = false;
            }

            m_trailerFastList.rowHeight = UITrailerRow.HEIGHT;
            m_trailerFastList.rowsData = newRowsData;
        }

        public new void Show()
        {
            base.Show(true);
            UpdateCollectionList();
        }
    }
}
