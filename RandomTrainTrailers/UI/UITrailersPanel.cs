using ColossalFramework.UI;
using RandomTrainTrailers.Definition;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RandomTrainTrailers.UI
{
    internal class UITrailersPanel : UIBaseListPanel<Trailer, UITrailerRow>
    {
        private UIButton _autoImportButton;
        private UIButton _createButton;
        private UIButton _setCargoButton;

        public override string DefaultTitle => "Trailers";
        protected override float RowHeight => UITrailerRow.Height;

        protected override void CreateExtraSelectionButtons(UIPanel panel, UIComponent lastButton)
        {
            _setCargoButton = UIUtils.CreateButton(panel);
            _setCargoButton.text = "Cargo";
            _setCargoButton.relativePosition = UIUtils.RightOf(lastButton);
            _setCargoButton.anchor = UIAnchorStyle.Left | UIAnchorStyle.CenterVertical;
            _setCargoButton.eventClicked += (_, __) =>
            {
                EditSelectedCargo();
            };
        }

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

            _createButton = UIUtils.CreateButton(panel);
            _createButton.text = "Create";
            _createButton.relativePosition = UIUtils.RightOf(_autoImportButton);
            _createButton.anchor = UIAnchorStyle.Left | UIAnchorStyle.CenterVertical;
            _createButton.eventClicked += (_, __) =>
            {
                CreateTrailer();
            };
        }

        private void EditSelectedCargo()
        {
            var rows = List.GetSelectedRows();
            if (rows.Count == 0)
                return;

            var combinedFlags = CargoFlags.None;
            foreach (var row in rows)
                combinedFlags |= row.Value.CargoType;

            UIFlagsPanel.Main.Content.Show(combinedFlags, (flags) =>
            {
                foreach (var row in rows)
                    row.Value.CargoType = flags;
                List.Refresh();
            });
        }

        private void ImportAllTrailers()
        {
            ConfirmPanel.ShowModal(Mod.name, $"This will import all trailers from legacy vehicle configurations and trailer collections. Are you sure you want to do this?", delegate (UIComponent comp, int ret)
            {
                if (ret == 1)
                {
                    ImportAllTrailersImpl();
                }
            });
        }

        private void CreateTrailer()
        {
            UIFindAssetPanel.Main.Content.Show((vehicle) =>
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
            var available = UIDataManager.instance.AvailableDefinition;
            var addedTrailers = new HashSet<string>();
            foreach (var collection in available.Collections)
            {
                foreach (var trailer in collection.Trailers)
                {
                    if (trailer.IsCollection)
                        continue;
                    if (available.Trailers.Any(l => l.AssetName == trailer.AssetName))
                        continue;
                    // Had some problems with trailers existing in multiple places, this should filter that out
                    if (addedTrailers.Contains(trailer.AssetName))
                        continue;

                    Util.Log($"Imported '{trailer.AssetName}' as trailer from collection '{collection.Name}'");
                    UIDataManager.instance.EditDefinition.Trailers.Add(trailer.Copy());
                    addedTrailers.Add(trailer.AssetName);
                }
            }

            foreach (var vehicle in available.Vehicles)
            {
                foreach (var trailer in vehicle.Trailers)
                {
                    if (trailer.IsCollection)
                        continue;
                    if (available.Trailers.Any(l => l.AssetName == trailer.AssetName))
                        continue;

                    Util.Log($"Imported '{trailer.AssetName}' as trailer from vehicle '{vehicle.AssetName}'");
                    UIDataManager.instance.EditDefinition.Trailers.Add(trailer.Copy());
                }
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
