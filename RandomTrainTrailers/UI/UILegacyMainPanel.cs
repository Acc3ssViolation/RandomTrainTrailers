using ColossalFramework.UI;
using RandomTrainTrailers.Definition;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RandomTrainTrailers.UI
{
    /*
     *  Main UI panel of the mod.
     *  I didn't know anything about UI, so thanks to SamsamTS for putting your stuff up on Github for others to see and learn from.
     */
    public class UILegacyMainPanel : UIWindowPanel
    {
        public static UILegacyMainPanel Main { get; private set; }

        private UIDropDown vehicleDropdown;
        private UIFastList trailerFastList;
        private UIFastList blacklistFastList;
        private UIPanel vehiclePanel;
        private UIPanel trailerPanel;
        private UIPanel blacklistPanel;

        private UILabel m_labelNoVehicles;

        // Widths
        public int TrailerRowWidth { get; private set; }
        public int BlacklistRowWidth { get; private set; }

        // vehicle options
        private UIIntField m_randomChance;
        private UIIntField m_startOffset;
        private UIIntField m_endOffset;
        private UICheckBox m_useTrailerOverride;
        private UIIntField m_trailerMin;
        private UIIntField m_trailerMax;
        private UICheckBox m_useDefault;
        private UICheckBox m_useCargo;
        private UIButton m_copyButton;
        private UIButton m_pastButton;

        // Trailer add buttons
        private UIButton m_addTrailer;
        private UIButton m_addCollection;
        private UIButton m_addMultiTrailer;
        private UIButton m_addBlacklist;

        public const int HEIGHT = 500;
        public const int WIDTH = 770;

        public TrailerDefinition UserDefinition
        {
            get
            {
                return m_userDefinition;
            }
        }

        public override float DefaultWidth => WIDTH + 20;
        public override float DefaultHeight => HEIGHT + 60;
        public override string DefaultTitle => "Random Train Trailers";

        public TrailerDefinition m_userDefinition;
        public Definition.Vehicle m_selectedVehicleData;

        private Definition.Vehicle m_copyData;
        private TrailerImporter _trailerImporter = new TrailerImporter();

        public void SetData(TrailerDefinition trailerDefinition)
        {
            m_userDefinition = trailerDefinition;
            UpdateTrainList();
        }

        public override void Start()
        {
            base.Start();
            Main = this;

            // Didn't even realize there was still a reference to the old Coupled Trains mod in here, I'm keeping it :3
            name = "CoupledTrainsOptionsPanel";

            // Add window for adding new sets
            var go = new GameObject("RTTCollectionsPanel");
            go.transform.parent = this.gameObject.transform;
            go.AddComponent<UICollectionsPanel>();

            go = new GameObject("RTTMultiTrailerPanel");
            go.transform.parent = this.gameObject.transform;
            go.AddComponent<UIMultiTrailerPanel>();

            // Create our own components
            CreateComponents();
        }

        private void CreateComponents()
        {
            float verticalOffset = 0;
            // dropdown
            var label = AddUIComponent<UILabel>();
            label.text = "Train consist: ";
            label.textScale = 0.8f;
            label.padding = new RectOffset(0, 0, 8, 0);
            label.relativePosition = new Vector3(10f, verticalOffset);

            vehicleDropdown = UIUtils.CreateDropDown(this);
            vehicleDropdown.relativePosition = new Vector3(115f, verticalOffset);
            vehicleDropdown.width = 300f;
            vehicleDropdown.eventSelectedIndexChanged += VehicleDropdown_eventSelectedIndexChanged;
            vehicleDropdown.tooltip = "???";

            // new button
            UIButton button = UIUtils.CreateButton(this);
            button.text = "Add new";
            button.relativePosition = new Vector3(425f, verticalOffset);
            button.eventClicked += (c, p) =>
            {
                UIFindAssetPanel.Main.Content.Show((data) => {
                    

                    if(m_userDefinition != null)
                    {
                        if(m_userDefinition.Vehicles.Where((vehicle) => {
                            if(vehicle.GetInfo() == data.info)
                                return true;
                            return false;
                        }).Count() > 0)
                        {
                            // NOPE NOPE NOPE NOPE?!
                            Util.ShowWarningMessage("Definition for info " + data.info.name + " already exists!");
                        }
                        else
                        {
                            // Add it
                            m_userDefinition.Vehicles.Add(new Definition.Vehicle()
                            {
                                AssetName = data.info.name,
                            });
                            UpdateTrainList();
                            DisplayLast();
                        }
                    }


                }, UIFindAssetPanel.DisplayMode.Engines);
            };
            button.tooltip = "Adds a new config for a vehicle.";

            // delete button
            button = UIUtils.CreateButton(this);
            button.text = "Delete";
            button.relativePosition = new Vector3(520f, verticalOffset);
            button.eventClicked += (c, p) =>
            {
                if(m_selectedVehicleData != null && m_userDefinition != null)
                {
                    ConfirmPanel.ShowModal(Mod.name, "Are you sure you want to delete the config for " + m_selectedVehicleData.AssetName + "?", delegate (UIComponent comp, int ret)
                    {
                        if(ret == 1)
                        {
                            m_userDefinition.Vehicles.Remove(m_selectedVehicleData);
                            UpdateTrainList();
                        }
                    });
                }
            };
            button.tooltip = "Deletes the selected config.";

            // edit collections button
            button = UIUtils.CreateButton(this);
            button.text = "Edit Collections";
            button.relativePosition = new Vector3(625f, verticalOffset);
            button.width = WIDTH - button.relativePosition.x - 10;
            button.eventClicked += (c, p) =>
            {
                UICollectionsPanel.main.Show();
            };
            button.tooltip = "Opens the collections screen.";

            // Panels
            vehiclePanel = AddUIComponent<UIPanel>();
            vehiclePanel.relativePosition = new Vector3(10, verticalOffset + 60);
            vehiclePanel.width = (WIDTH - 30) / 3;
            vehiclePanel.height = HEIGHT - verticalOffset - 70;
            //vehiclePanel.backgroundSprite = "UnlockingPanel";
            label = AddUIComponent<UILabel>();
            label.text = "Settings";
            label.relativePosition = vehiclePanel.relativePosition + new Vector3(0, -20);

            trailerPanel = AddUIComponent<UIPanel>();
            trailerPanel.relativePosition = new Vector3(vehiclePanel.relativePosition.x + vehiclePanel.width + 5, verticalOffset + 60);
            trailerPanel.width = (WIDTH - 30) / 3;
            trailerPanel.height = vehiclePanel.height;
            //trailerPanel.backgroundSprite = "UnlockingItemBackground";
            TrailerRowWidth = (int)trailerPanel.width - 20;
            label = AddUIComponent<UILabel>();
            label.text = "Trailers";
            label.relativePosition = trailerPanel.relativePosition + new Vector3(0, -20);

            m_addTrailer = UIUtils.CreateButton(trailerPanel);
            m_addTrailer.text = "Add trailer";
            m_addTrailer.relativePosition = new Vector3(0, trailerPanel.height - m_addTrailer.height);
            m_addTrailer.eventClicked += (c, m) =>
            {
                // Show panel and add trailer maybe idk
                UIFindAssetPanel.Main.Content.Show((data) =>
                {
                    if(m_selectedVehicleData != null)
                    {
                        m_selectedVehicleData.Trailers.Add(_trailerImporter.ImportFromAsset(data.info));
                        UpdatePanels();
                    }
                },
                UIFindAssetPanel.DisplayMode.Trailers);
            };

            m_addCollection = UIUtils.CreateButton(trailerPanel);
            m_addCollection.text = "Add collection";
            m_addCollection.width += 40;
            m_addCollection.relativePosition = new Vector3(m_addTrailer.relativePosition.x + m_addTrailer.width + 10, trailerPanel.height - m_addCollection.height);
            m_addCollection.eventClicked += (c, m) =>
            {
                // Show panel and add collection maybe idk
                UIFindAssetPanel.Main.Content.Show((data) =>
                {
                    if(m_selectedVehicleData != null)
                    {
                        m_selectedVehicleData.Trailers.Add(new Trailer()
                        {
                            AssetName = data.localeName,
                            IsCollection = true,
                        });
                        UpdatePanels();
                    }
                },
                UIFindAssetPanel.DisplayMode.Collections);
            };

            m_addMultiTrailer = UIUtils.CreateButton(trailerPanel);
            m_addMultiTrailer.text = "Add multi";
            m_addMultiTrailer.relativePosition = new Vector3(0, m_addTrailer.relativePosition.y - 5 - m_addMultiTrailer.height);
            m_addMultiTrailer.eventClicked += (c, m) =>
            {
                // Show panel and add multi trailer based on the single trailer
                UIFindAssetPanel.Main.Content.Show((data) =>
                {
                    if(m_selectedVehicleData != null)
                    {
                        var subTrailer = _trailerImporter.ImportFromAsset(data.info);
                        m_selectedVehicleData.Trailers.Add(new Trailer()
                        {
                            // Is multi trailer because it has a subtrailer
                            AssetName = "New Multi Trailer",
                            IsCollection = false,
                            SubTrailers = new List<Trailer>() {
                                subTrailer,
                            },
                            CargoType = subTrailer.CargoType,
                            InvertProbability = subTrailer.InvertProbability,
                        });
                        UpdatePanels();
                    }
                },
                UIFindAssetPanel.DisplayMode.Trailers);
            };
            m_addMultiTrailer.tooltip = "Adds a new Multi Trailer with the trailer you select as the first subtrailer.";

            blacklistPanel = AddUIComponent<UIPanel>();
            blacklistPanel.relativePosition = new Vector3(trailerPanel.relativePosition.x + trailerPanel.width + 5, verticalOffset + 60);
            blacklistPanel.width = (WIDTH - 30) / 3;
            blacklistPanel.height = vehiclePanel.height;
            //blacklistPanel.backgroundSprite = "UnlockingPanel";
            BlacklistRowWidth = (int)blacklistPanel.width - 20;
            label = AddUIComponent<UILabel>();
            label.text = "Blacklist";
            label.relativePosition = blacklistPanel.relativePosition + new Vector3(0, -20);

            m_addBlacklist = UIUtils.CreateButton(blacklistPanel);
            m_addBlacklist.text = "Add item";
            m_addBlacklist.relativePosition = new Vector3(0, blacklistPanel.height - m_addBlacklist.height);
            m_addBlacklist.eventClicked += (c, m) =>
            {
                // Show panel and add blacklist item
                UIFindAssetPanel.Main.Content.Show((data) =>
                {
                    if(m_selectedVehicleData != null)
                    {
                        m_selectedVehicleData.LocalBlacklist.Add(new BlacklistItem()
                        {
                            AssetName = data.info.name,
                        });
                        UpdatePanels();
                    }
                },
                UIFindAssetPanel.DisplayMode.Trailers);
            };

            // fastlist
            trailerFastList = UIFastList.Create<UILegacyTrailerRow>(trailerPanel);
            trailerFastList.backgroundSprite = "UnlockingPanel";
            trailerFastList.width = trailerPanel.width;
            trailerFastList.height = trailerPanel.height - 35 - 35;
            trailerFastList.canSelect = true;
            trailerFastList.relativePosition = Vector3.zero;

            // fastlist blacklist
            blacklistFastList = UIFastList.Create<UIBlacklistRow>(blacklistPanel);
            blacklistFastList.backgroundSprite = "UnlockingPanel";
            blacklistFastList.width = trailerPanel.width;
            blacklistFastList.height = trailerPanel.height - 35;
            blacklistFastList.canSelect = true;
            blacklistFastList.relativePosition = Vector3.zero;

            // Settings
            float y = 0;
            float padding = 10;
            m_randomChance = UIIntField.CreateField("Chance (0-100):", vehiclePanel, false);
            m_randomChance.panel.relativePosition = new Vector3(0, y);
            m_randomChance.textField.eventTextChanged += (c, text) =>
            {
                if(m_selectedVehicleData != null)
                {
                    var i = m_selectedVehicleData.RandomTrailerChance;
                    m_randomChance.IntFieldHandler(ref i);
                    m_selectedVehicleData.RandomTrailerChance = i;
                }
            };
            y += padding + m_randomChance.panel.height;

            m_startOffset = UIIntField.CreateField("Start offset:", vehiclePanel, false);
            m_startOffset.panel.relativePosition = new Vector3(0, y);
            m_startOffset.textField.eventTextChanged += (c, text) =>
            {
                if(m_selectedVehicleData != null)
                {
                    var i = m_selectedVehicleData.StartOffset;
                    m_startOffset.IntFieldHandler(ref i);
                    m_selectedVehicleData.StartOffset = i;
                }
            };
            y += padding + m_startOffset.panel.height;

            m_endOffset = UIIntField.CreateField("End offset:", vehiclePanel, false);
            m_endOffset.panel.relativePosition = new Vector3(0, y);
            m_endOffset.textField.eventTextChanged += (c, text) =>
            {
                if(m_selectedVehicleData != null)
                {
                    var i = m_selectedVehicleData.EndOffset;
                    m_endOffset.IntFieldHandler(ref i);
                    m_selectedVehicleData.EndOffset = i;
                }
            };
            y += padding + m_endOffset.panel.height;

            m_useTrailerOverride = UIUtils.CreateCheckBox(vehiclePanel);
            m_useTrailerOverride.text = "Override trailer count";
            m_useTrailerOverride.relativePosition = new Vector3(0, y);
            m_useTrailerOverride.eventCheckChanged += (c, value) => {

                if(m_selectedVehicleData != null)
                {
                    if(value)
                    {
                        if(m_selectedVehicleData.TrailerCountOverride != null)
                        {
                            m_trailerMin.SetValue(m_selectedVehicleData._TrailerCountOverrideMin);
                            m_trailerMax.SetValue(m_selectedVehicleData._TrailerCountOverrideMax);
                        }
                        else
                        {
                            m_trailerMin.SetValue(0);
                            m_trailerMax.SetValue(0);
                        }
                    }
                    else
                    {
                        m_trailerMin.SetValue(-1);
                        m_trailerMax.SetValue(-1);
                        m_selectedVehicleData.TrailerCountOverride = null;
                    }
                }
            };
            y += padding + m_useTrailerOverride.height;

            m_trailerMin = UIIntField.CreateField("Min trailer count:", vehiclePanel, false);
            m_trailerMin.panel.relativePosition = new Vector3(0, y);
            m_trailerMin.textField.eventTextChanged += (c, text) =>
            {
                if(m_selectedVehicleData != null && m_useTrailerOverride.isChecked)
                {
                    var i = m_selectedVehicleData._TrailerCountOverrideMin;
                    m_trailerMin.IntFieldHandler(ref i);
                    m_selectedVehicleData._TrailerCountOverrideMin = i;

                    if(m_selectedVehicleData._TrailerCountOverrideMin < -1)
                    {
                        m_selectedVehicleData._TrailerCountOverrideMin = -1;
                        m_trailerMin.SetValue(m_selectedVehicleData._TrailerCountOverrideMin);
                        m_trailerMax.SetValue(m_selectedVehicleData._TrailerCountOverrideMax);
                    }
                    if(m_selectedVehicleData._TrailerCountOverrideMax < m_selectedVehicleData._TrailerCountOverrideMin)
                    {
                        m_selectedVehicleData._TrailerCountOverrideMax = m_selectedVehicleData._TrailerCountOverrideMin;
                        m_trailerMax.SetValue(m_selectedVehicleData._TrailerCountOverrideMax);
                    }
                }
                else
                {
                    m_trailerMin.textField.text = "-1";
                }
            };
            y += padding + m_trailerMin.panel.height;

            m_trailerMax = UIIntField.CreateField("Max trailer count:", vehiclePanel, false);
            m_trailerMax.panel.relativePosition = new Vector3(0, y);
            m_trailerMax.textField.eventTextChanged += (c, text) =>
            {
                if(m_selectedVehicleData != null && m_useTrailerOverride.isChecked)
                {
                    var i = m_selectedVehicleData._TrailerCountOverrideMax;
                    m_trailerMax.IntFieldHandler(ref i);
                    m_selectedVehicleData._TrailerCountOverrideMax = i;

                    if(m_selectedVehicleData._TrailerCountOverrideMin < -1)
                    {
                        m_selectedVehicleData._TrailerCountOverrideMin = -1;
                        m_trailerMin.SetValue(m_selectedVehicleData._TrailerCountOverrideMin);
                        m_trailerMax.SetValue(m_selectedVehicleData._TrailerCountOverrideMax);
                    }
                    if(m_selectedVehicleData._TrailerCountOverrideMax < m_selectedVehicleData._TrailerCountOverrideMin)
                    {
                        m_selectedVehicleData._TrailerCountOverrideMax = m_selectedVehicleData._TrailerCountOverrideMin;
                        m_trailerMax.SetValue(m_selectedVehicleData._TrailerCountOverrideMax);
                    }
                }
                else
                {
                    m_trailerMax.textField.text = "-1";
                }
            };
            y += padding + m_trailerMax.panel.height;

            m_useDefault = UIUtils.CreateCheckBox(vehiclePanel);
            m_useDefault.text = "Use default trailers as well";
            m_useDefault.relativePosition = new Vector3(0, y);
            m_useDefault.eventCheckChanged += (c, value) => {

                if(m_selectedVehicleData != null)
                {
                    m_selectedVehicleData.AllowDefaultTrailers = value;
                }
            };
            y += padding + m_useDefault.height;

            m_useCargo = UIUtils.CreateCheckBox(vehiclePanel);
            m_useCargo.text = "Cargo changes trailers";
            m_useCargo.relativePosition = new Vector3(0, y);
            m_useCargo.eventCheckChanged += (c, value) => {

                if(m_selectedVehicleData != null)
                {
                    m_selectedVehicleData.UseCargoContents = value;
                }
            };
            y += padding + m_useCargo.height;

            // Copy/paste
            m_copyButton = UIUtils.CreateButton(vehiclePanel);
            m_copyButton.text = "Copy";
            m_copyButton.relativePosition = new Vector3(0, y);
            m_copyButton.eventClicked += (c, m) =>
            {
                CopySettings();
            };
            m_copyButton.tooltip = "Copy the settings so you can paste them on another vehicle.";

            m_pastButton = UIUtils.CreateButton(vehiclePanel);
            m_pastButton.text = "Paste";
            m_pastButton.relativePosition = new Vector3(m_copyButton.width + padding, y);
            m_pastButton.eventClicked += (c, m) =>
            {
                PasteSettings();
            };
            m_pastButton.tooltip = "Paste the settings previously copied from another vehicle.";

            y += padding + m_copyButton.height;

            // No vehicle text
            m_labelNoVehicles = AddUIComponent<UILabel>();
            m_labelNoVehicles.textScale = 1.5f;
            m_labelNoVehicles.text = "Press Add New to add a new Random Train Trailers Definition";
            m_labelNoVehicles.relativePosition = new Vector3((WIDTH - m_labelNoVehicles.width) / 2, HEIGHT / 2);

            UpdateTrainList();
        }

        private void VehicleDropdown_eventSelectedIndexChanged(UIComponent component, int value)
        {
            m_selectedVehicleData = null;
            if(m_userDefinition != null)
            {
                if(value >= 0 && value < m_userDefinition.Vehicles.Count)
                {
                    m_selectedVehicleData = m_userDefinition.Vehicles[value];
                }
            }
            vehicleDropdown.tooltip = m_selectedVehicleData != null ? m_selectedVehicleData.AssetName : "???";
            UpdatePanels();
        }

        private void CopySettings()
        {
            if (m_selectedVehicleData == null)
                return;
            m_copyData = m_selectedVehicleData.Copy();
            UpdatePanels();
        }

        private void PasteSettings()
        {
            if (m_copyData == null)
                return;

            // TODO: Vehicle type is always Unknown
            if (m_copyData.VehicleType != m_selectedVehicleData.VehicleType)
                return;

            m_copyData.AssetName = m_selectedVehicleData.AssetName;
            m_selectedVehicleData.CopyFrom(m_copyData);
            UpdatePanels();
        }

        /// <summary>
        /// Removes trailer from currently selected vehicle.
        /// </summary>
        /// <param name="trailerDef"></param>
        /// <returns></returns>
        public bool RemoveTrailer(Trailer trailerDef)
        {
            if(m_selectedVehicleData != null)
            {
                var b = m_selectedVehicleData.Trailers.Remove(trailerDef);
                if(!b)
                {
                    b = UICollectionsPanel.main.RemoveTrailer(trailerDef);
                    if(b)
                    {
                        UICollectionsPanel.main.UpdatePanels();
                    }
                }
                else
                {
                    UpdatePanels();
                }
                return b;
            }
            return false;
        }

        /// <summary>
        /// Removes trailer from currently selected vehicle.
        /// </summary>
        /// <param name="trailerDef"></param>
        /// <returns></returns>
        public bool RemoveBlacklist(BlacklistItem itemDef)
        {
            if(m_selectedVehicleData != null)
            {
                var b = m_selectedVehicleData.LocalBlacklist.Remove(itemDef);
                UpdatePanels();
                return b;
            }
            return false;
        }

        public void UpdateTrainList()
        {
            if (vehicleDropdown == null)
                return;

            vehicleDropdown.items = new string[0];
            if(m_userDefinition != null)
            {
                foreach(var vehicle in m_userDefinition.Vehicles)
                {
                    vehicleDropdown.AddItem(Util.GetVehicleDisplayName(vehicle.AssetName));
                }
            }
            vehicleDropdown.selectedIndex = 0;
            VehicleDropdown_eventSelectedIndexChanged(null, 0);
            UpdatePanels();
        }

        public void DisplayLast()
        {
            vehicleDropdown.selectedIndex = vehicleDropdown.items.Length - 1;
        }

        public void UpdatePanels()
        {
           // Util.Log("Selected index for fastlist: " + vehicleDropdown.selectedIndex);
            //Util.Log("selectedTrainData: " + (selectedTrainData != null ? selectedTrainData.name : "NULL"));
            FastList<object> newRowsData = new FastList<object>();
            FastList<object> newRowsData2 = new FastList<object>();

            if(m_selectedVehicleData != null)
            {
                m_labelNoVehicles.isVisible = false;
                vehiclePanel.isVisible = true;
                blacklistPanel.isVisible = true;
                trailerPanel.isVisible = true;

                foreach(var trailer in m_selectedVehicleData.Trailers)
                {
                    newRowsData.Add(trailer);
                }

                foreach(var item in m_selectedVehicleData.LocalBlacklist)
                {
                    newRowsData2.Add(item);
                }

                m_randomChance.SetValue(m_selectedVehicleData.RandomTrailerChance);
                m_startOffset.SetValue(m_selectedVehicleData.StartOffset);
                m_endOffset.SetValue(m_selectedVehicleData.EndOffset);
                m_useTrailerOverride.isChecked = m_selectedVehicleData.TrailerCountOverride != null;
                m_trailerMin.SetValue(m_selectedVehicleData._TrailerCountOverrideMin);
                m_trailerMax.SetValue(m_selectedVehicleData._TrailerCountOverrideMax);
                m_useDefault.isChecked = m_selectedVehicleData.AllowDefaultTrailers;
                var info = m_selectedVehicleData.GetInfo();
                var isCargoTrain = info != null ? info.GetAI() is CargoTrainAI : true;
                m_useCargo.isVisible = isCargoTrain;
                m_useCargo.isChecked = m_selectedVehicleData.UseCargoContents && isCargoTrain;

                var allowPaste = m_copyData != null && m_copyData.VehicleType == m_selectedVehicleData.VehicleType;
                m_pastButton.enabled = allowPaste;
            }
            else
            {
                m_labelNoVehicles.isVisible = true;
                vehiclePanel.isVisible = false;
                blacklistPanel.isVisible = false;
                trailerPanel.isVisible = false;
            }

            trailerFastList.rowHeight = UILegacyTrailerRow.HEIGHT;
            trailerFastList.rowsData = newRowsData;

            blacklistFastList.rowHeight = UIBlacklistRow.HEIGHT;
            blacklistFastList.rowsData = newRowsData2;
        }
    }
}
