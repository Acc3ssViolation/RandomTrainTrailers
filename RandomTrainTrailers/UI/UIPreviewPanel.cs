using ColossalFramework.UI;
using UnityEngine;

namespace RandomTrainTrailers.UI
{
    internal class UIPreviewPanel : UIPanel
    {
        private UITextureSprite _preview;
        private PreviewRenderer _previewRenderer;
        private int _resolutionFactor = 2;
        private VehicleInfo _vehicleInfo;
        private UILabel _noVehicleLabel;

        public int ResolutionFactor
        {
            get => _resolutionFactor;
            set
            {
                if (_resolutionFactor != value)
                {
                    _resolutionFactor = value;
                    UpdateTexture();
                }
            }
        }

        public VehicleInfo VehicleInfo
        {
            get => _vehicleInfo;
            set
            {
                if (_vehicleInfo != value)
                {
                    _vehicleInfo = value;
                    RenderVehicle();
                }
            }
        }

        public override void Start()
        {
            base.Start();
            CreateComponents();
            ResetCamera();
            UpdateTexture();
        }

        public void ResetCamera()
        {
            _previewRenderer.cameraRotation = -60;// 120f;
            _previewRenderer.zoom = 4.0f;
        }

        private void CreateComponents()
        {
            backgroundSprite = UIConstants.PreviewBackground;

            // TODO: Do we want to create a fancy border?
            _preview = AddUIComponent<UITextureSprite>();
            _preview.width = width;
            _preview.height = height;
            _preview.relativePosition = Vector3.zero;
            _preview.anchor = UIAnchorStyle.All;

            _noVehicleLabel = AddUIComponent<UILabel>();
            _noVehicleLabel.anchor = UIAnchorStyle.CenterHorizontal | UIAnchorStyle.CenterVertical;
            _noVehicleLabel.text = "No preview\navailable";

            // Mouse events
            _preview.eventMouseDown += (_, __) =>
            {
                eventMouseMove += RotateCamera;
            };

            _preview.eventMouseUp += (_, __) =>
            {
                eventMouseMove -= RotateCamera;
            };

            _preview.eventMouseWheel += (_, p) =>
            {
                _previewRenderer.zoom -= Mathf.Sign(p.wheelDelta) * 0.25f;
                RenderVehicle();
            };

            _previewRenderer = gameObject.AddComponent<PreviewRenderer>();
            _previewRenderer.size = size * ResolutionFactor;
        }

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();
            UpdateTexture();
        }

        private void UpdateTexture()
        {
            if (_previewRenderer == null || _preview == null)
                return;

            _previewRenderer.size = size * ResolutionFactor;
            _preview.texture = _previewRenderer.texture;
            RenderVehicle();
        }

        private void RenderVehicle()
        {
            if (_vehicleInfo != null && _previewRenderer != null)
            {
                _previewRenderer.RenderVehicle(_vehicleInfo);
                _noVehicleLabel.isVisible = _vehicleInfo == null;
                _preview.isVisible = _vehicleInfo != null;
            }
        }

        private void RotateCamera(UIComponent c, UIMouseEventParameter p)
        {
            _previewRenderer.cameraRotation -= p.moveDelta.x / _preview.width * 360f;
            RenderVehicle();
        }
    }
}
