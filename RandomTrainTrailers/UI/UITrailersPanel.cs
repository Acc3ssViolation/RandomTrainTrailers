using ColossalFramework.UI;
using RandomTrainTrailers.Definition;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RandomTrainTrailers.UI
{
    internal class UITrailersPanel : UIBaseListPanel<Trailer, UITrailerRow>
    {
        private UIButton _importButton;

        public override string DefaultTitle => "Trailers";
        protected override float RowHeight => UITrailerRow.Height;

        protected override void CreateEditButtons(UIPanel panel)
        {
            _importButton = UIUtils.CreateButton(panel);
            _importButton.text = "Import Trailers";
            _importButton.width = 180;
            _importButton.relativePosition = new Vector3(0, 0);
            _importButton.anchor = UIAnchorStyle.Left | UIAnchorStyle.CenterVertical;
            _importButton.eventClicked += (_, __) =>
            {
                ImportAllTrailers();
            };
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
            importer.SetTrailers(TrailerDefinition);
            var available = UIDataManager.instance.AvailableDefinition;
            var cargoTrains = VehiclePrefabs.CargoTrains;
            foreach (var train in cargoTrains)
            {
                // We only want trailer assets that aren't yet in use as a trailer or as a locomotive
                if (!train.isTrailer || available.Trailers.Any(l => (l.VehicleInfos?.Any(i => i == train.info)) ?? false) || available.Locomotives.Any(l => l.VehicleInfo == train.info))
                    continue;

                var trailer = importer.ImportFromAsset(train.info);
                Util.Log($"Imported '{trailer.AssetName}' as trailer for cargo '{trailer.CargoType}'");
                UIDataManager.instance.EditDefinition.Trailers.Add(trailer);
            }

            UIDataManager.instance.Invalidate();
            UpdateData();
        }

        protected override bool Filter(Trailer item, string filter)
        {
            var itemName = item.AssetName.ToUpperInvariant();
            var altItemName = Util.GetVehicleDisplayName(item.AssetName).ToUpperInvariant();
            filter = filter.ToUpperInvariant();
            return itemName.Contains(filter) || altItemName.Contains(filter);
        }

        protected override void Remove(TrailerDefinition trailerDefinition, Trailer item)
            => trailerDefinition.Trailers.Remove(item);

        protected override IEnumerable<Trailer> GetData(TrailerDefinition trailerDefinition)
            => trailerDefinition.Trailers;
    }
}
