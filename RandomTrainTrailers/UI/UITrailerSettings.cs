using ColossalFramework.UI;
using RandomTrainTrailers.Definition;
using UnityEngine;

namespace RandomTrainTrailers.UI
{
    internal class UITrailerSettings : UIWindowPanel
    {
        public override float DefaultWidth => 480;
        public override float DefaultHeight => 490;
        public override string DefaultTitle => "Create Trailer";

        private Trailer _trailer;

        private UIIntField _invertProbability;
        private UIIntField _randomWeight;
        private UIPreviewPanel _previewPanel;
        private UICargoTypeRow _cargoType;

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

            // Cargo type
            var label = AddUIComponent<UILabel>();
            label.text = "Cargo types (click icons to edit)";
            label.relativePosition = UIUtils.Below(_randomWeight.panel);
            label.anchor = UIAnchorStyle.Left | UIAnchorStyle.Bottom;

            _cargoType = AddUIComponent<UICargoTypeRow>();
            _cargoType.Summarized = false;
            _cargoType.autoLayoutStart = LayoutStart.TopLeft;
            _cargoType.relativePosition = UIUtils.Below(label);
            _cargoType.anchor = UIAnchorStyle.Left | UIAnchorStyle.Bottom;
            _cargoType.eventClicked += (c, p) => {
                if (_trailer == null)
                    return;

                // TODO: Flags panel is always behind these windows, probably because it is a child of UIMainPanel
                UIFlagsPanel.Main.Content.Show(_trailer.CargoType, (flags) =>
                {
                    if (_trailer == null)
                        return;

                    _trailer.CargoType = flags;
                    UpdateData();
                });
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
            _cargoType.Flags = _trailer.CargoType;

            UpdatePreview();
        }

        private void UpdatePreview()
        {
            _previewPanel.VehicleInfo = _trailer.VehicleInfos?[0];
        }
    }
}
