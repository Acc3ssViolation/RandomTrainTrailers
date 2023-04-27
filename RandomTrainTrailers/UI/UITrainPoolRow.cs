using ColossalFramework.UI;
using RandomTrainTrailers.Definition;
using UnityEngine;

namespace RandomTrainTrailers.UI
{
    internal class UITrainPoolRow : UIPanel, IUIFastListRow
    {
        public const float Height = 50;

        private RowData<TrainPool> _data;
        private bool _isRowOdd;
        private bool _createdComponents;

        private UICheckBox _selectedCheckbox;
        private UIButton _deleteButton;
        private UITextField _nameField;
        private UIButton _locomotiveButton;
        private UIButton _wagonButton;
        private UIButton _settings;
        private UICheckBox _enabled;
        
        public void Deselect(bool isRowOdd)
        {
        }

        public void Select(bool isRowOdd)
        {
        }

        public void Display(object data, bool isRowOdd)
        {
            if (data is RowData<TrainPool> pool)
            {
                _data = pool;
                _isRowOdd = isRowOdd;
                UpdateDisplay();
            }
        }

        public void UpdateDisplay()
        {
            if (_data == null)
                return;

            EnsureComponents();

            _selectedCheckbox.isChecked = _data.Selected;
            _nameField.text = _data.Value.Name;
            _wagonButton.text = $"{_data.Value.TrailerCollections.Count} wagons";
            _locomotiveButton.text = $"{_data.Value.Locomotives.Count} locomotives";
            _enabled.isChecked = _data.Value.Enabled;

            if (_isRowOdd)
            {
                backgroundSprite = "UnlockingItemBackground";
                color = new Color32(255, 255, 255, 255);
            }
            else
            {
                backgroundSprite = null;
            }
        }

        private void EnsureComponents()
        {
            width = parent.width;
            height = Height;

            if (_createdComponents)
                return;

            const float Margin = 10;
            const float ScrollBarWidth = 20;
            var x = Margin;

            // Selection checkbox
            _selectedCheckbox = UIUtils.CreateCheckBox(this);
            _selectedCheckbox.relativePosition = new Vector3(x, 0);
            _selectedCheckbox.width = _selectedCheckbox.height = 16;
            _selectedCheckbox.anchor = UIAnchorStyle.Left | UIAnchorStyle.CenterVertical;
            _selectedCheckbox.eventCheckChanged += (_, __) =>
            {
                if (_data != null)
                    _data.Selected = _selectedCheckbox.isChecked;
            };

            // Name of pool (will expand on resize)
            _nameField = UIUtils.CreateTextField(this);
            _nameField.relativePosition = UIUtils.RightOf(_selectedCheckbox);
            _nameField.width = 250;
            _nameField.anchor = UIAnchorStyle.Left | UIAnchorStyle.CenterVertical | UIAnchorStyle.Right;
            _nameField.eventTextChanged += (_, __) =>
            {
                if (_data != null)
                    _data.Value.Name = _nameField.text;
            };

            // Locomotive button
            _locomotiveButton = UIUtils.CreateButton(this);
            _locomotiveButton.text = "{Num} locomotives";
            _locomotiveButton.width = 150;
            _locomotiveButton.relativePosition = UIUtils.RightOf(_nameField);
            _locomotiveButton.anchor = UIAnchorStyle.Right | UIAnchorStyle.CenterVertical;
            _locomotiveButton.eventClicked += (_, __) =>
            {
                OpenLocomotiveWindow();
            };

            // Wagon button
            _wagonButton = UIUtils.CreateButton(this);
            _wagonButton.text = "{Num} wagons";
            _wagonButton.width = 120;
            _wagonButton.relativePosition = UIUtils.RightOf(_locomotiveButton);
            _wagonButton.anchor = UIAnchorStyle.Right | UIAnchorStyle.CenterVertical;
            _wagonButton.eventClicked += (_, __) =>
            {
                OpenWagonWindow();
            };

            // Settings
            _settings = UIUtils.CreateButton(this);
            _settings.text = "Settings";
            _settings.width = 90;
            _settings.relativePosition = UIUtils.RightOf(_wagonButton);
            _settings.anchor = UIAnchorStyle.Right | UIAnchorStyle.CenterVertical;
            _settings.eventClicked += (_, __) =>
            {
                OpenSettingsWindow();
            };

            // Delete button
            _deleteButton = UIUtils.CreateButton(this);
            _deleteButton.size = new Vector2(30, 30);
            _deleteButton.normalBgSprite = "buttonclose";
            _deleteButton.hoveredBgSprite = "buttonclosehover";
            _deleteButton.pressedBgSprite = "buttonclosepressed";
            _deleteButton.relativePosition = new Vector3(width - _deleteButton.width - Margin - ScrollBarWidth, 0);
            _deleteButton.anchor = UIAnchorStyle.Right | UIAnchorStyle.CenterVertical;
            _deleteButton.eventClicked += (c, p) =>
            {
                ConfirmPanel.ShowModal(Mod.name, $"Are you sure you want to delete '{_data?.Value.Name}'?", (_, ret) =>
                {
                    if (ret == 1)
                    {
                        _data?.Delete(_data);
                    }
                });
            };
            _deleteButton.tooltip = "Deletes the pool";

            // Enabled checkbox
            _enabled = UIUtils.CreateCheckBox(this);
            _enabled.text = "Enabled";
            _enabled.width = 85;
            _enabled.relativePosition = UIUtils.LeftOf(_enabled, _deleteButton);
            _enabled.anchor = UIAnchorStyle.Right | UIAnchorStyle.CenterVertical;
            _enabled.eventCheckChanged += (_, __) =>
            {
                if (_data != null)
                    _data.Value.Enabled = _enabled.isChecked;
            };

            _createdComponents = true;
        }

        private void OpenLocomotiveWindow()
        {
            var window = UIWindow.Create<UITrainPoolReferencePanel>(600, 500, "Locomotives in pool");
            window.DestroyOnClose = true;
            // TODO: This reference to UIMainPanel feels a bit hacky
            ((UITrainPoolReferencePanel)window.Content).SetData(UIMainPanel.main.m_userDefinition, _data.Value, UITrainPoolReferencePanel.DataType.Locomotives, this);
            window.Open();
        }

        private void OpenWagonWindow()
        {
            var window = UIWindow.Create<UITrainPoolReferencePanel>(600, 500, "Trailers in pool");
            window.DestroyOnClose = true;
            // TODO: This reference to UIMainPanel feels a bit hacky
            ((UITrainPoolReferencePanel)window.Content).SetData(UIMainPanel.main.m_userDefinition, _data.Value, UITrainPoolReferencePanel.DataType.TrailerCollections, this);
            window.Open();
        }

        private void OpenSettingsWindow()
        {

        }
    }
}
