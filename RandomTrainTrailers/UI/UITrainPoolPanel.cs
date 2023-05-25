using ColossalFramework.UI;
using RandomTrainTrailers.Definition;
using System.Collections.Generic;
using UnityEngine;

namespace RandomTrainTrailers.UI
{
    internal class UITrainPoolPanel : UIBaseListPanel<TrainPool, UITrainPoolRow>
    {
        public override string DefaultTitle => "Train Pools";
        protected override float RowHeight => UITrainPoolRow.Height;

        protected override void CreateEditButtons(UIPanel panel)
        {
            var _createButton = UIUtils.CreateButton(panel);
            _createButton.text = "Create Pool";
            _createButton.width = 150;
            _createButton.relativePosition = new Vector3();
            _createButton.anchor = UIAnchorStyle.Left | UIAnchorStyle.CenterVertical;
            _createButton.eventClicked += (_, __) =>
            {
                CreatePool();
            };
        }

        protected override bool Filter(TrainPool item, string filter)
            => item.Name.ToUpperInvariant().Contains(filter.ToUpperInvariant());

        protected override IEnumerable<TrainPool> GetData(TrailerDefinition trailerDefinition)
            => trailerDefinition.TrainPools;

        protected override void Remove(TrailerDefinition trailerDefinition, TrainPool item)
            => trailerDefinition.TrainPools.Remove(item);

        private void CreatePool()
        {
            var pool = new TrainPool()
            {
                Enabled = true,
                Name = "New Pool",
                UseCargo = true,
                MaxLocomotiveCount = 1,
                MinLocomotiveCount = 1,
                MaxTrainLength = 10,
                MinTrainLength = 10,
            };
            UIDataManager.instance.EditDefinition.TrainPools.Add(pool);
            UIDataManager.instance.Invalidate();
            UpdateData();
        }
    }
}
