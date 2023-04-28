using ColossalFramework.UI;
using RandomTrainTrailers.Definition;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RandomTrainTrailers.UI
{
    internal class UITrailersPanel : UIPanel
    {
        private FilterableFastList<Trailer> _trailerList;
        private TrailerDefinition _trailerDefinition;
        private UIButton _importButton;
        private UIButton _deleteButton;
        private UIButton _selectAllButton;
        private UIButton _enableButton;
        private UIButton _disableButton;

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
            _trailerList = CreateList(UIUtils.Below(selectionPanel));
            var buttonPanel = CreateEditButtons();
            _trailerList.UIList.height -= buttonPanel.height + Margin;
        }

        private FilterableFastList<Trailer> CreateList(Vector3 relativePosition)
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
            var list = UIFastList.Create<UITrailerRow>(this);
            list.relativePosition = UIUtils.Below(filterPanel);
            list.width = width;
            list.height = height - list.relativePosition.y;
            list.rowHeight = UITrailerRow.Height;
            list.backgroundSprite = UIConstants.FastListBackground;
            list.anchor = UIAnchorStyle.All;

            var filterable = new FilterableFastList<Trailer>(list);
            filterable.SetFilter((item) =>
            {
                var itemName = item.AssetName.ToUpperInvariant();
                var altItemName = Util.GetVehicleDisplayName(item.AssetName).ToUpperInvariant();
                var filter = filterField.text.ToUpperInvariant();
                return itemName.Contains(filter) || altItemName.Contains(filter);
            });
            filterField.eventTextChanged += (_, __) =>
            {
                filterable.ApplyFilter();
            };

            return filterable;
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
                _trailerList.SelectAll();
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

            _importButton = UIUtils.CreateButton(panel);
            _importButton.text = "Import Trailers";
            _importButton.width = 180;
            _importButton.relativePosition = new Vector3(0, 0);
            _importButton.anchor = UIAnchorStyle.Left | UIAnchorStyle.CenterVertical;
            _importButton.eventClicked += (_, __) =>
            {
                ImportAllTrailers();
            };

            return panel;
        }

        public void SetData(TrailerDefinition trailerDefinition)
        {
            _trailerDefinition = trailerDefinition;
            UpdateData();
        }

        private void ImportAllTrailers()
        {
            ConfirmPanel.ShowModal(Mod.name, $"Are you sure you want to import all available trailer assets?", delegate (UIComponent comp, int ret)
            {
                if (ret == 1)
                {
                    ImportAllTrailersImpl();
                }
            });
        }

        private void ImportAllTrailersImpl()
        {
            var importer = new TrailerImporter();
            var available = UIDataManager.instance.AvailableDefinition;
            var cargoTrains = VehiclePrefabs.cargoTrains;
            foreach (var train in cargoTrains)
            {
                // We only want trailer assets that aren't yet in use as a trailer or as a locomotive
                if (!train.isTrailer || available.Trailers.Any(l => l.VehicleInfos.Any(i => i == train.info)) || available.Locomotives.Any(l => l.VehicleInfo == train.info))
                    continue;

                var trailer = importer.ImportFromAsset(train.info);
                Util.Log($"Imported '{trailer.AssetName}' as trailer for cargo '{trailer.CargoType}'");
                UIDataManager.instance.EditDefinition.Trailers.Add(trailer);
            }

            UIDataManager.instance.Invalidate();
            UpdateData();
        }

        private void DisableSelected()
        {
            var selected = _trailerList.GetSelectedRows();

            if (selected.Count == 0)
                return;

            foreach (var row in selected)
                row.Value.Enabled = false;

            _trailerList.Refresh();
        }

        private void EnableSelected()
        {
            var selected = _trailerList.GetSelectedRows();

            if (selected.Count == 0)
                return;

            foreach (var row in selected)
                row.Value.Enabled = true;

            _trailerList.Refresh();
        }

        private void DeleteSelected()
        {
            var rows = _trailerList.GetSelectedRows();

            if (rows.Count == 0)
                return;

            ConfirmPanel.ShowModal(Mod.name, $"Are you sure you want to remove {rows.Count} entries?", delegate (UIComponent comp, int ret)
            {
                if (ret == 1)
                {
                    foreach (var row in rows)
                    {
                        _trailerDefinition.Trailers.Remove(row.Value);
                        _trailerList.Remove(row, false);
                    }
                    _trailerList.Refresh();
                    UIDataManager.instance.Invalidate();
                }
            });
        }

        private void DeleteTrailer(RowData<Trailer> rowData)
        {
            _trailerDefinition.Trailers.Remove(rowData.Value);
            _trailerList.Remove(rowData);
            UIDataManager.instance.Invalidate();
        }

        private void UpdateData()
        {
            if (_trailerList == null || _trailerDefinition == null)
                return;

            var list = new List<RowData<Trailer>>();

            foreach (var pool in _trailerDefinition.Trailers)
            {
                list.Add(new RowData<Trailer>(pool, DeleteTrailer));
            }

            _trailerList.Data = list;
        }
    }
}
