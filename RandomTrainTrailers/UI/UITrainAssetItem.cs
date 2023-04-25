
#if false
namespace RandomTrainTrailers.UI
{

    public class UITrainAssetItem : UIPanel, IUIFastListRow
    {
        public class Data
        {
            public int prefabIndex;
            //public TrainConsist trainData;
        }

        private UIMainPanel mainPanel;

        private UIButton buttonRemove;
        private UIButton buttonUp;
        private UIButton buttonDown;
        private UIPanel settingsPanel;
        private UIDropDown assetDropdown;
        private UICheckBox checkboxBackEngine;
        private UITextField fieldFoF;
        private UITextField fieldFoB;
        private UITextField fieldBoF;
        private UITextField fieldBoB;

        private UIButton buttonAdd;

        public const int HEIGHT = 100;

        private Data itemData;
        private bool checkEvents = false;

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

            mainPanel = UIMainPanel.main;

            settingsPanel = AddUIComponent<UIPanel>();
            settingsPanel.relativePosition = Vector3.zero;
            UIHelper settingsPanelHelper = new UIHelper(settingsPanel);

            // Dropdown for prefab selection
            assetDropdown = UIUtils.CreateDropDown(this);
            assetDropdown.width = 250;
            assetDropdown.relativePosition = new Vector3(10, 10);
            assetDropdown.eventSelectedIndexChanged += (c, i) => {
                if(!checkEvents || itemData == null) { return; }

                /*var list = VehiclePrefabs.GetPrefabs(itemData.trainData.aiType);
                if(i >= 0 && i < list.Length)
                {
                    Util.Log("Changing " + itemData.trainData.name + " prefab " + itemData.trainData.prefabs[itemData.prefabIndex].name + " with " + list[i].name + " at index " + itemData.prefabIndex);
                    itemData.trainData.prefabs[itemData.prefabIndex] = new TrainConsist.Prefab(list[i]);
                }*/
            };
            assetDropdown.tooltip = UIStringCollection.TooltipDropdownPrefab;

            // Button for adding new prefab
            buttonAdd = UIUtils.CreateButton(this);
            buttonAdd.width = 170;
            buttonAdd.text = "Add new asset";
            buttonAdd.eventClicked += (c, p) => {
                if(!checkEvents) { return; }

                /*var list = VehiclePrefabs.GetPrefabs(itemData.trainData.aiType);
                if(assetDropdown.selectedIndex >= 0 && assetDropdown.selectedIndex < list.Length)
                {
                    //Util.Log("Adding " + itemData.trainData.name + " prefab " + list[assetDropdown.selectedIndex].name + " at last");
                    //itemData.trainData.prefabs.Add(new TrainConsist.Prefab(list[assetDropdown.selectedIndex]));

                    //Update UI
                    mainPanel.UpdateFastList();
                }*/
            };
            buttonAdd.relativePosition = new Vector3(400, 10);
            buttonAdd.tooltip = UIStringCollection.TooltipButtonAddPrefab;

            // Checkbox for back engine
            checkboxBackEngine = (UICheckBox)settingsPanelHelper.AddCheckbox("Back Engine", false, (b) => {
                if(!checkEvents) { return; }

                /*if(b != itemData.trainData.prefabs[itemData.prefabIndex].useBackEngine)
                {
                    if(itemData.prefabIndex >= 0 && itemData.prefabIndex < itemData.trainData.prefabs.Count)
                    {
                        Util.Log("Setting " + itemData.trainData.name + " prefab " + itemData.trainData.prefabs[itemData.prefabIndex].name + " to " + (b ? "" : "not") + " have a back engine");
                        itemData.trainData.prefabs[itemData.prefabIndex].useBackEngine = b;
                    }
                }*/
            });
            checkboxBackEngine.relativePosition = new Vector3(270, 14);
            checkboxBackEngine.width = 125;
            checkboxBackEngine.tooltip = UIStringCollection.TooltipCheckboxBackEngine;

            // Offset labels main
            UILabel label = settingsPanel.AddUIComponent<UILabel>();
            label.textScale = 0.8f;
            label.text = "Engine Offsets";
            label.relativePosition = new Vector3(10, 50);

            label = settingsPanel.AddUIComponent<UILabel>();
            label.textScale = 0.8f;
            label.text = "Rear Offsets";
            label.relativePosition = new Vector3(10, 80);

