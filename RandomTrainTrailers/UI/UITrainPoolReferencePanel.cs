using ColossalFramework.UI;
using RandomTrainTrailers.Definition;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace RandomTrainTrailers.UI
{
    internal class UITrainPoolReferencePanel : UIWindowPanel
    {
        public enum DataType
        {
            Locomotives,
            Trailers,
        }

        public override float DefaultWidth => 600;
        public override float DefaultHeight => 500;
        public override string DefaultTitle => GetTitle(DataType.Locomotives);

        private TrainPool _pool;
        private DataType _type;

        // TODO: Remove this and get some other mechanism to propagate UI refreshes on changes
        private UITrainPoolRow _parentRow;

        private FilterableFastList<ItemReference> _availableList;
        private FilterableFastList<ItemReference> _assignedList;

        public override void Start()
        {
            base.Start();
            // This has to be done in Start, if we try it in Awake the anchors seem to get weird
            CreateComponents();
            UpdateData();
        }

        private void CreateComponents()
        {
            _availableList = CreateList(Vector3.zero, "Available", () =>
            {
                AssignSelected();
            });
            _assignedList = CreateList(new Vector3(width / 2, 0), "Assigned", () =>
            {
                RemoveSelected();
            });
        }

        private FilterableFastList<ItemReference> CreateList(Vector3 relativePosition, string title, Action onMoveClicked)
        {
            var totalPanel = AddUIComponent<UIPanel>();
            totalPanel.width = width / 2;
            totalPanel.height = height;
            totalPanel.relativePosition = relativePosition;
            totalPanel.anchor = UIAnchorStyle.All | UIAnchorStyle.Proportional;

            // The top row with buttons
            var topRowPanel = totalPanel.AddUIComponent<UIPanel>();
            topRowPanel.width = totalPanel.width;
            topRowPanel.height = 30;
            topRowPanel.relativePosition = relativePosition.x > 0 ? new Vector3(5, 0) : new Vector3(0, 0);
            topRowPanel.anchor = UIAnchorStyle.Top | UIAnchorStyle.Left | UIAnchorStyle.Right;

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

            // The row with filter options
            var filterRowPanel = totalPanel.AddUIComponent<UIPanel>();
            filterRowPanel.width = totalPanel.width;
            filterRowPanel.height = 30;
            filterRowPanel.relativePosition = UIUtils.Below(topRowPanel);
            filterRowPanel.anchor = UIAnchorStyle.Top | UIAnchorStyle.Left | UIAnchorStyle.Right;

            var filterLabel = filterRowPanel.AddUIComponent<UILabel>();
            filterLabel.text = "Filter";
            filterLabel.relativePosition = new Vector3(0, 0);
            filterLabel.anchor = UIAnchorStyle.Left | UIAnchorStyle.CenterVertical;

            var filterField = UIUtils.CreateTextField(filterRowPanel);
            filterField.relativePosition = UIUtils.RightOf(filterLabel);
            filterField.width = 230;
            filterField.anchor = UIAnchorStyle.Left | UIAnchorStyle.CenterVertical;

            // The panel itself
            var list = UIFastList.Create<UIItemReferenceRow>(totalPanel);
            list.relativePosition = UIUtils.Below(filterRowPanel);
            list.width = totalPanel.width - 5;
            list.height = height - list.relativePosition.y;
            list.rowHeight = UITrainPoolRow.Height;
            list.backgroundSprite = "UnlockingPanel";
            list.anchor = UIAnchorStyle.All;

            var filterable = new FilterableFastList<ItemReference>(list);
            filterable.SetFilter((item) =>
            {
                // TODO: Add things like /enabled /disabled, etc.
                var filter = filterField.text.ToUpperInvariant();
                return item.Name.ToUpperInvariant().Contains(filter) || item.DisplayName.ToUpperInvariant().Contains(filter);
            });

            filterField.eventTextChanged += (_, __) =>
            {
                filterable.ApplyFilter();
            };
            selectButton.eventClicked += (_, __) =>
            {
                filterable.SelectAll();
            };
            moveButton.eventClicked += (_, __) =>
            {
                onMoveClicked();
            };

            return filterable;
        }

        public void SetData(TrainPool pool, DataType type, UITrainPoolRow parentRow)
        {
            _pool = pool;
            _type = type;
            _parentRow = parentRow;
            UpdateData();
        }

        private void AssignSelected()
        {
            var rows = _availableList.GetSelectedRows();

            if (rows.Count == 0)
                return;

            foreach (var row in rows)
            {
                _assignedList.Add(row, false);
                _availableList.Remove(row, false);
                if (_type == DataType.Locomotives)
                    _pool.Locomotives.Add((TrainPool.LocomotiveReference)row.Value);
                else if (_type == DataType.Trailers)
                    _pool.Trailers.Add((TrainPool.TrailerReference)row.Value);
            }

            _assignedList.ApplyFilter();
            _availableList.ApplyFilter();
            _parentRow?.UpdateDisplay();
        }

        private void RemoveSelected()
        {
            var rows = _assignedList.GetSelectedRows();

            if (rows.Count == 0)
                return;

            foreach (var row in rows)
            {
                _availableList.Add(row, false);
                _assignedList.Remove(row, false);
                if (_type == DataType.Locomotives)
                    _pool.Locomotives.Remove((TrainPool.LocomotiveReference)row.Value);
                else if (_type == DataType.Trailers)
                    _pool.Trailers.Remove((TrainPool.TrailerReference)row.Value);
            }

            _assignedList.ApplyFilter();
            _availableList.ApplyFilter();
            _parentRow?.UpdateDisplay();
        }

        private void UpdateData()
        {
            if (_availableList == null || _assignedList == null || _pool == null)
                return;

            Window.Title = GetTitle(_type);

            var assigned = new List<RowData<ItemReference>>();
            var available = new List<RowData<ItemReference>>();

            var availableDef = UIDataManager.instance.AvailableDefinition;

            if (_type == DataType.Locomotives)
            {
                foreach (var locomotiveRef in _pool.Locomotives)
                    assigned.Add(new RowData<ItemReference>(locomotiveRef, null));

                foreach (var locomotive in availableDef.Locomotives)
                {
                    if (_pool.Locomotives.Any(l => l.Name == locomotive.AssetName))
                        continue;
                    available.Add(new RowData<ItemReference>(new TrainPool.LocomotiveReference(locomotive), null));
                }
            }
            else if (_type == DataType.Trailers)
            {
                foreach (var trailerRef in _pool.Trailers)
                    assigned.Add(new RowData<ItemReference>(trailerRef, null));

                foreach (var trailer in availableDef.Trailers)
                {
                    if (_pool.Trailers.Any(l => l.Name == trailer.AssetName))
                        continue;
                    available.Add(new RowData<ItemReference>(new TrainPool.TrailerReference(trailer), null));
                }
            }
            else
            {
                Util.LogError($"Unknown data type '{_type}'");
                return;
            }

            _availableList.Data = available;
            _assignedList.Data = assigned;
        }

        private static string GetTitle(DataType type)
        {
            if (type == DataType.Locomotives)
                return "Locomotives in pool";
            return "Trailers in pool";
        }
    }
}
