using RandomTrainTrailers.Definition;
using System.Collections.Generic;

namespace RandomTrainTrailers.UI
{
    internal class UITrainPoolPanel : UIBaseListPanel<TrainPool, UITrainPoolRow>
    {
        protected override float RowHeight => UITrainPoolRow.Height;

        protected override bool Filter(TrainPool item, string filter)
            => item.Name.ToUpperInvariant().Contains(filter.ToUpperInvariant());

        protected override IEnumerable<TrainPool> GetData(TrailerDefinition trailerDefinition)
            => trailerDefinition.TrainPools;

        protected override void Remove(TrailerDefinition trailerDefinition, TrainPool item)
            => trailerDefinition.TrainPools.Remove(item);
    }
}
