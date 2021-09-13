using Sandbox;
using System;
using VRage;
using VRage.Audio;
using VRage.Game;
using VRage.Utils;
using VRageMath;
using Sandbox.Graphics.GUI;
using Sandbox.ModAPI;

namespace avaness.CameraLCD.Gui
{
    public class MyGuiScreenPluginConfig : MyGuiScreenBase
    {
        private const float space = 0.01f;

        private MyGuiControlCombobox ratioCombobox;
        private MyGuiControlLabel rangeLabel;

        public MyGuiScreenPluginConfig() : base(new Vector2(0.5f, 0.5f), MyGuiConstants.SCREEN_BACKGROUND_COLOR, new Vector2(0.7264286f, 0.8633588f), false, null, MySandboxGame.Config.UIBkOpacity, MySandboxGame.Config.UIOpacity)
        {
            EnabledBackgroundFade = true;
            CloseButtonEnabled = true;
        }

        public override string GetFriendlyName()
        {
            return "MyGuiScreenModConfig";
        }

        public override void LoadContent()
        {
            base.LoadContent();
            RecreateControls(true);
        }

        public override void RecreateControls(bool constructor)
        {
            CameraLCDSettings settings = CameraLCD.Settings;

            MyGuiControlLabel caption = AddCaption("Camera LCD Settings");
            Vector2 pos = caption.Position;
            pos.Y += (caption.Size.Y / 2) + space;

            MyGuiControlSeparatorList sperators = new MyGuiControlSeparatorList();
            float sepWidth = Size.Value.X * 0.8f;
            sperators.AddHorizontal(pos - new Vector2(sepWidth / 2, 0), sepWidth);
            Controls.Add(sperators);
            pos.Y += space;

            MyGuiControlCheckbox enabledCheckbox = new MyGuiControlCheckbox(pos, isChecked: settings.Enabled, originAlign: MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
            enabledCheckbox.IsCheckedChanged += IsEnabledCheckedChanged;
            Controls.Add(enabledCheckbox);
            AddCaption(enabledCheckbox, "Enabled");
            pos.Y += enabledCheckbox.Size.Y + space;

            ratioCombobox = new MyGuiControlCombobox(pos, toolTip: "Base camera update rate relative to main FPS.", originAlign: MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP)
            {
                VisualStyle = MyGuiControlComboboxStyleEnum.Debug,
            };
            ratioCombobox.AddItem(2, "2x");
            ratioCombobox.AddItem(3, "3x");
            ratioCombobox.AddItem(4, "4x");
            ratioCombobox.AddItem(5, "5x");
            ratioCombobox.AddItem(8, "8x");
            ratioCombobox.AddItem(10, "10x");
            ratioCombobox.AddItem(15, "15x");
            ratioCombobox.AddItem(30, "30x");
            ratioCombobox.SelectItemByKey(settings.Ratio);
            ratioCombobox.ItemSelected += OnModeComboSelect;
            Controls.Add(ratioCombobox);
            AddCaption(ratioCombobox, "Render ratio");
            pos.Y += ratioCombobox.Size.Y + space;

            MyGuiControlSlider rangeSlider = new MyGuiControlSlider(pos, 10, 120, 0.18f, settings.Range, originAlign: MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, intValue: true);
            rangeSlider.ValueChanged += RangeValueChanged;
            Controls.Add(rangeSlider);
            AddCaption(rangeSlider, "Render range");
            rangeLabel = new MyGuiControlLabel(rangeSlider.Position + new Vector2(rangeSlider.Size.X + space, rangeSlider.Size.Y / 2), text: rangeSlider.Value.ToString(), originAlign: MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            Controls.Add(rangeLabel);
            pos.Y += rangeSlider.Size.Y + space;

            MyGuiControlCheckbox headFixCheckbox = new MyGuiControlCheckbox(pos, isChecked: settings.HeadFix, toolTip: "Fix to render your own head in camera view", originAlign: MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
            headFixCheckbox.IsCheckedChanged += IsHeadfixCheckedChanged;
            Controls.Add(headFixCheckbox);
            AddCaption(headFixCheckbox, "Head fix");
            pos.Y += headFixCheckbox.Size.Y + space;

            MyGuiControlCheckbox lodCheckbox = new MyGuiControlCheckbox(pos, isChecked: settings.UpdateLOD, toolTip: "LOD update between frames", originAlign: MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
            lodCheckbox.IsCheckedChanged += IsLODCheckedChanged;
            Controls.Add(lodCheckbox);
            AddCaption(lodCheckbox, "Update LOD");
            pos.Y += lodCheckbox.Size.Y + space;

            MyGuiControlCheckbox aspectCheckbox = new MyGuiControlCheckbox(pos, isChecked: settings.LockAspectRatio, toolTip: "Fix distortion by locking the aspect ratio", originAlign: MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
            aspectCheckbox.IsCheckedChanged += IsAspectCheckedChanged;
            Controls.Add(aspectCheckbox);
            AddCaption(aspectCheckbox, "Lock Aspect Ratio");
            pos.Y += aspectCheckbox.Size.Y + space;

            // Bottom
            pos = new Vector2(0, (m_size.Value.Y / 2) - space);
            MyGuiControlButton closeButton = new MyGuiControlButton(pos, text: MyTexts.Get(MyCommonTexts.Close), originAlign: MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_BOTTOM, onButtonClick: OnCloseClicked);
            Controls.Add(closeButton);
        }

        private void IsAspectCheckedChanged(MyGuiControlCheckbox cb)
        {
            CameraLCD.Settings.LockAspectRatio = cb.IsChecked;
        }

        private void OnCloseClicked(MyGuiControlButton btn)
        {
            CloseScreen();
        }

        protected override void OnClosed()
        {
            CameraLCD.Settings.Save();
        }

        private void AddCaption(MyGuiControlBase control, string caption)
        {
            Controls.Add(new MyGuiControlLabel(control.Position + new Vector2(-space, control.Size.Y / 2), text: caption, originAlign: MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER));
        }

        private void OnModeComboSelect()
        {
            CameraLCD.Settings.Ratio = (int)ratioCombobox.GetSelectedKey();
        }

        void IsEnabledCheckedChanged(MyGuiControlCheckbox cb)
        {
            CameraLCD.Settings.Enabled = cb.IsChecked;
        }

        void IsHeadfixCheckedChanged(MyGuiControlCheckbox cb)
        {
            CameraLCD.Settings.HeadFix = cb.IsChecked;
        }

        void IsLODCheckedChanged(MyGuiControlCheckbox cb)
        {
            CameraLCD.Settings.UpdateLOD = cb.IsChecked;
        }

        void RangeValueChanged(MyGuiControlSlider cb)
        {
            CameraLCD.Settings.Range = (int)cb.Value;
            rangeLabel.Text = cb.Value.ToString();
        }

    }

}