            // Offset fields
            label = settingsPanel.AddUIComponent<UILabel>();
            label.textScale = 0.8f;
            label.text = "Front:";
            label.relativePosition = new Vector3(115, 50);
            fieldFoF = UIUtils.CreateTextField(settingsPanel);
            fieldFoF.relativePosition = new Vector3(160, 45);
            fieldFoF.eventTextChanged += (c, t) => {
                if(!checkEvents) { return; }

                float v;
                if(float.TryParse(t, out v))
                {
                    /*if(itemData.prefabIndex >= 0 && itemData.prefabIndex < itemData.trainData.prefabs.Count)
                    {
                        Util.Log("Setting " + itemData.trainData.name + " prefab " + itemData.trainData.prefabs[itemData.prefabIndex].name + " front offset front to " + v);
                        itemData.trainData.prefabs[itemData.prefabIndex].frontInfoOffsetFront = v;
                    }*/
                    fieldFoF.color = Color.white;
                }
                else
                {
                    fieldFoF.color = Color.red;
                }
            };
            fieldFoF.tooltip = UIStringCollection.TooltipFieldOffsetFront;

            label = settingsPanel.AddUIComponent<UILabel>();
            label.textScale = 0.8f;
            label.text = "Back:";
            label.relativePosition = new Vector3(255, 50);
            fieldFoB = UIUtils.CreateTextField(settingsPanel);
            fieldFoB.relativePosition = new Vector3(295, 45);
            fieldFoB.eventTextChanged += (c, t) => {
                if(!checkEvents) { return; }

                float v;
                if(float.TryParse(t, out v))
                {
                    /*if(itemData.prefabIndex >= 0 && itemData.prefabIndex < itemData.trainData.prefabs.Count)
                    {
                        Util.Log("Setting " + itemData.trainData.name + " prefab " + itemData.trainData.prefabs[itemData.prefabIndex].name + " front offset back to " + v);
                        itemData.trainData.prefabs[itemData.prefabIndex].frontInfoOffsetBack = v;
                    }*/
                    fieldFoB.color = Color.white;
                }
                else
                {
                    fieldFoB.color = Color.red;
                }
            };
            fieldFoB.tooltip = UIStringCollection.TooltipFieldOffsetBack;

            label = settingsPanel.AddUIComponent<UILabel>();
            label.textScale = 0.8f;
            label.text = "Front:";
            label.relativePosition = new Vector3(115, 80);
            fieldBoF = UIUtils.CreateTextField(settingsPanel);
            fieldBoF.relativePosition = new Vector3(160, 75);
            fieldBoF.eventTextChanged += (c, t) => {
                if(!checkEvents) { return; }

                float v;
                if(float.TryParse(t, out v))
                {
                    /*if(itemData.prefabIndex >= 0 && itemData.prefabIndex < itemData.trainData.prefabs.Count)
                    {
                        Util.Log("Setting " + itemData.trainData.name + " prefab " + itemData.trainData.prefabs[itemData.prefabIndex].name + " back offset front to " + v);
                        itemData.trainData.prefabs[itemData.prefabIndex].backInfoOffsetFront = v;
                    }*/
                    fieldBoF.color = Color.white;
                }
                else
                {
                    fieldBoF.color = Color.red;
                }
            };
            fieldBoF.tooltip = UIStringCollection.TooltipFieldOffsetFront;

            label = settingsPanel.AddUIComponent<UILabel>();
            label.textScale = 0.8f;
            label.text = "Back:";
            label.relativePosition = new Vector3(255, 80);
            fieldBoB = UIUtils.CreateTextField(settingsPanel);
            fieldBoB.relativePosition = new Vector3(295, 75);
            fieldBoB.eventTextChanged += (c, t) => {
                if(!checkEvents) { return; }

                float v;
                if(float.TryParse(t, out v))
                {
                    /*if(itemData.prefabIndex >= 0 && itemData.prefabIndex < itemData.trainData.prefabs.Count)
                    {
                        Util.Log("Setting " + itemData.trainData.name + " prefab " + itemData.trainData.prefabs[itemData.prefabIndex].name + " back offset back to " + v);
                        itemData.trainData.prefabs[itemData.prefabIndex].backInfoOffsetBack = v;
                    }*/
                    fieldBoB.color = Color.white;
                }
                else
                {
                    fieldBoB.color = Color.red;
                }
            };
            fieldBoB.tooltip = UIStringCollection.TooltipFieldOffsetBack;

            // Button for removing prefab
            buttonRemove = UIUtils.CreateButton(settingsPanel);
            buttonRemove.size = new Vector2(30, 30);
            buttonRemove.normalBgSprite = "buttonclose";
            buttonRemove.hoveredBgSprite = "buttonclosehover";
            buttonRemove.pressedBgSprite = "buttonclosepressed";
            buttonRemove.relativePosition = new Vector3(UIMainPanel.WIDTH - 80, 35);
            buttonRemove.eventClicked += (c, p) => {
                if(!checkEvents) { return; }

                /*ConfirmPanel.ShowModal(CoupledTrainsMod.modName, "Are you sure you want to remove " + assetDropdown.selectedValue + "?", delegate (UIComponent comp, int ret)
                {
                    if(ret == 1)
                    {
                        itemData.trainData.prefabs.RemoveAt(itemData.prefabIndex);
                        //Update UI
                        mainPanel.UpdateFastList();
                    }
                });*/
            };
            buttonRemove.tooltip = UIStringCollection.TooltipButtonRemovePrefab;

