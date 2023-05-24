using ColossalFramework.UI;
using RandomTrainTrailers.Definition;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace RandomTrainTrailers.UI
{
    internal class UITrailersPanel : UIBaseListPanel<Trailer, UITrailerRow>
    {
        private UIButton _autoImportButton;
        private UIButton _importButton;

        public override string DefaultTitle => "Trailers";
        protected override float RowHeight => UITrailerRow.Height;

        protected override void CreateEditButtons(UIPanel panel)
        {
            _autoImportButton = UIUtils.CreateButton(panel);
            _autoImportButton.text = "Import Trailers";
            _autoImportButton.width = 180;
            _autoImportButton.relativePosition = new Vector3(0, 0);
            _autoImportButton.anchor = UIAnchorStyle.Left | UIAnchorStyle.CenterVertical;
            _autoImportButton.eventClicked += (_, __) =>
            {
                ImportAllTrailers();
            };

            _importButton = UIUtils.CreateButton(panel);
            _importButton.text = "Create Trailer";
            _importButton.width = 180;
            _importButton.relativePosition = UIUtils.RightOf(_autoImportButton);
            _importButton.anchor = UIAnchorStyle.Left | UIAnchorStyle.CenterVertical;
            _importButton.eventClicked += (_, __) =>
            {
                ImportTrailer();
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

        private void ImportTrailer()
        {
            UIFindAssetPanel.main.Show((vehicle) =>
            {
                var window = UIWindow.Create<UITrailerImportPanel>();
                window.Window.DestroyOnClose = true;
                window.Content.SetData(vehicle.info, (trailer) =>
                {
                    var available = UIDataManager.instance.AvailableDefinition;
                    if (trailer.IsMultiTrailer)
                    {
                        // Find an unused name for it
                        var baseName = trailer.AssetName;
                        var postfix = 1;
                        var name = $"{baseName} {postfix}";
                        while (available.Trailers.Any(l => l.AssetName == name))
                        {
                            postfix++;
                            name = $"{baseName} {postfix}";
                        }
                        trailer.AssetName = name;
                    }
                    else
                    {
                        // Do not allow duplicates
                        if (available.Trailers.Any(l => (l.AssetName == trailer.AssetName)))
                            Util.ShowWarningMessage($"Trailer {trailer.AssetName} already exists");
                    }

                    Util.Log($"Created '{trailer.AssetName}' as trailer for cargo '{trailer.CargoType}'");
                    UIDataManager.instance.EditDefinition.Trailers.Add(trailer);
                    UIDataManager.instance.Invalidate();
                    UpdateData();
                });
                window.Open();
            },
            UIFindAssetPanel.DisplayMode.Engines);
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
