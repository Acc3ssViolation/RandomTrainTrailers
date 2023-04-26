using ColossalFramework.UI;
using UnityEngine;

namespace RandomTrainTrailers.UI
{
    internal class UIWindow<T> : UIPanel where T: UIPanel
    {
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                if (_title != value)
                {
                    _title = value;
                    if (_titleLabel != null)
                        _titleLabel.text = _title;
                }
            }
        }

        public T Content => _content;

        private string _title;
        private UILabel _titleLabel;
        private T _content;

        public override void Awake()
        {
            base.Awake();

            name = $"{GetType().Name}";
            backgroundSprite = "UnlockingPanel2";
            isVisible = false;
            canFocus = true;
            isInteractive = true;
            width = 600;
            height = 400;
            var view = UIView.GetAView();
            relativePosition = new Vector3(Mathf.Floor((view.fixedWidth - width) / 2), Mathf.Floor((view.fixedHeight - height) / 2));

            CreateComponents();
        }

        public void Open()
        {
            Show();
            BringToFront();
        }

        private void CreateComponents()
        {
            // header text
            _titleLabel = AddUIComponent<UILabel>();
            _titleLabel.text = _title;
            _titleLabel.relativePosition = new Vector3(width / 2 - _titleLabel.width / 2, 13);
            _titleLabel.anchor = UIAnchorStyle.Top | UIAnchorStyle.CenterHorizontal;
            
            // drag
            UIDragHandle handle = AddUIComponent<UIDragHandle>();
            handle.target = this;
            handle.constrainToScreen = true;
            handle.width = width;
            handle.height = 40;
            handle.relativePosition = Vector3.zero;
            handle.anchor = UIAnchorStyle.Left | UIAnchorStyle.Right | UIAnchorStyle.Top;

            // TODO: Resize?
            //var resizeHandle = AddUIComponent<UIResizeHandle>();


            // close button
            UIButton closeButton = UIUtils.CreateButton(this);
            closeButton.size = new Vector2(30, 30);
            closeButton.normalBgSprite = "buttonclose";
            closeButton.hoveredBgSprite = "buttonclosehover";
            closeButton.pressedBgSprite = "buttonclosepressed";
            closeButton.relativePosition = new Vector3(width - 35, 5);
            closeButton.eventClicked += (c, p) => {
                Hide();
            };

            // Content
            _content = AddUIComponent<T>();
            _content.width = width;
            _content.height = height - 50;
            _content.relativePosition = new Vector3(0, 50);
            _content.anchor = UIAnchorStyle.All;
        }

        public static UIWindow<T> Create(int width, int height, string title)
        {
            var go = new GameObject(title);
            var window = go.AddComponent<UIWindow<T>>();
            window.size = new Vector2(width, height);
            window.Title = title;
            return window;
        }
    }
}
