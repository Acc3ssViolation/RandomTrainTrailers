using ColossalFramework.UI;
using RandomTrainTrailers.Definition;
using System.Collections.Generic;
using UnityEngine;

namespace RandomTrainTrailers.UI
{
    internal abstract class UIBaseListPanel<DataType, RowType> : UIPanel where RowType : UIPanel, IUIFastListRow where DataType : IEnableable
    {
        private FilterableFastList<DataType> _itemList;
        private TrailerDefinition _trailerDefinition;
        private UIButton _deleteButton;
        private UIButton _selectAllButton;
        private UIButton _enableButton;
        private UIButton _disableButton;

        protected FilterableFastList<DataType> List => _itemList;

        protected abstract float RowHeight { get; }

        public override void Start()
        {
            base.Start();
            CreateComponents();
            UpdateData();
        }

        private void CreateComponents()
        {
            const float Margin = 10;

            var selectionPanel = CreateSelectionButtons();
            _itemList = CreateList(UIUtils.Below(selectionPanel));
            var buttonPanel = CreateEditButtons();
            if (buttonPanel != null)
                _itemList.UIList.height -= buttonPanel.height + Margin;
        }

        private FilterableFastList<DataType> CreateList(Vector3 relativePosition)
        {
            // Filter
            var filterPanel = AddUIComponent<UIPanel>();
            filterPanel.height = 30;
            filterPanel.width = width;
            filterPanel.relativePosition = relativePosition;
            filterPanel.anchor = UIAnchorStyle.Top | UIAnchorStyle.Left | UIAnchorStyle.Right;

            var filterLabel = filterPanel.AddUIComponent<UILabel>();
            filterLabel.text = "Filter";
            filterLabel.relativePosition = new Vector3(0, 0);
            filterLabel.anchor = UIAnchorStyle.Left | UIAnchorStyle.CenterVertical;

            var filterField = UIUtils.CreateTextField(filterPanel);
            filterField.relativePosition = UIUtils.RightOf(filterLabel);
            filterField.width = 250;
            filterField.anchor = UIAnchorStyle.Left | UIAnchorStyle.CenterVertical;

            // Main list
            var list = UIFastList.Create<RowType>(this);
            list.relativePosition = UIUtils.Below(filterPanel);
            list.width = width;
            list.height = height - list.relativePosition.y;
            list.rowHeight = RowHeight;
            list.backgroundSprite = UIConstants.FastListBackground;
            list.anchor = UIAnchorStyle.All;

            var filterable = new FilterableFastList<DataType>(list);
            filterable.SetFilter((item) => Filter(item, filterField.text));
            filterField.eventTextChanged += (_, __) =>
            {
                filterable.ApplyFilter();
            };

            return filterable;
        }

        protected abstract bool Filter(DataType item, string filter);

        private UIPanel CreateSelectionButtons()
        {
            var panel = AddUIComponent<UIPanel>();
            panel.width = width;
            panel.height = 30;
            panel.relativePosition = new Vector3(0, 0);
            //panel.ResetLayout(false, true);
            panel.anchor = UIAnchorStyle.Left | UIAnchorStyle.Right | UIAnchorStyle.Top;

            _selectAllButton = UIUtils.CreateButton(panel);
            _selectAllButton.text = "Select all";
            _selectAllButton.relativePosition = new Vector3(0, 0);
            _selectAllButton.anchor = UIAnchorStyle.Left | UIAnchorStyle.CenterVertical;
            _selectAllButton.eventClicked += (_, __) =>
            {
                _itemList.SelectAll();
            };

            _deleteButton = UIUtils.CreateButton(panel);
            _deleteButton.text = "Delete";
            _deleteButton.width = 80;
            _deleteButton.relativePosition = UIUtils.RightOf(_selectAllButton);
            _deleteButton.anchor = UIAnchorStyle.Left | UIAnchorStyle.CenterVertical;
            _deleteButton.eventClicked += (_, __) =>
            {
                DeleteSelected();
            };

            _enableButton = UIUtils.CreateButton(panel);
            _enableButton.text = "Enable";
            _enableButton.width = 80;
            _enableButton.relativePosition = UIUtils.RightOf(_deleteButton);
            _enableButton.anchor = UIAnchorStyle.Left | UIAnchorStyle.CenterVertical;
            _enableButton.eventClicked += (_, __) =>
            {
                EnableSelected();
            };

            _disableButton = UIUtils.CreateButton(panel);
            _disableButton.text = "Disable";
            _disableButton.width = 80;
            _disableButton.relativePosition = UIUtils.RightOf(_enableButton);
            _disableButton.anchor = UIAnchorStyle.Left | UIAnchorStyle.CenterVertical;
            _disableButton.eventClicked += (_, __) =>
            {
                DisableSelected();
            };

            CreateExtraSelectionButtons(panel, _disableButton);

            return panel;
        }

        protected virtual void CreateExtraSelectionButtons(UIPanel panel, UIComponent lastButton)
        {
        }

        private UIPanel CreateEditButtons()
        {
            var panel = AddUIComponent<UIPanel>();
            panel.width = width;
            panel.height = 30;
            panel.relativePosition = new Vector3(0, height - panel.height);
            panel.anchor = UIAnchorStyle.Left | UIAnchorStyle.Right | UIAnchorStyle.Bottom;

            CreateEditButtons(panel);

            if (panel.childCount == 0)
            {
                RemoveUIComponent(panel);
                return null;
            }

            return panel;
        }

        protected virtual void CreateEditButtons(UIPanel panel)
        { 
        }

        public void SetData(TrailerDefinition trailerDefinition)
        {
            _trailerDefinition = trailerDefinition;
            UpdateData();
        }

        private void DisableSelected()
        {
            var selected = _itemList.GetSelectedRows();

            if (selected.Count == 0)
                return;

            foreach (var row in selected)
                row.Value.Enabled = false;

            _itemList.Refresh();
        }

        private void EnableSelected()
        {
            var selected = _itemList.GetSelectedRows();

            if (selected.Count == 0)
                return;

            foreach (var row in selected)
                row.Value.Enabled = true;

            _itemList.Refresh();
        }

        private void DeleteSelected()
        {
            var rows = _itemList.GetSelectedRows();

            if (rows.Count == 0)
                return;

            ConfirmPanel.ShowModal(Mod.name, $"Are you sure you want to remove {rows.Count} entries?", delegate (UIComponent comp, int ret)
            {
                if (ret == 1)
                {
                    foreach (var row in rows)
                    {
                        Remove(_trailerDefinition, row.Value);
                        _itemList.Remove(row, false);
                    }
                    _itemList.Refresh();
                    UIDataManager.instance.Invalidate();
                }
            });
        }

        protected abstract void Remove(TrailerDefinition trailerDefinition, DataType item);

        private void DeleteItem(RowData<DataType> rowData)
        {
            Remove(_trailerDefinition, rowData.Value);
            _itemList.Remove(rowData);
            UIDataManager.instance.Invalidate();
        }

        protected abstract IEnumerable<DataType> GetData(TrailerDefinition trailerDefinition);

        protected void UpdateData()
        {
            if (_itemList == null || _trailerDefinition == null)
                return;

            var list = new List<RowData<DataType>>();

            foreach (var item in GetData(_trailerDefinition))
            {
                list.Add(new RowData<DataType>(item, DeleteItem));
            }

            _itemList.Data = list;
        }
    }
}
