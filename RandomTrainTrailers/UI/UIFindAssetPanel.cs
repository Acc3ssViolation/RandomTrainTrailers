using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RandomTrainTrailers.UI
{
    public class UIFindAssetPanel : UIPanel
    {
        public enum DisplayMode
        {
            Engines,
            Trailers,
            Both,
            Collections,
        }

        public static UIFindAssetPanel main { get; private set; }

        Action<VehiclePrefabs.VehicleData> m_callback;
        UIDropDown m_typeDropdown;
        UIFastList m_fastList;
        UITextField m_searchField;
        UIButton m_select;
        public const int HEIGHT = 550;
        public const int WIDTH = 700;
        public const int WIDTHLEFT = 500;
        public const int WIDTHRIGHT = 200;
        private DisplayMode m_mode;
        private DisplayMode m_prevMode;

        UITextureSprite m_preview;
        PreviewRenderer m_previewRenderer;

        VehiclePrefabs.VehicleData m_lastSelectedData;

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

            name = "FindTrainAssetPanel";
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
            label.text = "Find Asset";
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
                Close();
            };

            // dropdown
            label = AddUIComponent<UILabel>();
            label.text = "Asset type: ";
            label.textScale = 0.8f;
            label.padding = new RectOffset(0, 0, 8, 0);
            label.relativePosition = new Vector3(10f, verticalOffset);


            m_typeDropdown = UIUtils.CreateDropDown(this);
            m_typeDropdown.AddItem("Passenger Train");
            m_typeDropdown.AddItem("Cargo Train");
            m_typeDropdown.AddItem("Metro");
            m_typeDropdown.AddItem("Tram");
            m_typeDropdown.selectedIndex = 0;
            m_typeDropdown.relativePosition = new Vector3(105f, verticalOffset);
            m_typeDropdown.width = 200f;
            m_typeDropdown.eventSelectedIndexChanged += (c, i) =>
            {
                UpdateFastList();
            };
            m_typeDropdown.tooltip = "Selects the asset type to search for";

            // Search
            label = AddUIComponent<UILabel>();
            label.text = "Search: ";
            label.textScale = 0.8f;
            label.padding = new RectOffset(0, 0, 8, 0);
            label.relativePosition = new Vector3(m_typeDropdown.relativePosition.x + m_typeDropdown.width + 20, verticalOffset);
            m_searchField = UIUtils.CreateTextField(this);
            m_searchField.relativePosition = new Vector3(label.relativePosition.x + label.width + 5, verticalOffset + 5);
            m_searchField.eventTextChanged += (c, i) =>
            {
                UpdateFastList();
            };
            m_searchField.width = WIDTH - m_searchField.relativePosition.x - 10;

            // Preview
            UIPanel panel = AddUIComponent<UIPanel>();
            panel.backgroundSprite = "GenericPanel";
            panel.width = WIDTHRIGHT - 10;
            panel.height = HEIGHT - 375;
            panel.relativePosition = new Vector3(WIDTHLEFT, verticalOffset + 40);

            m_preview = panel.AddUIComponent<UITextureSprite>();
            m_preview.size = panel.size;
            m_preview.relativePosition = Vector3.zero;

            m_previewRenderer = gameObject.AddComponent<PreviewRenderer>();
            m_previewRenderer.size = m_preview.size * 2; // Twice the size for anti-aliasing

            m_preview.texture = m_previewRenderer.texture;

            // fastlist
            m_fastList = UIFastList.Create<UIAssetRow>(this);
            m_fastList.backgroundSprite = "UnlockingPanel";
            m_fastList.width = WIDTHLEFT - 20;
            m_fastList.height = HEIGHT - verticalOffset - 90;
            m_fastList.canSelect = true;
            m_fastList.relativePosition = new Vector3(10, verticalOffset + 40);
            m_fastList.eventSelectedIndexChanged += (c, index) =>
            {
                if(index >= 0 && m_fastList.rowsData.m_size > 0)
                {
                    m_lastSelectedData = m_fastList.selectedItem as VehiclePrefabs.VehicleData;
                    m_previewRenderer.cameraRotation = -60;// 120f;
                    m_previewRenderer.zoom = 4.0f;
                    if(m_lastSelectedData?.info != null)
                    {
                        m_previewRenderer.RenderVehicle(m_lastSelectedData.info);
                    }
                    m_select.enabled = true;
                }
                else
                {
                    m_lastSelectedData = null;
                    m_select.enabled = false;
                }
            };

            // Select button
            m_select = UIUtils.CreateButton(this);
            m_select.text = "Select";
            m_select.relativePosition = new Vector3(WIDTH - m_select.width - 10, HEIGHT - m_select.height - 10);
            m_select.eventClicked += (c, m) =>
            {
                var data = m_fastList.selectedItem as VehiclePrefabs.VehicleData;
                if(m_callback != null)
                {
                    try
                    {
                        m_callback(data);
                    }
                    catch(Exception e)
                    {
                        Util.LogError("Error with callback in UIFindAssetPanel");
                        Util.LogError(e);
                    }
                    Close();
                }
            };

            // Preview events
            panel.eventMouseDown += (c, p) =>
            {
                eventMouseMove += RotateCamera;
                if(m_lastSelectedData?.info != null)
                {
                    m_previewRenderer.RenderVehicle(m_lastSelectedData.info);
                }
            };

            panel.eventMouseUp += (c, p) =>
            {
                eventMouseMove -= RotateCamera;
                if(m_lastSelectedData?.info != null)
                {
                    m_previewRenderer.RenderVehicle(m_lastSelectedData.info);
                }
            };

            panel.eventMouseWheel += (c, p) =>
            {
                m_previewRenderer.zoom -= Mathf.Sign(p.wheelDelta) * 0.25f;
                if(m_lastSelectedData?.info != null)
                {
                    m_previewRenderer.RenderVehicle(m_lastSelectedData.info);
                }
            };

            UpdateFastList();
        }

        private void RotateCamera(UIComponent c, UIMouseEventParameter p)
        {
            m_previewRenderer.cameraRotation -= p.moveDelta.x / m_preview.width * 360f;
            if(m_lastSelectedData?.info != null)
            {
                m_previewRenderer.RenderVehicle(m_lastSelectedData.info);
            }
        }


        private void UpdateFastList()
        {
            var pos = m_fastList.listPosition;

            FastList<object> newRowsData = new FastList<object>();

            VehiclePrefabs.VehicleType type = VehiclePrefabs.VehicleType.Unknown;
            switch(m_typeDropdown.selectedIndex)
            {
                case 0:
                    type = VehiclePrefabs.VehicleType.PassengerTrain;
                    break;
                case 1:
                    type = VehiclePrefabs.VehicleType.CargoTrain;
                    break;
                case 2:
                    type = VehiclePrefabs.VehicleType.Metro;
                    break;
                case 3:
                    type = VehiclePrefabs.VehicleType.Tram;
                    break;
            }

            if(m_mode != DisplayMode.Collections)
            {
                var list = VehiclePrefabs.GetPrefabs(type);
                foreach(var item in list)
                {
                    if((m_mode == DisplayMode.Both ||
                        (m_mode == DisplayMode.Engines && item.isTrailer == false) ||
                        (m_mode == DisplayMode.Trailers && item.isTrailer))
                        &&
                        (string.IsNullOrEmpty(m_searchField.text) ||
                        (item.localeName.ToLower().Contains(m_searchField.text.ToLower()) || item.info.name.ToLower().Contains(m_searchField.text.ToLower()))))
                    {
                        newRowsData.Add(item);
                    }
                }
            }
            else
            {
                // Collections
                var collections = UIMainPanel.main.UserDefinition?.Collections;
                if(collections != null)
                {
                    foreach(var collection in collections)
                    {
                        if(string.IsNullOrEmpty(m_searchField.text) ||
                        collection.Name.ToLower().Contains(m_searchField.text.ToLower()))
                        {
                            newRowsData.Add(new VehiclePrefabs.VehicleData() {
                                localeName = collection.Name
                            });
                        }
                    }
                }

                // Show default collections as well, useful for the all-cargo-type ones.
                collections = DefaultTrailerConfig.DefaultDefinition?.Collections;
                if(collections != null)
                {
                    foreach(var collection in collections)
                    {
                        if(string.IsNullOrEmpty(m_searchField.text) ||
                        collection.Name.ToLower().Contains(m_searchField.text.ToLower()))
                        {
                            newRowsData.Add(new VehiclePrefabs.VehicleData()
                            {
                                localeName = collection.Name,
                                isTrailer = true
                            });
                        }
                    }
                }
            }


            m_fastList.rowHeight = UIAssetRow.HEIGHT;
            m_fastList.rowsData = newRowsData;

            if(m_prevMode == m_mode)
            {
                m_fastList.DisplayAt(pos);
            }
        }

        private void Close()
        {
            m_callback = null;
            Hide();
        }

        public void Show(Action<VehiclePrefabs.VehicleData> callback, DisplayMode mode)
        {
            m_prevMode = m_mode;
            m_mode = mode;
            m_callback = callback;
            Show(true);
            UpdateFastList();
        }
    }
}