            // Button to move prefab up
            buttonUp = UIUtils.CreateButton(settingsPanel);
            buttonUp.size = new Vector2(30, 30);
            buttonUp.normalBgSprite = "IconDownArrow";
            buttonUp.hoveredBgSprite = "IconDownArrowHovered";
            buttonUp.pressedBgSprite = "IconDownArrowPressed";
            buttonUp.relativePosition = new Vector3(UIMainPanel.WIDTH - 90, 15 + 30);
            buttonUp.transform.Rotate(Vector3.forward, 180);
            buttonUp.eventClicked += (c, p) => {
                if(!checkEvents) { return; }

                //itemData.trainData.prefabs.Swap(itemData.prefabIndex, itemData.prefabIndex - 1);
                //Update UI
                mainPanel.UpdatePanels();
            };
            buttonUp.tooltip = UIStringCollection.TooltipButtonUp;

            // Button to move prefab down
            buttonDown = UIUtils.CreateButton(settingsPanel);
            buttonDown.size = new Vector2(30, 30);
            buttonDown.normalBgSprite = "IconDownArrow";
            buttonDown.hoveredBgSprite = "IconDownArrowHovered";
            buttonDown.pressedBgSprite = "IconDownArrowPressed";
            buttonDown.relativePosition = new Vector3(UIMainPanel.WIDTH - 120, 55);
            buttonDown.eventClicked += (c, p) => {
                if(!checkEvents) { return; }

                //itemData.trainData.prefabs.Swap(itemData.prefabIndex, itemData.prefabIndex + 1);
                //Update UI
                mainPanel.UpdatePanels();
            };
            buttonDown.tooltip = UIStringCollection.TooltipButtonDown;

            // Button to reset offsets
            UIButton button = UIUtils.CreateButton(settingsPanel);
            button.text = "Reset offsets";
            button.tooltip = UIStringCollection.TooltipButtonResetOffsets;
            button.width = 170;
            button.relativePosition = new Vector3(390, 55);
            button.eventClicked += (c, p) => {
                if(!checkEvents) { return; }
                //itemData.trainData.prefabs[itemData.prefabIndex].SetDefaultOffsets();
            };
        }

        /*private void PopulateAssetDropdown(TrainConsist.VehicleAIType aiType)
        {
            if(assetDropdown == null || aiType == TrainConsist.VehicleAIType.None)
            {
                return;
            }

            assetDropdown.selectedIndex = -1;

            assetDropdown.items = VehiclePrefabs.GetPrefabLocales(aiType);

            assetDropdown.selectedIndex = 0;
        }*/

        private int PrefabToIndex(Data data)
        {
            /*var list = VehiclePrefabs.GetPrefabs(data.trainData.aiType);
            for(int i = 0; i < list.Length; i++)
            {
                if(list[i] == data.trainData.prefabs[data.prefabIndex].info)
                {
                    return i;
                }
            }*/
            return -1;
        } 

        public void Display(object data, bool isRowOdd)
        {
            CreateComponents();

            itemData = data as Data;
            if(assetDropdown == null || itemData == null)
                return;

            checkEvents = false;

            if(itemData.prefabIndex < 0)
            {
                // Row for adding new asset
                buttonAdd.isVisible = true;
                settingsPanel.isVisible = false;

                //PopulateAssetDropdown(itemData.trainData.aiType);
                assetDropdown.selectedIndex = 0;


                backgroundSprite = "UnlockingItemBackground";
                color = new Color32(0, 0, 0, 128);
            }
            else
            {
                // Normal entry
                buttonAdd.isVisible = false;
                settingsPanel.isVisible = true;

                //buttonRemove.isVisible = (itemData.trainData.prefabs.Count > 1);
                buttonUp.isVisible = (itemData.prefabIndex > 0);
                //buttonDown.isVisible = (itemData.prefabIndex < itemData.trainData.prefabs.Count - 1);

                //PopulateAssetDropdown(itemData.trainData.aiType);
                assetDropdown.selectedIndex = PrefabToIndex(itemData);
                //checkboxBackEngine.isChecked = itemData.trainData.prefabs[itemData.prefabIndex].useBackEngine;

                /*fieldFoF.text = itemData.trainData.prefabs[itemData.prefabIndex].frontInfoOffsetFront.ToString();
                fieldFoB.text = itemData.trainData.prefabs[itemData.prefabIndex].frontInfoOffsetBack.ToString();
                fieldBoF.text = itemData.trainData.prefabs[itemData.prefabIndex].backInfoOffsetFront.ToString();
                fieldBoB.text = itemData.trainData.prefabs[itemData.prefabIndex].backInfoOffsetBack.ToString();
                */
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
#endif