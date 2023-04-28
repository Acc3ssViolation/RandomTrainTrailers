using ColossalFramework.UI;
using RandomTrainTrailers.Definition;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RandomTrainTrailers.UI
{
    internal class UILocomotivesPanel : UIBaseListPanel<Locomotive, UILocomotiveRow>
    {
        private UIButton _importButton;

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
