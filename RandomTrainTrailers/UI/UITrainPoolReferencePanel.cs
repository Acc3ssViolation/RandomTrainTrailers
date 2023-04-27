using ColossalFramework.UI;
using RandomTrainTrailers.Definition;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RandomTrainTrailers.UI
{
    internal class UITrainPoolReferencePanel : UIPanel
    {
        public enum DataType
        {
            Locomotives,
            TrailerCollections,
        }

        private TrailerDefinition _trailerDefinition;
        private TrainPool _pool;
        private DataType _type;

        // TODO: Remove this and get some other mechanism to propagate UI refreshes on changes
        private UITrainPoolRow _parentRow;

        private UIFastList _availableList;
        private UIFastList _assignedList;

        public override void Start()
        {
            base.Start();
            // This has to be done in Start, if we try it in Awake the anchors seem to get weird
            CreateComponents();
            UpdateData();
        }

        private void CreateComponents()
        {
            const float Margin = 10;
            float listWidth = (width - Margin) / 2;
            _availableList = CreateList(Vector3.zero, listWidth, "Available", () =>
            {
                AssignSelected();
            });
            _assignedList = CreateList(new Vector3(listWidth + Margin, 0), listWidth, "Assigned", () =>
            {
                RemoveSelected();
            });
        }

        private UIFastList CreateList(Vector3 relativePosition, float width, string title, Action onMoveClicked)
        {
            var topRowPanel = AddUIComponent<UIPanel>();
            topRowPanel.width = width;
            topRowPanel.height = 30;
            topRowPanel.relativePosition = relativePosition;
            topRowPanel.anchor = UIAnchorStyle.Top | UIAnchorStyle.Left | UIAnchorStyle.Right | UIAnchorStyle.Proportional;

            var label = topRowPanel.AddUIComponent<UILabel>();
            label.text = title;
            label.relativePosition = new Vector3(0, 0);
            label.anchor = UIAnchorStyle.Left | UIAnchorStyle.CenterVertical;

            var selectButton = UIUtils.CreateButton(topRowPanel);
            selectButton.text = "Select all";
            selectButton.width = 90;
            selectButton.relativePosition = UIUtils.RightOf(label);
            selectButton.anchor = UIAnchorStyle.Left | UIAnchorStyle.CenterVertical;

            var moveButton = UIUtils.CreateButton(topRowPanel);
            moveButton.text = "Move";
            moveButton.width = 65;
            moveButton.relativePosition = UIUtils.RightOf(selectButton);
            moveButton.anchor = UIAnchorStyle.Left | UIAnchorStyle.CenterVertical;

            var list = UIFastList.Create<UIItemReferenceRow>(this);
            list.relativePosition = UIUtils.Below(topRowPanel);
            list.width = width;
            list.height = height - list.relativePosition.y;
            list.rowHeight = UITrainPoolRow.Height;
            list.backgroundSprite = "UnlockingPanel";
            list.anchor = UIAnchorStyle.All | UIAnchorStyle.Proportional;

            selectButton.eventClicked += (_, __) =>
            {
                SelectAll(list);
            };
            moveButton.eventClicked += (_, __) =>
            {
                onMoveClicked();
            };

            return list;
        }

        public void SetData(TrailerDefinition trailerDefinition, TrainPool pool, DataType type, UITrainPoolRow parentRow)
        {
            _trailerDefinition = trailerDefinition;
            _pool = pool;
            _type = type;
            _parentRow = parentRow;
            UpdateData();
        }

        private void SelectAll(UIFastList list)
        {
            var allSelected = true;

            foreach (var row in list.rowsData)
            {
                var rowData = (RowData<ItemReference>)row;
                if (!rowData.Selected)
                {
                    allSelected = false;
                    break;
                }
            }

            foreach (var row in list.rowsData)
            {
                var rowData = (RowData<ItemReference>)row;
                rowData.Selected = !allSelected;
            }

            list.Refresh();
        }

        private void AssignSelected()
        {
            var rows = GetSelectedRows(_availableList);

            if (rows.Count == 0)
                return;

            foreach (var row in rows)
            {
                _assignedList.rowsData.Add(row);
                _availableList.rowsData.Remove(row);
                if (_type == DataType.Locomotives)
                    _pool.Locomotives.Add((TrainPool.LocomotiveReference)row.Value);
                else if (_type == DataType.TrailerCollections)
                    _pool.TrailerCollections.Add((TrainPool.CollectionReference)row.Value);
            }

            _assignedList.Refresh();
            _availableList.Refresh();
            _parentRow?.UpdateDisplay();
        }

        private void RemoveSelected()
        {
            var rows = GetSelectedRows(_assignedList);

            if (rows.Count == 0)
                return;

            foreach (var row in rows)
            {
                _availableList.rowsData.Add(row);
                _assignedList.rowsData.Remove(row);
                if (_type == DataType.Locomotives)
                    _pool.Locomotives.Remove((TrainPool.LocomotiveReference)row.Value);
                else if (_type == DataType.TrailerCollections)
                    _pool.TrailerCollections.Remove((TrainPool.CollectionReference)row.Value);
            }

            _assignedList.Refresh();
            _availableList.Refresh();
            _parentRow?.UpdateDisplay();
        }

        private List<RowData<ItemReference>> GetSelectedRows(UIFastList list)
        {
            var result = new List<RowData<ItemReference>>();

            foreach (var row in list.rowsData)
            {
                var rowData = (RowData<ItemReference>)row;
                if (rowData.Selected)
                    result.Add(rowData);
            }

            return result;
        }

        private void UpdateData()
        {
            if (_availableList == null || _assignedList == null || _pool == null)
                return;

            var assigned = new FastList<object>();
            var available = new FastList<object>();

            if (_type == DataType.Locomotives)
            {
                foreach (var pool in _pool.Locomotives)
                    assigned.Add(new RowData<ItemReference>(pool, null));

                foreach (var loco in _trailerDefinition.Locomotives)
                {
                    if (_pool.Locomotives.Any(l => l.Name == loco.AssetName))
                        continue;
                    available.Add(new RowData<ItemReference>(new TrainPool.LocomotiveReference { Name = loco.AssetName }, null));
                }
            }
            else if (_type == DataType.TrailerCollections)
            {
                foreach (var pool in _pool.TrailerCollections)
                    assigned.Add(new RowData<ItemReference>(pool, null));

                foreach (var collection in _trailerDefinition.Collections)
                {
                    if (_pool.TrailerCollections.Any(l => l.Name == collection.Name))
                        continue;
                    available.Add(new RowData<ItemReference>(new TrainPool.CollectionReference { Name = collection.Name }, null));
                }
            }
            else
            {
                Util.LogError($"Unknown data type '{_type}'");
                return;
            }

            _availableList.rowsData = available;
            _assignedList.rowsData = assigned;
        }
    }
}
