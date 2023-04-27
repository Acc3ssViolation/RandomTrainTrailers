using ColossalFramework;
using ColossalFramework.UI;
using System;
using UnityEngine;

namespace RandomTrainTrailers.UI
{
    internal class UIWindow : UIPanel
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

        public bool DestroyOnClose { get; set; }

        public Type ContentType => _contentType;
        public UIPanel Content => _content;

        private string _title;
        private UILabel _titleLabel;
        private UIPanel _content;
        private Type _contentType;

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

            // Resize
            var resizeHandle = AddUIComponent<UIResizeHandle>();
            resizeHandle.height = 9;
            resizeHandle.width = 9;
            resizeHandle.relativePosition = new Vector3(width - resizeHandle.width, height - resizeHandle.height);
            resizeHandle.anchor = UIAnchorStyle.Bottom | UIAnchorStyle.Right;
            resizeHandle.hoverCursor = FindResizeCursor();
            resizeHandle.edges = UIResizeHandle.ResizeEdge.Bottom | UIResizeHandle.ResizeEdge.Right;

            // close button
            UIButton closeButton = UIUtils.CreateButton(this);
            closeButton.size = new Vector2(30, 30);
            closeButton.normalBgSprite = "buttonclose";
            closeButton.hoveredBgSprite = "buttonclosehover";
            closeButton.pressedBgSprite = "buttonclosepressed";
            closeButton.relativePosition = new Vector3(width - 35, 5);
            closeButton.eventClicked += (c, p) => {
                if (DestroyOnClose)
                {
                    Destroy(gameObject);
                }
                else
                {
                    Hide();
                }
            };
            closeButton.anchor = UIAnchorStyle.Top | UIAnchorStyle.Right;

            // Content
            _content = (UIPanel)AddUIComponent(_contentType);
            _content.width = width - 20;
            _content.height = height - 60;
            _content.relativePosition = new Vector3(10, 50);
            _content.anchor = UIAnchorStyle.All;
        }

        private CursorInfo FindResizeCursor()
        {
            var handles = FindObjectsOfType<UIResizeHandle>();
            foreach (var handle in handles)
            {
                if (handle.hoverCursor == null)
                    continue;
                if (handle.hoverCursor.name == "ResizeVertical")
                    return handle.hoverCursor;
            }
            return null;
        }

        public static UIWindow Create<T>(int width, int height, string title) where T : UIPanel
        {
            var go = new GameObject(title);
            var view = UIView.GetAView();
            go.transform.SetParent(view.transform);

            var window = go.AddComponent<UIWindow>();
            window._contentType = typeof(T);
            window.size = new Vector2(width, height);
            window.minimumSize = window.size;
            window.Title = title;

            // We need to do this here instead of in Awake, otherwise _contentType isn't set yet.
            // We could use Start() but I want Content to be set when we exit this method.
            window.CreateComponents();
            return window;
        }
    }
}
