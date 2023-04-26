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

        private void UpdateDisplay()
        {
            if (_data == null)
                return;

            EnsureComponents();

            _selectedCheckbox.label.text = _data.Value.Name;

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
            if (_createdComponents)
                return;

            const float Margin = 10;
            var x = Margin;

            // Selection checkbox + name of asset
            _selectedCheckbox = UIUtils.CreateCheckBox(this);
            _selectedCheckbox.relativePosition = new Vector3(x, 0);
            _selectedCheckbox.anchor = UIAnchorStyle.Left | UIAnchorStyle.CenterVertical;

            // Delete button
            // Button for removing prefab
            _deleteButton = UIUtils.CreateButton(this);
            _deleteButton.size = new Vector2(30, 30);
            _deleteButton.normalBgSprite = "buttonclose";
            _deleteButton.hoveredBgSprite = "buttonclosehover";
            _deleteButton.pressedBgSprite = "buttonclosepressed";
            _deleteButton.relativePosition = new Vector3(width - Margin, 0);
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

            _createdComponents = true;
        }
    }
}
