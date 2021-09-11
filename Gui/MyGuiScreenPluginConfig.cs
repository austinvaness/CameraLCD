using Sandbox;
using System;
using VRage;
using VRage.Audio;
using VRage.Game;
using VRage.Utils;
using VRageMath;
using Sandbox.Graphics.GUI;

namespace avaness.CameraLCD.Gui
{
    public class MyGuiScreenPluginConfig : MyGuiScreenBase
    {
        private MyGuiControlCombobox ratioCombobox;
        private MyGuiControlButton okBtn;

        public MyGuiScreenPluginConfig() : base(new Vector2?(new Vector2(0.5f, 0.5f)), new Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), new Vector2?(new Vector2(0.7264286f, 0.8633588f)), false, null, MySandboxGame.Config.UIBkOpacity, MySandboxGame.Config.UIOpacity, null)
        {
            EnabledBackgroundFade = true;
            m_closeOnEsc = true;
            m_drawEvenWithoutFocus = true;
            CanHideOthers = true;
            CanBeHidden = true;
        }

        public override void LoadContent()
        {
            base.LoadContent();
            RecreateControls(true);
        }

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            BuildControls();
        }

        public override string GetFriendlyName()
        {
            return "MyGuiScreenModConfig";
        }

        protected void BuildControls()
        {
            AddCaption("CameraLCD Settings", null, new Vector2?(new Vector2(0f, 0.003f)), 0.8f);
            MyGuiControlSeparatorList myGuiControlSeparatorList = new MyGuiControlSeparatorList();
            myGuiControlSeparatorList.AddHorizontal(-new Vector2(m_size.Value.X * 0.78f / 2f, m_size.Value.Y / 2f - 0.075f), m_size.Value.X * 0.79f, 0f, null);
            myGuiControlSeparatorList.AddHorizontal(-new Vector2(m_size.Value.X * 0.78f / 2f, -m_size.Value.Y / 2f + 0.023f), m_size.Value.X * 0.79f, 0f, null);
            Controls.Add(myGuiControlSeparatorList);

            okBtn = new MyGuiControlButton(new Vector2?(new Vector2(0, 0.40f)), MyGuiControlButtonStyleEnum.Default, null, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_BOTTOM, null, MyTexts.Get(MyCommonTexts.Close), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(OnCloseButtonClick), GuiSounds.MouseClick, 1f, null, false)
            {
                Enabled = true
            };
            okBtn.ButtonClicked += OnOkButtonClick;
            Controls.Add(okBtn);
            CloseButtonEnabled = true;
            float y = -0.25f;

            Controls.Add(new MyGuiControlLabel(new Vector2(-0.16f, y - 0.05f), null, "Enabled"));
            MyGuiControlCheckbox enabledCheckbox = new MyGuiControlCheckbox(new Vector2(0f, y - 0.05f), null, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER)
            {
                IsChecked = CameraLCD.Settings.Enabled,
                Enabled = true,
                Visible = true
            };
            enabledCheckbox.IsCheckedChanged += IsEnabledCheckedChanged;
            Controls.Add(enabledCheckbox);

            Controls.Add(new MyGuiControlLabel(new Vector2(-0.16f, y + 0.05f), null, "Render ratio"));
            ratioCombobox = new MyGuiControlCombobox(new Vector2(-0.02f, y + 0.025f), null, null, null, 10, null, false, "Base camera update rate relative to main FPS. 1x disables steal", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER)
            {
                VisualStyle = MyGuiControlComboboxStyleEnum.Debug,
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP
            };
            ratioCombobox.AddItem(2L, "2x", null, null);
            ratioCombobox.AddItem(3L, "3x", null, null);
            ratioCombobox.AddItem(4L, "4x", null, null);
            ratioCombobox.AddItem(5L, "5x", null, null);
            ratioCombobox.AddItem(8L, "8x", null, null);
            ratioCombobox.AddItem(10L, "10x", null, null);
            ratioCombobox.AddItem(15L, "15x", null, null);
            ratioCombobox.AddItem(30L, "30x", null, null);
            ratioCombobox.SelectItemByKey(CameraLCD.Settings.Ratio);
            ratioCombobox.ItemSelected += OnModeComboSelect;
            Controls.Add(ratioCombobox);

            Controls.Add(new MyGuiControlLabel(new Vector2(-0.16f, y + 0.40f), null, "Head fix"));
            MyGuiControlCheckbox headFixCheckbox = new MyGuiControlCheckbox(new Vector2(0f, y + 0.40f), toolTip: "Fix to render your own head in camera view")
            {
                IsChecked = CameraLCD.Settings.HeadFix,
                Enabled = true,
                Visible = true
            };
            headFixCheckbox.IsCheckedChanged += IsHeadfixCheckedChanged;
            Controls.Add(headFixCheckbox);

            Controls.Add(new MyGuiControlLabel(new Vector2(-0.16f, y + 0.45f), null, "Update LOD"));
            MyGuiControlCheckbox lodCheckbox = new MyGuiControlCheckbox(new Vector2(0f, y + 0.45f), toolTip: "LOD update between frames")
            {
                IsChecked = CameraLCD.Settings.UpdateLOD,
                Enabled = true,
                Visible = true
            };
            lodCheckbox.IsCheckedChanged += IsLODCheckedChanged;
            Controls.Add(lodCheckbox);

            Controls.Add(new MyGuiControlLabel(new Vector2(-0.16f, y + 0.55f), null, "Render range"));
            MyGuiControlSlider rangeSlider = new MyGuiControlSlider(new Vector2(0f, y + 0.55f), 0, 120, 0.18f, CameraLCD.Settings.Range, null, string.Empty, 0, 0.8f, 0, "White", null, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, true)
            {
                Enabled = true,
                Visible = true
            };
            rangeSlider.ValueChanged += RangeCheckedChanged;
            Controls.Add(rangeSlider);

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

        void RangeCheckedChanged(MyGuiControlSlider cb)
        {
            CameraLCD.Settings.Range = (int)cb.Value;
        }

        private void OnCloseButtonClick(object sender)
        {
            CloseScreen();
        }

        private void OnOkButtonClick(object sender)
        {
            CloseScreen();
        }

    }

}