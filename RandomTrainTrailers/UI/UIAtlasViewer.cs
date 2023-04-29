using ColossalFramework.UI;
using RandomTrainTrailers.Definition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RandomTrainTrailers.UI
{
    internal class AtlasSprite : IEnableable
    {
        public UITextureAtlas Atlas;
        public string Name;

        public bool Enabled { get; set; }
    }

    internal class UIAtlasRow : UIPanel, IUIFastListRow
    {
        public static readonly float Height = 48;

        private UIPanel _panel;
        private UILabel _label;

        private void EnsureComponents()
        {
            width = parent.width;
            height = Height;

            if (_panel != null)
                return;

            _panel = AddUIComponent<UIPanel>();
            _panel.relativePosition = Vector3.zero;
            _panel.height = height;
            _panel.width = height;
            _panel.anchor = UIAnchorStyle.Left | UIAnchorStyle.CenterVertical;

            _label = AddUIComponent<UILabel>();
            _label.relativePosition = UIUtils.RightOf(_panel);
            _label.anchor = UIAnchorStyle.Left | UIAnchorStyle.CenterVertical;
        }

        public void Display(object data, bool isRowOdd)
        {
            EnsureComponents();

            if (data is RowData<AtlasSprite> spriteRow)
            {
                var sprite = spriteRow.Value;
                _label.text = $"[{sprite.Atlas.name}] {sprite.Name}";
                _panel.backgroundSprite = null;
                _panel.atlas = sprite.Atlas;
                _panel.backgroundSprite = sprite.Name;
            }
        }

        public void Deselect(bool isRowOdd)
        {
        }

        public void Select(bool isRowOdd)
        {
        }
    }

    internal class UIAtlasViewer : UIBaseListPanel<AtlasSprite, UIAtlasRow>
    {
        public override string DefaultTitle => "Atlas Viewer";

        protected override float RowHeight => UIAtlasRow.Height;

        private IList<UITextureAtlas> _atlases;

        protected override bool Filter(AtlasSprite item, string filter)
        {
            return item.Name.ToUpperInvariant().Contains(filter.ToUpperInvariant());
        }

        protected override IEnumerable<AtlasSprite> GetData(TrailerDefinition trailerDefinition)
        {
            if (_atlases == null)
                _atlases = UIUtils.Atlases;
            return _atlases.SelectMany(a => a.spriteNames.Select(s => new AtlasSprite {  Atlas = a, Name = s }));
        }

        protected override void Remove(TrailerDefinition trailerDefinition, AtlasSprite item)
        {
        }
    }
}
