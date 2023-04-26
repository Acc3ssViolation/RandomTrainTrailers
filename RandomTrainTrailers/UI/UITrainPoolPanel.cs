using ColossalFramework.UI;
using RandomTrainTrailers.Definition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static ColossalFramework.IO.EncodedArray;

namespace RandomTrainTrailers.UI
{

    internal class UITrainPoolPanel : UIPanel
    {
        private UIFastList _poolList;
        private TrailerDefinition _trailerDefinition;
        private UIButton _createButton;

        public override void Awake()
        {
            base.Awake();
            CreateComponents();
        }

        private void CreateComponents()
        {
            const float Margin = 10;

            var buttonPanel = CreateEditButtons();
            _poolList = UIFastList.Create<UITrainPoolRow>(this);
            _poolList.relativePosition = new Vector3(Margin, Margin);
            _poolList.size = new Vector2(width - Margin * 2, height - 3 * Margin - buttonPanel.height);
            _poolList.anchor = UIAnchorStyle.All;
        }

        private UIPanel CreateEditButtons()
        {
            const float Margin = 10;

            var panel = AddUIComponent<UIPanel>();
            panel.width = width - 2 * Margin;
            panel.height = 30;
            panel.relativePosition = new Vector3(Margin, height - Margin - panel.height);
            panel.anchor = UIAnchorStyle.Left | UIAnchorStyle.Right | UIAnchorStyle.Bottom;

            _createButton = UIUtils.CreateButton(this);
            _createButton.text = "Create";
            _createButton.relativePosition = new Vector3(0, 0);
            _createButton.anchor = UIAnchorStyle.Left | UIAnchorStyle.CenterVertical;
            _createButton.eventClicked += (_, __) =>
            {
                CreatePool();
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

        private void DeletePool(RowData<TrainPool> rowData)
        {
            _trailerDefinition.TrainPools.Remove(rowData.Value);
            _poolList.rowsData.Remove(rowData);
            _poolList.Refresh();
        }

        private void UpdateData()
        {
            var list = new FastList<object>();

            foreach (var pool in _trailerDefinition.TrainPools)
            {
                list.Add(new RowData<TrainPool>(pool, DeletePool));
            }

            _poolList.rowsData = list;
        }
    }
}
