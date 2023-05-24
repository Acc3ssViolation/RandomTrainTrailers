using ColossalFramework.UI;
using RandomTrainTrailers.Definition;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RandomTrainTrailers.UI
{
    internal class UITrailerImportPanel : UIWindowPanel
    {
        private struct Data
        {
            public int startIndex;
            public int length;
            public CargoFlags cargo;
        }

        public override float DefaultWidth => 480;
        public override float DefaultHeight => 530;
        public override string DefaultTitle => "{Trailer}";

        private Action<Trailer> _createdCallback;
        private VehicleInfo _vehicleInfo;
        private int _maxIndex;

        private UIPreviewPanel _previewPanel;
        private UICargoTypeRow _cargoType;
        private UIIntField _startIndex;
        private UIIntField _length;
        private UIButton _createButton;

        private Data _data;

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

            _startIndex = UIIntField.CreateField("Vehicle index", this, false);
            _startIndex.panel.tooltip = "The start index in the whole vehicle where the trailer is";
            _startIndex.panel.relativePosition = UIUtils.Below(_previewPanel);
            _startIndex.panel.anchor = UIAnchorStyle.Left | UIAnchorStyle.Bottom;
            _startIndex.textField.eventTextChanged += (_, __) =>
            {
                if (_vehicleInfo != null)
                {
                    if (_startIndex.IntFieldHandler(ref _data.startIndex, (v) => v >= 0 && v <= _maxIndex))
                        ClampData();
                    UpdatePreview();
                }
            };

            _length = UIIntField.CreateField("Length", this, false);
            _length.panel.relativePosition = UIUtils.Below(_startIndex.panel);
            _length.panel.anchor = UIAnchorStyle.Left | UIAnchorStyle.Bottom;
            _length.textField.eventTextChanged += (_, __) =>
            {
                if (_vehicleInfo != null)
                {
                    _length.IntFieldHandler(ref _data.length, (v) => v >= 1 && v <= (_maxIndex + 1) - _data.startIndex);
                    UpdatePreview();
                }
            };

            // Cargo type
            var label = AddUIComponent<UILabel>();
            label.text = "Cargo types (click icons to edit)";
            label.relativePosition = UIUtils.Below(_length.panel);
            label.anchor = UIAnchorStyle.Left | UIAnchorStyle.Bottom;

            _cargoType = AddUIComponent<UICargoTypeRow>();
            _cargoType.Summarized = false;
            _cargoType.autoLayoutStart = LayoutStart.TopLeft;
            _cargoType.relativePosition = UIUtils.Below(label);
            _cargoType.anchor = UIAnchorStyle.Left | UIAnchorStyle.Bottom;
            _cargoType.eventClicked += (c, p) => {
                if (_vehicleInfo == null)
                    return;

                UIFlagsPanel.Main.Content.Show(_data.cargo, (flags) =>
                {
                    if (_vehicleInfo == null)
                        return;

                    _data.cargo = flags;
                    UpdateData();
                });
            };

            // Create button
            _createButton = UIUtils.CreateButton(this);
            _createButton.relativePosition = UIUtils.Below(_cargoType);
            _createButton.text = "Create";
            _createButton.tooltip = "Create the trailer definition";
            _createButton.anchor = UIAnchorStyle.Left | UIAnchorStyle.Bottom;
            _createButton.eventClicked += (c, p) =>
            {
                if (_vehicleInfo == null)
                    return;

                CreateTrailer();
                Window.Close();
            };
        }

        public void SetData(VehicleInfo vehicleInfo, Action<Trailer> createdCallback)
        {
            _createdCallback = createdCallback;
            _vehicleInfo = vehicleInfo;
            _maxIndex = 0;
            if (vehicleInfo.m_trailers != null)
                _maxIndex += vehicleInfo.m_trailers.Length;
            _data.length = 1;
            _data.startIndex = 0;
            _data.cargo = CargoFlags.None;            
            UpdateData();
        }

        private void UpdateData()
        {
            if (_vehicleInfo == null || _previewPanel == null)
                return;

            _startIndex.SetValue(_data.startIndex);
            _length.SetValue(_data.length);
            _cargoType.Flags = _data.cargo;

            UpdatePreview();
        }

        private void UpdatePreview()
        {
            var vehicles = new List<VehicleRenderInfo>(_data.length);
            Debug.Assert(_data.length > 0);

            for (var i = 0; i < _data.length; i++)
                vehicles.Add(GetVehicleRenderInfo(i + _data.startIndex));

            _previewPanel.VehicleInfos = vehicles;
        }

        private VehicleRenderInfo GetVehicleRenderInfo(int index)
        {
            if (index == 0)
                return new VehicleRenderInfo { VehicleInfo = _vehicleInfo };
            var trailerIndex = index - 1;
            var trailer = _vehicleInfo.m_trailers[trailerIndex];
            return new VehicleRenderInfo { VehicleInfo = trailer.m_info, Inverted = trailer.m_invertProbability >= 50 };
        }

        private void ClampData()
        {
            var vehicleLength = _maxIndex + 1;
            if (_data.startIndex + _data.length > vehicleLength)
            {
                _data.length = vehicleLength - _data.startIndex;
                UpdateData();
            }
        }

        private void CreateTrailer()
        {
            Debug.Assert(_data.length >= 1);

            if (_data.length == 1)
            {
                CreateBasicTrailer();
            }
            else
            {
                CreateMultiTrailer();
            }
        }

        private void CreateBasicTrailer()
        {
            var trailer = new Trailer(_vehicleInfo)
            {
                CargoType = _data.cargo,
            };
            _createdCallback?.Invoke(trailer);
        }

        private void CreateMultiTrailer()
        {
            var subTrailers = new List<Trailer>(_data.length);
            for (var i = 0; i < _data.length; i++)
            {
                var info = GetVehicleRenderInfo(i + _data.startIndex);
                subTrailers.Add(new Trailer(info.VehicleInfo)
                {
                    InvertProbability = info.Inverted ? 100 : 0,
                });
            }
            var name = $"{Util.GetVehicleDisplayName(_vehicleInfo.name)} Multi";
            var trailer = new Trailer(name)
            {
                CargoType = _data.cargo,
                SubTrailers = subTrailers,
            };
            _createdCallback?.Invoke(trailer);
        }
    }
}
