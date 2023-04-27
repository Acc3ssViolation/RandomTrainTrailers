using ColossalFramework.UI;
using RandomTrainTrailers.Definition;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RandomTrainTrailers.UI
{
    internal class UITrainPoolPanel : UIPanel
    {
        private UIFastList _poolList;
        private TrailerDefinition _trailerDefinition;
        private UIButton _createButton;
        private UIButton _deleteButton;
        private UIButton _selectAllButton;
        private UIButton _enableButton;
        private UIButton _disableButton;

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

            var selectionPanel = CreateSelectionButtons();

            // Main list of items
            _poolList = UIFastList.Create<UITrainPoolRow>(this);
            _poolList.relativePosition = UIUtils.Below(selectionPanel);
            _poolList.width = width;
            _poolList.height = height - (selectionPanel.height + Margin);
            //_poolList.ResetLayout(false, true);
            _poolList.rowHeight = UITrainPoolRow.Height;
            _poolList.backgroundSprite = "UnlockingPanel";
            _poolList.anchor = UIAnchorStyle.All;

            var buttonPanel = CreateEditButtons();
            _poolList.height -= buttonPanel.height + Margin;
        }

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
                SelectAll();
            };

            _deleteButton = UIUtils.CreateButton(panel);
            _deleteButton.text = "Delete selected";
            _deleteButton.width = 150;
            _deleteButton.relativePosition = UIUtils.RightOf(_selectAllButton);
            _deleteButton.anchor = UIAnchorStyle.Left | UIAnchorStyle.CenterVertical;
            _deleteButton.eventClicked += (_, __) =>
            {
                DeleteSelected();
            };

            _enableButton = UIUtils.CreateButton(panel);
            _enableButton.text = "Enable selected";
            _enableButton.width = 150;
            _enableButton.relativePosition = UIUtils.RightOf(_deleteButton);
            _enableButton.anchor = UIAnchorStyle.Left | UIAnchorStyle.CenterVertical;
            _enableButton.eventClicked += (_, __) =>
            {
                EnableSelected();
            };

            _disableButton = UIUtils.CreateButton(panel);
            _disableButton.text = "Disable selected";
            _disableButton.width = 150;
            _disableButton.relativePosition = UIUtils.RightOf(_enableButton);
            _disableButton.anchor = UIAnchorStyle.Left | UIAnchorStyle.CenterVertical;
            _disableButton.eventClicked += (_, __) =>
            {
                DisableSelected();
            };

            return panel;
        }

        private UIPanel CreateEditButtons()
        {
            var panel = AddUIComponent<UIPanel>();
            panel.width = width;
            panel.height = 30;
            panel.relativePosition = new Vector3(0, height - panel.height);
            //panel.ResetLayout(false, true);
            panel.anchor = UIAnchorStyle.Left | UIAnchorStyle.Right | UIAnchorStyle.Bottom;

            _createButton = UIUtils.CreateButton(panel);
            _createButton.text = "Create";
            _createButton.relativePosition = new Vector3(0, 0);
            _createButton.anchor = UIAnchorStyle.Left | UIAnchorStyle.CenterVertical;
            _createButton.eventClicked += (_, __) =>
            {
                CreatePool();
            };

            var importLocomotives = UIUtils.CreateButton(panel);
            importLocomotives.text = "Import Locomotives";
            importLocomotives.width = 180;
            importLocomotives.relativePosition = UIUtils.RightOf(_createButton);
            importLocomotives.anchor = UIAnchorStyle.Left | UIAnchorStyle.CenterVertical;
            importLocomotives.eventClicked += (_, __) =>
            {
                ImportAllLocomotives();
            };

            return panel;
        }

        public void SetData(TrailerDefinition trailerDefinition)
        {
            _trailerDefinition = trailerDefinition;
            UpdateData();
        }

        private void CreatePool()
        {
            var pool = new TrainPool
            {
                Name = $"New pool {_trailerDefinition.TrainPools.Count}",
            };
            _trailerDefinition.TrainPools.Add(pool);
            _poolList.rowsData.Add(new RowData<TrainPool>(pool, DeletePool));
            _poolList.Refresh();
        }

        private void ImportAllLocomotives()
        {
            ConfirmPanel.ShowModal(Mod.name, $"Are you sure you want to import all available locomotive assets?", delegate (UIComponent comp, int ret)
            {
                if (ret == 1)
                {
                    ImportAllLocomotivesImpl();
                }
            });
        }

        private void ImportAllLocomotivesImpl()
        {
            var importer = new LocomotiveImporter();
            var available = UIDataManager.instance.AvailableDefinition;
            var cargoTrains = VehiclePrefabs.cargoTrains;
            foreach (var train in cargoTrains)
            {
                if (train.isTrailer || available.Locomotives.Any(l => l.VehicleInfo == train.info))
                    continue;

                var locomotive = importer.ImportFromAsset(train.info);
                Util.Log($"Imported '{locomotive.AssetName}' as locomotive of type '{locomotive.Type}'");
                UIDataManager.instance.EditDefinition.Locomotives.Add(locomotive);
            }

            UIDataManager.instance.Invalidate();
        }

        private void DisableSelected()
        {
            var selected = GetSelectedRows();

            if (selected.Count == 0)
                return;

            foreach (var row in selected)
                row.Value.Enabled = false;

            _poolList.Refresh();
        }

        private void EnableSelected()
        {
            var selected = GetSelectedRows();

            if (selected.Count == 0)
                return;

            foreach (var row in selected)
                row.Value.Enabled = true;

            _poolList.Refresh();
        }

        private void SelectAll()
        {
            var allSelected = true;

            foreach (var row in _poolList.rowsData)
            {
                var rowData = (RowData<TrainPool>)row;
                if (!rowData.Selected)
                {
                    allSelected = false;
                    break;
                }
            }

            foreach (var row in _poolList.rowsData)
            {
                var rowData = (RowData<TrainPool>)row;
                rowData.Selected = !allSelected;
            }

            _poolList.Refresh();
        }

        private void DeleteSelected()
        {
            var rows = GetSelectedRows();

            if (rows.Count == 0)
                return;

            ConfirmPanel.ShowModal(Mod.name, $"Are you sure you want to remove {rows.Count} entries?", delegate (UIComponent comp, int ret)
            {
                if (ret == 1)
                {
                    foreach (var row in rows)
                    {
                        _trailerDefinition.TrainPools.Remove(row.Value);
                        _poolList.rowsData.Remove(row);
                    }
                    _poolList.Refresh();
                }
            });
        }

        private List<RowData<TrainPool>> GetSelectedRows()
        {
            var result = new List<RowData<TrainPool>>();

            foreach (var row in _poolList.rowsData)
            {
                var rowData = (RowData<TrainPool>)row;
                if (rowData.Selected)
                    result.Add(rowData);
            }

            return result;
        }

        private void DeletePool(RowData<TrainPool> rowData)
        {
            _trailerDefinition.TrainPools.Remove(rowData.Value);
            _poolList.rowsData.Remove(rowData);
            _poolList.Refresh();
        }

        private void UpdateData()
        {
            if (_poolList == null || _trailerDefinition == null)
                return;

            var list = new FastList<object>();

            foreach (var pool in _trailerDefinition.TrainPools)
            {
                list.Add(new RowData<TrainPool>(pool, DeletePool));
            }

            _poolList.rowsData = list;
        }
    }
}
