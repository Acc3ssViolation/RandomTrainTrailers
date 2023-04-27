using ColossalFramework.UI;
using RandomTrainTrailers.Definition;
using UnityEngine;

namespace RandomTrainTrailers.UI
{
    internal class UIItemReferenceRow : UIPanel, IUIFastListRow
    {
        public const float Height = 50;

        private RowData<ItemReference> _data;
        private bool _isRowOdd;
        private bool _createdComponents;

        private UICheckBox _selectedCheckbox;
        private UILabel _nameLabel;
        
        public void Deselect(bool isRowOdd)
        {
        }

        public void Select(bool isRowOdd)
        {
        }

        public void Display(object data, bool isRowOdd)
        {
            if (data is RowData<ItemReference> item)
            {
                _data = item;
                _isRowOdd = isRowOdd;
                UpdateDisplay();
            }
        }

        private void UpdateDisplay()
        {
            if (_data == null)
                return;

            EnsureComponents();

            _selectedCheckbox.isChecked = _data.Selected;
            _nameLabel.text = _data.Value.DisplayName;
            tooltip = _data.Value.DisplayName;

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

            // Name
            _nameLabel = AddUIComponent<UILabel>();
            _nameLabel.relativePosition = UIUtils.RightOf(_selectedCheckbox);
            _nameLabel.width = 250;
            _nameLabel.anchor = UIAnchorStyle.Left | UIAnchorStyle.CenterVertical;
            _nameLabel.eventClicked += (_, __) =>
            {
                _selectedCheckbox.isChecked = !_selectedCheckbox.isChecked;
            };

            _createdComponents = true;
        }
    }
}
