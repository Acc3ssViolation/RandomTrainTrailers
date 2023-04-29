using ColossalFramework.UI;
using RandomTrainTrailers.Definition;
using UnityEngine;

namespace RandomTrainTrailers.UI
{
    internal class UITrailerSettings : UIPanel, IUIWindowPanel
    {
        public float DefaultWidth => 300;
        public float DefaultHeight => 460;
        public string DefaultTitle => "{Trailer}";

        private Trailer _trailer;

        private UIIntField _invertProbability;
        private UIIntField _randomWeight;
        private UIPreviewPanel _previewPanel;

        public override void Start()
        {
            base.Start();
            CreateComponents();
            UpdateData();
        }

        private void CreateComponents()
        {
            _previewPanel = AddUIComponent<UIPreviewPanel>();
            _previewPanel.relativePosition = Vector3.zero;
            _previewPanel.width = width;
            _previewPanel.height = 300;
            _previewPanel.anchor = UIAnchorStyle.All;

            _invertProbability = UIIntField.CreateField("Invert chance", this, false);
            _invertProbability.panel.relativePosition = UIUtils.Below(_previewPanel);
            _invertProbability.panel.anchor = UIAnchorStyle.Left | UIAnchorStyle.Bottom;
            _invertProbability.textField.eventTextChanged += (_, __) =>
            {
                if (_trailer != null)
                {
                    _invertProbability.IntFieldHandler(ref _trailer.InvertProbability, (v) => v >= 0 && v <= 100);
                }
            };

            _randomWeight = UIIntField.CreateField("Random weight", this, false);
            _randomWeight.panel.relativePosition = UIUtils.Below(_invertProbability.panel);
            _randomWeight.panel.anchor = UIAnchorStyle.Left | UIAnchorStyle.Bottom;
            _randomWeight.textField.eventTextChanged += (_, __) =>
            {
                if (_trailer != null)
                {
                    _randomWeight.IntFieldHandler(ref _trailer.Weight, (v) => v >= 0 && v <= 1000000);
                }
            };
        }

        public void SetData(Trailer data)
        {
            _trailer = data;
            UpdateData();
        }

        private void UpdateData()
        {
            if (_invertProbability == null || _trailer == null)
                return;

            _invertProbability.SetValue(_trailer.InvertProbability);
            _randomWeight.SetValue(_trailer.Weight);

            UpdatePreview();
        }

        private void UpdatePreview()
        {
            _previewPanel.VehicleInfo = _trailer.VehicleInfos?[0];
        }
    }
}
