using ColossalFramework.UI;
using RandomTrainTrailers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RandomTrainTrailers.UI
{
    public class UIFlagsPanel : UIPanel
    {
        public static UIFlagsPanel Main { get; private set; }

        public const int WIDTH = 250;
        public const int HEIGHT = 400;

        public Dictionary<UICheckBox, CargoFlags> m_boxFlagDict = new Dictionary<UICheckBox, CargoFlags>();
        public Dictionary<CargoFlags, UICheckBox> m_flagBoxDict = new Dictionary<CargoFlags, UICheckBox>();

        private UILabel m_label;
        private UIPanel m_flagsPanel;
        private UIButton m_confirmButton;

        public delegate void OnFlagsSet(CargoFlags flags);

        private OnFlagsSet m_callback1;

        public override void Start()
        {
            Main = this;
            base.Start();

            UIView view = UIView.GetAView();
            width = WIDTH;
            height = HEIGHT;
            backgroundSprite = "MenuPanel2";
            name = Mod.name + " Flags Panel";
            canFocus = true;
            isInteractive = true;
            isVisible = false;
            relativePosition = new Vector3(Mathf.FloorToInt((view.fixedWidth - width) / 2), Mathf.FloorToInt((view.fixedHeight - height) / 2));

            CreateComponents();
        }

        public void Show(string title, CargoFlags checkedFlags, OnFlagsSet callback, bool allowEdit = true)
        {
            //Util.Log("-------------------------------------------");
            //Util.Log("checkedFlags: " + checkedFlags.ToString());
            m_callback1 = callback;
            m_label.text = title;

            m_flagsPanel.isVisible = true;

            foreach(var kv in m_flagBoxDict)
            {
                kv.Value.isChecked = ((checkedFlags & kv.Key) > 0);
               // Util.Log(kv.Key.ToString() + " - " + kv.Value.isChecked.ToString());
            }

            Show(true);
            m_confirmButton.isEnabled = allowEdit;
            m_label.relativePosition = new Vector3(WIDTH / 2 - m_label.width / 2, 10);
        }

        private void CreateComponents()
        {
            m_label = AddUIComponent<UILabel>();
            m_label.text = "Cargo options";
            m_label.relativePosition = new Vector3(WIDTH / 2 - m_label.width / 2, 10);

            // Drag handle
            UIDragHandle handle = AddUIComponent<UIDragHandle>();
            handle.target = this;
            handle.constrainToScreen = true;
            handle.width = WIDTH;
            handle.height = 40;
            handle.relativePosition = Vector3.zero;

            m_flagsPanel = CreateFlagCheckboxes(new Vector3(10, handle.height + 10), 250, HEIGHT - 100, m_boxFlagDict, m_flagBoxDict);

            // Buttons
            m_confirmButton = UIUtils.CreateButton(this);
            m_confirmButton.text = "Done";
            m_confirmButton.relativePosition = new Vector3(WIDTH / 2 - m_confirmButton.width - 10, HEIGHT - m_confirmButton.height - 10);
            m_confirmButton.eventClicked += (c, p) =>
            {
                Done();
            };

            UIButton cancelButton = UIUtils.CreateButton(this);
            cancelButton.text = "Cancel";
            cancelButton.relativePosition = new Vector3(WIDTH / 2 + 10, HEIGHT - cancelButton.height - 10);
            cancelButton.eventClicked += (c, p) =>
            {
                isVisible = false;
            };
        }

        private void Done()
        {
            if(m_callback1 != null)
            {
                CargoFlags flags = 0;
                foreach(var v in m_boxFlagDict)
                {
                    if(v.Key.isChecked)
                    {
                        flags |= v.Value;
                        //Util.Log(v.Value.ToString() + " - " + v.Key.isChecked.ToString());
                    }
                }
                m_callback1.Invoke(flags);
            }
            isVisible = false;
        }

        

        private UIPanel CreateFlagCheckboxes(Vector3 startLocation, float halfWidth, float maxHeight, Dictionary<UICheckBox, CargoFlags> checkboxDict, Dictionary<CargoFlags, UICheckBox> flagDict)
        {
            UIPanel panel = AddUIComponent<UIPanel>();
            panel.width = halfWidth;
            panel.relativePosition = startLocation;

            float y = 0;
            float x = 0;

            var flags = CargoParcel.ResourceTypes;
            for(int i = 0; i < flags.Length; i++)
            {
                UICheckBox checkbox = UIUtils.CreateCheckBox(panel);
                checkbox.text = flags[i].ToString();
                checkbox.isChecked = false;
                checkbox.width = halfWidth - 10;
                checkbox.relativePosition = new Vector3(x, y);
                y += checkbox.height + 5;
 
                if(y >= maxHeight + checkbox.height)
                {
                    y = 0;
                    x += halfWidth;
                    panel.width += halfWidth;
                }

                flagDict.Add(flags[i], checkbox);
                checkboxDict.Add(checkbox, flags[i]);
            }

            panel.height = y;
            return panel;
        }
    }
}
