using ColossalFramework.UI;
using RandomTrainTrailers.Definition;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace RandomTrainTrailers.UI
{
    internal class UILocomotivesPanel : UIBaseListPanel<Locomotive, UILocomotiveRow>
    {
        private UIButton _importButton;
        private UIButton _createButton;

        public override string DefaultTitle => "Locomotives";
        protected override float RowHeight => UILocomotiveRow.Height;

        protected override void CreateEditButtons(UIPanel panel)
        {
            _importButton = UIUtils.CreateButton(panel);
            _importButton.text = "Import Locomotives";
            _importButton.width = 180;
            _importButton.relativePosition = new Vector3(0, 0);
            _importButton.anchor = UIAnchorStyle.Left | UIAnchorStyle.CenterVertical;
            _importButton.eventClicked += (_, __) =>
            {
                ImportAllLocomotives();
            };

            _createButton = UIUtils.CreateButton(panel);
            _createButton.relativePosition = UIUtils.RightOf(_importButton);
            _createButton.text = "Create";
            _createButton.anchor = UIAnchorStyle.Left | UIAnchorStyle.CenterVertical;
            _createButton.eventClicked -= (_, __) =>
            {
                CreateLocomotive();
            };
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
            UpdateData();
        }

        private void CreateLocomotive()
        {
            // TODO: Use new panel
            Util.Log("CreateLocomotive 1");
            var findAsset = UIFindAssetPanel.main;
            findAsset.Show((vehicle) =>
            {
                if (vehicle == null)
                    return;

                var available = UIDataManager.instance.AvailableDefinition;
                if (available.Locomotives.Any(l => l.VehicleInfo == vehicle.info) || available.Trailers.Any(l => l.VehicleInfos?.Contains(vehicle.info) ?? false))
                {
                    Util.ShowWarningMessage($"Vehicle {vehicle.localeName} is already in use as a locomotive or trailer");
                    return;
                }

                var importer = new LocomotiveImporter();
                var locomotive = importer.ImportFromAsset(vehicle.info);
                Util.Log($"Imported '{locomotive.AssetName}' as locomotive of type '{locomotive.Type}'");
                UIDataManager.instance.EditDefinition.Locomotives.Add(locomotive);
                UIDataManager.instance.Invalidate();
                UpdateData();
            }, UIFindAssetPanel.DisplayMode.Both);
            Util.Log("CreateLocomotive 2");
        }

        protected override bool Filter(Locomotive item, string filter)
        {
            var itemName = item.AssetName.ToUpperInvariant();
            var altItemName = Util.GetVehicleDisplayName(item.AssetName).ToUpperInvariant();
            filter = filter.ToUpperInvariant();
            return itemName.Contains(filter) || altItemName.Contains(filter);
        }

        protected override void Remove(TrailerDefinition trailerDefinition, Locomotive item)
        {
            trailerDefinition.Locomotives.Remove(item);
        }

        protected override IEnumerable<Locomotive> GetData(TrailerDefinition trailerDefinition)
        {
            return trailerDefinition.Locomotives;
        }
    }
}
