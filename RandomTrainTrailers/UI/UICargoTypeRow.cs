using ColossalFramework;
using ColossalFramework.UI;
using System.Collections.Generic;

namespace RandomTrainTrailers.UI
{
    internal class UICargoTypeRow : UIPanel
    {
        private struct Sprite
        {
            public string Atlas;
            public string Name;
        }

        private static readonly Dictionary<CargoFlags, Sprite> CargoSprites = new Dictionary<CargoFlags, Sprite>
        {
            { CargoFlags.Oil, new Sprite { Atlas = "Ingame", Name = "resourceIconOil"} },
            { CargoFlags.Petrol, new Sprite { Atlas = "Ingame", Name = "resourceIconPetroleum"} },
            { CargoFlags.AnimalProducts, new Sprite { Atlas = "Ingame", Name = "resourceIconAnimalProducts"} },
            { CargoFlags.Grain, new Sprite { Atlas = "Ingame", Name = "resourceIconflours"} },
            { CargoFlags.Food, new Sprite { Atlas = "Ingame", Name = "resourceIconFood"} },
            { CargoFlags.Goods, new Sprite { Atlas = "Ingame", Name = "resourceIconGoods"} },
            { CargoFlags.Metals, new Sprite { Atlas = "Ingame", Name = "resourceIconMetal"} },
            { CargoFlags.Ore, new Sprite { Atlas = "Ingame", Name = "resourceIconOre"} },
            { CargoFlags.Coal, new Sprite { Atlas = "Ingame", Name = "resourceIconCoal"} },
            { CargoFlags.Logs, new Sprite { Atlas = "Ingame", Name = "resourceIconLogs"} },
            { CargoFlags.Lumber, new Sprite { Atlas = "Ingame", Name = "resourceIconPlanedTimber"} },
            { CargoFlags.None, new Sprite { Atlas = "Ingame", Name = "IconGenericIndustry"} },
            { CargoFlags.Mail, new Sprite { Atlas = "Ingame", Name = "InfoIconPost"} },
        };

        private static CargoFlags GetSummarizedFlags(CargoFlags flags)
        {
            var result = CargoFlags.None;

            if (flags.IsFlagSet(CargoFlags.Oil) || flags.IsFlagSet(CargoFlags.Petrol))
                result |= CargoFlags.Oil;
            if (flags.IsFlagSet(CargoFlags.Lumber) || flags.IsFlagSet(CargoFlags.Logs))
                result |= CargoFlags.Logs;
            if (flags.IsFlagSet(CargoFlags.AnimalProducts) || flags.IsFlagSet(CargoFlags.Grain) || flags.IsFlagSet(CargoFlags.Food))
                result |= CargoFlags.Food;
            if (flags.IsFlagSet(CargoFlags.Coal) || flags.IsFlagSet(CargoFlags.Ore))
                result |= CargoFlags.Ore;
            if (flags.IsFlagSet(CargoFlags.Metals) || flags.IsFlagSet(CargoFlags.Goods) || flags.IsFlagSet(CargoFlags.Mail))
                result |= CargoFlags.Goods;

            return result;
        }

        private List<UIPanel> _childPanels;
        private bool _summarized;
        private CargoFlags _flags;

        public bool Summarized
        {
            get => _summarized;
            set
            {
                if (_summarized != value)
                {
                    _summarized = value;
                    UpdatePanels();
                }
            }
        }

        public CargoFlags Flags
        {
            get => _flags;
            set
            {
                if (_flags != value)
                {
                    _flags = value;
                    tooltip = _flags.ToString();
                    UpdatePanels();
                }
            }
        }

        public override void Awake()
        {
            base.Awake();
            CreateComponents();
            UpdatePanels();
        }

        private void CreateComponents()
        {
            width = 100;
            height = 30;

            if (_childPanels != null)
                return;

            _childPanels = new List<UIPanel>();
            autoLayout = true;
            autoFitChildrenHorizontally = true;
            wrapLayout = false;
            autoLayoutDirection = LayoutDirection.Horizontal;
            autoLayoutStart = LayoutStart.TopRight;
            autoLayoutPadding = new UnityEngine.RectOffset(0, 5, 0, 0);
            
            for (var i = 0; i < CargoSprites.Count; i++)
            {
                var panel = AddUIComponent<UIPanel>();
                panel.name = "Icon";
                panel.width = height;
                panel.height = height;
                panel.isVisible = false;
                _childPanels.Add(panel);
            }
        }

        private void UpdatePanels()
        {
            if (_childPanels == null)
                return;

            var flags = _summarized ? GetSummarizedFlags(_flags) : _flags;
            var i = 0;
            foreach (var cargoSprite in CargoSprites) 
            {
                if (flags.IsFlagSet(cargoSprite.Key))
                {
                    _childPanels[i].atlas = UIUtils.GetAtlas(cargoSprite.Value.Atlas);
                    _childPanels[i].backgroundSprite = cargoSprite.Value.Name;
                    _childPanels[i].isVisible = true;
                    i++;
                }
            }
            if (i == 0)
            {
                var cargoSprite = CargoSprites[CargoFlags.None];
                _childPanels[i].atlas = UIUtils.GetAtlas(cargoSprite.Atlas);
                _childPanels[i].backgroundSprite = cargoSprite.Name;
                _childPanels[i].isVisible = true;
                i++;
            }
            for (; i < _childPanels.Count; i++)
                _childPanels[i].isVisible = false;
        }
    }
}
