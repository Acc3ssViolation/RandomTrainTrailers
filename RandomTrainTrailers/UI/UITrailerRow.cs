using ColossalFramework.UI;
using RandomTrainTrailers.Definition;
using UnityEngine;
using static TimeMilestone;

namespace RandomTrainTrailers.UI
{
    internal class UITrailerRow : UIPanel, IUIFastListRow
    {
        public const float Height = 50;

        private RowData<Trailer> _data;
        private bool _isRowOdd;
        private bool _createdComponents;

        private UICheckBox _selectedCheckbox;
        private UIButton _deleteButton;
        private UILabel _nameField;
        private UIButton _settings;
        private UICheckBox _enabled;
        private UICargoTypeRow _cargoType;
        
        public void Deselect(bool isRowOdd)
        {
        }

        public void Select(bool isRowOdd)
        {
        }

        public void Display(object data, bool isRowOdd)
        {
            if (data is RowData<Trailer> pool)
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
            _nameField.text = Util.GetVehicleDisplayName(_data.Value.AssetName);
            _nameField.textColor = _data.Value.VehicleInfos != null ? UIConstants.TextColor : UIConstants.InvalidTextColor;
            _nameField.tooltip = _data.Value.AssetName;
            _enabled.isChecked = _data.Value.Enabled;
            _cargoType.Flags = _data.Value.CargoType;

            if (_isRowOdd)
            {
                backgroundSprite = UIConstants.OddRowBackground;
            }
            else
            {
                backgroundSprite = UIConstants.EvenRowBackground;
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

            // Name of trailer
            _nameField = AddUIComponent<UILabel>();
            _nameField.relativePosition = UIUtils.RightOf(_selectedCheckbox);
            _nameField.width = 250;
            _nameField.anchor = UIAnchorStyle.Left | UIAnchorStyle.CenterVertical;
            _nameField.eventClicked += (_, __) =>
            {
                _selectedCheckbox.isChecked = !_selectedCheckbox.isChecked;
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
                ConfirmPanel.ShowModal(Mod.name, $"Are you sure you want to delete '{_nameField.text}'?", (_, ret) =>
                {
                    if (ret == 1)
                    {
                        _data?.Delete(_data);
                    }
                });
            };
            _deleteButton.tooltip = "Deletes the trailer";

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

            // Settings
            _settings = UIUtils.CreateButton(this);
            _settings.text = "Settings";
            _settings.width = 90;
            _settings.relativePosition = UIUtils.LeftOf(_settings, _enabled);
            _settings.anchor = UIAnchorStyle.Right | UIAnchorStyle.CenterVertical;
            _settings.eventClicked += (_, __) =>
            {
                OpenSettingsWindow();
            };

            // Cargo type
            _cargoType = AddUIComponent<UICargoTypeRow>();
            _cargoType.Summarized = true;
            _cargoType.relativePosition = UIUtils.LeftOf(_cargoType, _settings);
            _cargoType.anchor = UIAnchorStyle.Right | UIAnchorStyle.CenterVertical;
            _cargoType.eventClicked += (c, p) => {
                if (_data == null)
                    return;

                UIFlagsPanel.Main.Content.Show(_data.Value.CargoType, (flags) =>
                {
                    if (_data == null)
                        return;

                    _data.Value.CargoType = flags;
                    UpdateDisplay();
                });
            };

            _createdComponents = true;
        }

        private void OpenSettingsWindow()
        {
            if (_data.Value.IsMultiTrailer)
            {
                // TODO: Replace with a new UI
                UIMultiTrailerPanel.Main.Content.Show(_data.Value, false);
            }
            else
            {
                // Single trailer window
                var window = UIWindow.Create<UITrailerSettings>();
                window.Window.Title = _nameField.text;
                window.Window.DestroyOnClose = true;
                window.Content.SetData(_data.Value);
                window.Open();
            }
        }
    }
}
