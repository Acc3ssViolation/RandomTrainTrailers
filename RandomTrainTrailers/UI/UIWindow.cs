using ColossalFramework;
using ColossalFramework.UI;
using System;
using UnityEngine;

namespace RandomTrainTrailers.UI
{
    internal interface IUIWindowPanel
    {
        float DefaultWidth { get; }
        float DefaultHeight { get; }
        string DefaultTitle { get; }
    }

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
            backgroundSprite = "MenuPanel2";
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
            // Use a panel for the title bar. No background sprite so that it looks different from the rest of the panel.
            var titlePanel = AddUIComponent<UIPanel>();
            titlePanel.width = width;
            titlePanel.height = 40;
            titlePanel.relativePosition = Vector3.zero;
            titlePanel.anchor = UIAnchorStyle.Left | UIAnchorStyle.Right | UIAnchorStyle.Top;

            // header text
            _titleLabel = titlePanel.AddUIComponent<UILabel>();
            _titleLabel.text = _title;
            _titleLabel.relativePosition = new Vector3(width / 2 - _titleLabel.width / 2, 13);
            _titleLabel.anchor = UIAnchorStyle.Top | UIAnchorStyle.CenterHorizontal;

            // drag
            UIDragHandle handle = titlePanel.AddUIComponent<UIDragHandle>();
            handle.target = this;
            handle.constrainToScreen = true;
            handle.width = titlePanel.width;
            handle.height = titlePanel.height;
            handle.relativePosition = Vector3.zero;
            handle.anchor = UIAnchorStyle.All;

            // close button
            UIButton closeButton = UIUtils.CreateButton(titlePanel);
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

            // Resize
            var resizeHandle = AddUIComponent<UIResizeHandle>();
            resizeHandle.height = 9;
            resizeHandle.width = 9;
            resizeHandle.relativePosition = new Vector3(width - resizeHandle.width, height - resizeHandle.height);
            resizeHandle.anchor = UIAnchorStyle.Bottom | UIAnchorStyle.Right;
            resizeHandle.hoverCursor = FindResizeCursor();
            resizeHandle.edges = UIResizeHandle.ResizeEdge.Bottom | UIResizeHandle.ResizeEdge.Right;

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

        public static UIWindow Create<T>() where T : UIPanel, IUIWindowPanel
        {
            // There must be a nicer way to do this :p
            var temp = new GameObject().AddComponent<T>();
            var title = temp.DefaultTitle;
            var width = temp.DefaultWidth;
            var height = temp.DefaultHeight;
            Destroy(temp.gameObject);

            return Create<T>(width, height, title);
        }

        private static UIWindow Create<T>(float width, float height, string title) where T : UIPanel
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
