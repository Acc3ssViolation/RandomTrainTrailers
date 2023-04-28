using ColossalFramework.UI;
using RandomTrainTrailers.Definition;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RandomTrainTrailers.UI
{
    internal class UILocomotiveSettings : UIPanel
    {
        private Locomotive _locomotive;

        private UIDropDown _type;
        private UIIntField _length;
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

            _length = UIIntField.CreateField("Length", this, false);
            _length.panel.relativePosition = UIUtils.Below(_previewPanel);
            _length.panel.anchor = UIAnchorStyle.Left | UIAnchorStyle.Bottom;
            _length.textField.eventTextChanged += (_, __) =>
            {
                if (_locomotive != null)
                {
                    if (_length.IntFieldHandler(ref _locomotive.Length, (v) => v >= 1))
                        UpdatePreview();
                }   
            };

            _type = CreateDropDown("Type", this, UIUtils.Below(_length.panel));
            _type.eventSelectedIndexChanged += (_, __) =>
            {
                if (_locomotive != null)
                    _locomotive.Type = (LocomotiveType)_type.selectedIndex;
            };
        }

        private UIDropDown CreateDropDown(string text, UIComponent parent, Vector3 relativePosition)
        {
            var panel = parent.AddUIComponent<UIPanel>();
            panel.name = $"{text} Dropdown";
            panel.relativePosition = relativePosition;
            panel.width = parent.width;
            panel.height = 30;
            panel.anchor = UIAnchorStyle.Left | UIAnchorStyle.Bottom;
            
            var label = panel.AddUIComponent<UILabel>();
            label.text = text;
            label.relativePosition = new Vector3(0, 0);
            label.anchor = UIAnchorStyle.Left | UIAnchorStyle.CenterVertical;

            var dropdown = UIUtils.CreateDropDown(panel);
            dropdown.relativePosition = UIUtils.RightOf(label);
            dropdown.width = panel.width - dropdown.relativePosition.x;
            dropdown.anchor = UIAnchorStyle.Left | UIAnchorStyle.CenterVertical;

            return dropdown;
        }

        public void SetData(Locomotive pool)
        {
            _locomotive = pool;
            UpdateData();
        }

        private void UpdateData()
        {
            if (_length == null || _locomotive == null)
                return;

            _length.SetValue(_locomotive.Length);
            _type.items = Enum.GetNames(typeof(LocomotiveType));
            var index = (int)_locomotive.Type;
            if (index < 0 || index >= _type.items.Length)
                index = 0;
            _type.selectedIndex = index;

            UpdatePreview();
        }

        private void UpdatePreview()
        {
            if (_locomotive.Length == 1)
            {
                _previewPanel.VehicleInfo = _locomotive.VehicleInfo;
            }
            else if (_locomotive.Length > 1 && _locomotive?.VehicleInfo?.m_trailers != null)
            {
                var infos = new List<VehicleRenderInfo>(_locomotive.Length)
                {
                    new VehicleRenderInfo(_locomotive.VehicleInfo, false)
                };
                var trailers = _locomotive.VehicleInfo.m_trailers;
                var trailerCount = _locomotive.Length - 1;
                if (trailerCount > trailers.Length)
                    trailerCount = trailers.Length;

                for (var i = 0; i < trailerCount; i++)
                {
                    infos.Add(new VehicleRenderInfo(trailers[i].m_info, trailers[i].m_invertProbability >= 50));
                }

                _previewPanel.VehicleInfos = infos;
            }
        }
    }
}
