using ColossalFramework.UI;
using RandomTrainTrailers.Definition;
using UnityEngine;

namespace RandomTrainTrailers.UI
{
    internal class UITrainPoolSettings : UIWindowPanel
    {
        public override float DefaultWidth => 300;
        public override float DefaultHeight => 200;
        public override string DefaultTitle => "{TrainPool}";

        private TrainPool _pool;

        private UIIntField _minLength;
        private UIIntField _maxLength;
        private UIIntField _minLocomotives;
        private UIIntField _maxLocomotives;
        private UICheckBox _useCargo;

        public override void Start()
        {
            base.Start();
            CreateComponents();
            UpdateData();
        }

        private void CreateComponents()
        {
            var length = CreateLengthRow(Vector3.zero);
            var locomotives = CreateLocomotivesRow(UIUtils.Below(length));
            _useCargo = UIUtils.CreateCheckBox(this);
            _useCargo.relativePosition = UIUtils.Below(locomotives);
            _useCargo.text = "Use cargo contents";
            _useCargo.eventCheckChanged += (_, __) =>
            {
                if (_pool != null)
                    _pool.UseCargo = _useCargo.isChecked;
            };
        }

        private UIPanel CreateLengthRow(Vector3 relativePosition)
        {
            var row = AddUIComponent<UIPanel>();
            row.relativePosition = relativePosition;
            row.width = width;
            row.height = 30;
            row.anchor = UIAnchorStyle.Left | UIAnchorStyle.Top | UIAnchorStyle.Right;

            var label = row.AddUIComponent<UILabel>();
            label.text = "Train length";
            label.relativePosition = Vector3.zero;
            label.anchor = UIAnchorStyle.Left;

            _minLength = UIIntField.CreateField("min", row, false);
            _minLength.panel.relativePosition = UIUtils.Below(label);
            _minLength.panel.anchor = UIAnchorStyle.Left;
            _minLength.textField.eventTextChanged += (_, __) =>
            {
                if (_pool != null)
                    _minLength.IntFieldHandler(ref _pool.MinTrainLength, (v) => v > 0 && v <= _pool.MaxTrainLength);
            };
            _maxLength = UIIntField.CreateField("max", row, false);
            _maxLength.panel.relativePosition = UIUtils.RightOf(_minLength.panel);
            _maxLength.panel.anchor = UIAnchorStyle.Left;
            _maxLength.textField.eventTextChanged += (_, __) =>
            {
                if (_pool != null)
                    _maxLength.IntFieldHandler(ref _pool.MaxTrainLength, (v) => v >= _pool.MinTrainLength);
            };

            row.FitChildrenVertically();

            return row;
        }

        private UIPanel CreateLocomotivesRow(Vector3 relativePosition)
        {
            var row = AddUIComponent<UIPanel>();
            row.relativePosition = relativePosition;
            row.width = width;
            row.height = 30;
            row.anchor = UIAnchorStyle.Left | UIAnchorStyle.Top | UIAnchorStyle.Right;

            var label = row.AddUIComponent<UILabel>();
            label.text = "Locomotives";
            label.relativePosition = Vector3.zero;
            label.anchor = UIAnchorStyle.Left;

            _minLocomotives = UIIntField.CreateField("min", row, false);
            _minLocomotives.panel.relativePosition = UIUtils.Below(label);
            _minLocomotives.panel.anchor = UIAnchorStyle.Left;
            _minLocomotives.textField.eventTextChanged += (_, __) =>
            {
                if (_pool != null)
                    _minLocomotives.IntFieldHandler(ref _pool.MinLocomotiveCount, (v) => v > 0 && v <= _pool.MaxLocomotiveCount);
            };
            _maxLocomotives = UIIntField.CreateField("max", row, false);
            _maxLocomotives.panel.relativePosition = UIUtils.RightOf(_minLocomotives.panel);
            _maxLocomotives.panel.anchor = UIAnchorStyle.Left;
            _maxLocomotives.textField.eventTextChanged += (_, __) =>
            {
                if (_pool != null)
                    _maxLocomotives.IntFieldHandler(ref _pool.MaxLocomotiveCount, (v) => v >= _pool.MinLocomotiveCount);
            };

            row.FitChildrenVertically();

            return row;
        }

        public void SetData(TrainPool pool)
        {
            _pool = pool;
            UpdateData();
        }

        private void UpdateData()
        {
            if (_minLength == null || _pool == null)
                return;

            // Set min value again to clear up any 'invalid' indicators
            _minLength.SetValue(_pool.MinTrainLength);
            _maxLength.SetValue(_pool.MaxTrainLength);
            _minLength.SetValue(_pool.MinTrainLength);

            _minLocomotives.SetValue(_pool.MinLocomotiveCount);
            _maxLocomotives.SetValue(_pool.MaxLocomotiveCount);
            _minLocomotives.SetValue(_pool.MinLocomotiveCount);

            _useCargo.isChecked = _pool.UseCargo;
        }
    }
}
