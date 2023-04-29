using ColossalFramework.UI;
using System.Collections.Generic;
using UnityEngine;

namespace RandomTrainTrailers.UI
{
    public class UIFlagsPanel : UIWindowPanel
    {
        private static UIWindowHandle<UIFlagsPanel> _main;

        public static UIWindowHandle<UIFlagsPanel> Main
        {
            get
            {
                if (_main == null)
                {
                    _main = UIWindow.Create<UIFlagsPanel>();
                }
                return _main;
            }
        }

        public override float DefaultWidth => 250;

        public override float DefaultHeight => 400;

        public override string DefaultTitle => "Cargo types";

        public delegate void OnFlagsSet(CargoFlags flags);

        private Dictionary<UICheckBox, CargoFlags> _boxFlagDict = new Dictionary<UICheckBox, CargoFlags>();
        private Dictionary<CargoFlags, UICheckBox> _flagBoxDict = new Dictionary<CargoFlags, UICheckBox>();
        private UIPanel _flagsPanel;
        private UIButton _confirmButton;
        private OnFlagsSet _callback;
        private CargoFlags _flags;
        private bool _allowEdit;

        public override void Start()
        {
            base.Start();
            CreateComponents();
            UpdateDisplay();
        }

        protected override void OnWindowSet(UIWindow window)
        {
            Util.Log($"{GetType()}.{nameof(OnWindowSet)}");
            window.Resizable = false;
        }

        public void Show(CargoFlags checkedFlags, OnFlagsSet callback, bool allowEdit = true)
        {
            _callback = callback;
            _flags = checkedFlags;
            _allowEdit = allowEdit;
           
            Window.Open();
            if (_flagsPanel != null)
                UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            foreach (var kv in _flagBoxDict)
            {
                kv.Value.isChecked = (_flags & kv.Key) > 0;
            }
            _confirmButton.isEnabled = _allowEdit;
        }

        private void CreateComponents()
        {
            _flagsPanel = CreateFlagCheckboxes(Vector3.zero, width / 2, height - 40, _boxFlagDict, _flagBoxDict);

            // Buttons
            var panel = AddUIComponent<UIPanel>();
            panel.relativePosition = UIUtils.Below(_flagsPanel);
            panel.width = width;
            panel.height = 30;
            panel.anchor = UIAnchorStyle.CenterHorizontal | UIAnchorStyle.Bottom;

            _confirmButton = UIUtils.CreateButton(panel);
            _confirmButton.text = "Done";
            _confirmButton.relativePosition = Vector3.zero;
            _confirmButton.eventClicked += (c, p) =>
            {
                Done();
            };

            UIButton cancelButton = UIUtils.CreateButton(panel);
            cancelButton.text = "Cancel";
            cancelButton.relativePosition = UIUtils.RightOf(_confirmButton);
            cancelButton.eventClicked += (c, p) =>
            {
                Window.Close();
            };

            panel.FitChildrenHorizontally();
        }

        private void Done()
        {
            if(_callback != null)
            {
                CargoFlags flags = 0;
                foreach(var v in _boxFlagDict)
                {
                    if(v.Key.isChecked)
                    {
                        flags |= v.Value;
                    }
                }
                _callback.Invoke(flags);
            }
            Window.Close();
        }

        private UIPanel CreateFlagCheckboxes(Vector3 startLocation, float halfWidth, float maxHeight, Dictionary<UICheckBox, CargoFlags> checkboxDict, Dictionary<CargoFlags, UICheckBox> flagDict)
        {
            UIPanel panel = AddUIComponent<UIPanel>();
            panel.width = halfWidth * 2;
            panel.relativePosition = startLocation;

            float y = 0;
            float x = 0;

            var flags = CargoParcel.ResourceTypes;
            for(int i = 0; i < flags.Length; i++)
            {
                UICheckBox checkbox = UIUtils.CreateCheckBox(panel);
                checkbox.text = flags[i].ToString();
                checkbox.isChecked = false;
                checkbox.FitChildrenHorizontally();
                checkbox.relativePosition = new Vector3(x, y);
                y += checkbox.height + 5;
 
                if(y >= maxHeight + checkbox.height)
                {
                    y = 0;
                    x += halfWidth;
                }

                flagDict.Add(flags[i], checkbox);
                checkboxDict.Add(checkbox, flags[i]);
            }

            panel.height = y;
            return panel;
        }
    }
}
