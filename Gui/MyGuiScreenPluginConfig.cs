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
        MyGuiControlCombobox myGuiControlCombobox;
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
            Controls.Add(myGuiControlSeparatorList);
            MyGuiControlSeparatorList myGuiControlSeparatorList2 = new MyGuiControlSeparatorList();
            myGuiControlSeparatorList2.AddHorizontal(-new Vector2(m_size.Value.X * 0.78f / 2f, -m_size.Value.Y / 2f + 0.023f), m_size.Value.X * 0.79f, 0f, null);
            Controls.Add(myGuiControlSeparatorList2);

            m_okBtn = new MyGuiControlButton(new Vector2?(new Vector2(0, 0.40f)), MyGuiControlButtonStyleEnum.Default, null, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_BOTTOM, null, MyTexts.Get(MyCommonTexts.Close), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(OnCloseButtonClick), GuiSounds.MouseClick, 1f, null, false);
            m_okBtn.Enabled = true;
            m_okBtn.ButtonClicked += OnOkButtonClick;
            /*var m_cancelBtn = new MyGuiControlButton(new Vector2?(new Vector2(0.1f, 0.338f)), MyGuiControlButtonStyleEnum.Default, null, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_BOTTOM, null, MyTexts.Get(MyCommonTexts.Cancel), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnCloseButtonClick), GuiSounds.MouseClick, 1f, null, false, null);
            m_cancelBtn.Enabled = true;
            m_cancelBtn.ButtonClicked += OnCloseButtonClick;
            
            this.Controls.Add(m_cancelBtn);*/
            Controls.Add(m_okBtn);
            CloseButtonEnabled = true;
            float y = -0.25f;

            Controls.Add(new MyGuiControlLabel(new Vector2(-0.16f, y - 0.05f), null, "Enabled"));
            MyGuiControlCheckbox myGuiControlCheckbox2 = new MyGuiControlCheckbox(new Vector2(0f, y - 0.05f), null, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            myGuiControlCheckbox2.IsChecked = CameraLCD.Settings.Enabled;
            myGuiControlCheckbox2.IsCheckedChanged += new Action<MyGuiControlCheckbox>(IsEnabledCheckedChanged);
            myGuiControlCheckbox2.Enabled = true;
            myGuiControlCheckbox2.Visible = true;
            Controls.Add(myGuiControlCheckbox2);

            Controls.Add(new MyGuiControlLabel(new Vector2(-0.16f, y + 0f), null, "Steal frames"));
            myGuiControlCheckbox2 = new MyGuiControlCheckbox(new Vector2(0f, y + 0f), null, "Use frame stealing method instead of additional rendering. Also enable SpriteFix. Warning: May leak overlays into OBS", false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            myGuiControlCheckbox2.IsChecked = CameraLCD.Settings.steal;
            myGuiControlCheckbox2.IsCheckedChanged += new Action<MyGuiControlCheckbox>(IsStealCheckedChanged);
            myGuiControlCheckbox2.Enabled = true;
            myGuiControlCheckbox2.Visible = true;
            Controls.Add(myGuiControlCheckbox2);

            Controls.Add(new MyGuiControlLabel(new Vector2(-0.16f, y + 0.05f), null, "Render ratio"));
            myGuiControlCombobox = new MyGuiControlCombobox(new Vector2(-0.02f, y + 0.025f), null, null, null, 10, null, false, "Base camera update rate relative to main FPS. 1x disables steal", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER)
            {
                VisualStyle = MyGuiControlComboboxStyleEnum.Debug,
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP
            };

            Controls.Add(new MyGuiControlLabel(new Vector2(-0.16f, y + 0.1f), null, "Sprite Fix"));
            MyGuiControlCheckbox myGuiControlCheckbox3 = new MyGuiControlCheckbox(new Vector2(0f, y + 0.1f), null, "Fixes sprite flickering in steal mode", false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            myGuiControlCheckbox3.IsChecked = CameraLCD.Settings.SpriteFix;
            myGuiControlCheckbox3.IsCheckedChanged += new Action<MyGuiControlCheckbox>(IsSpriteFixCheckedChanged);
            myGuiControlCheckbox3.Enabled = true;
            myGuiControlCheckbox3.Visible = true;
            Controls.Add(myGuiControlCheckbox3);

            Controls.Add(new MyGuiControlLabel(new Vector2(-0.16f, y + 0.15f), null, "Shadows"));
            myGuiControlCheckbox3 = new MyGuiControlCheckbox(new Vector2(0f, y + 0.15f), null, "Renders shadows in cameras", false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            myGuiControlCheckbox3.IsChecked = CameraLCD.Settings.Shadows;
            myGuiControlCheckbox3.IsCheckedChanged += new Action<MyGuiControlCheckbox>(IsShadowsCheckedChanged);
            myGuiControlCheckbox3.Enabled = true;
            myGuiControlCheckbox3.Visible = true;
            Controls.Add(myGuiControlCheckbox3);

            Controls.Add(new MyGuiControlLabel(new Vector2(-0.16f, y + 0.2f), null, "Bloom"));
            myGuiControlCheckbox3 = new MyGuiControlCheckbox(new Vector2(0f, y + 0.2f), null, "Renders Bloom in cameras", false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            myGuiControlCheckbox3.IsChecked = CameraLCD.Settings.Bloom;
            myGuiControlCheckbox3.IsCheckedChanged += new Action<MyGuiControlCheckbox>(IsBloomCheckedChanged);
            myGuiControlCheckbox3.Enabled = true;
            myGuiControlCheckbox3.Visible = true;
            Controls.Add(myGuiControlCheckbox3);

            Controls.Add(new MyGuiControlLabel(new Vector2(-0.16f, y + 0.25f), null, "Billboards"));
            myGuiControlCheckbox3 = new MyGuiControlCheckbox(new Vector2(0f, y + 0.25f), null, "Renders Billboards in cameras", false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            myGuiControlCheckbox3.IsChecked = CameraLCD.Settings.BillboardsDynamic;
            myGuiControlCheckbox3.IsCheckedChanged += new Action<MyGuiControlCheckbox>(IsBillboardsCheckedChanged);
            myGuiControlCheckbox3.Enabled = true;
            myGuiControlCheckbox3.Visible = true;
            Controls.Add(myGuiControlCheckbox3);

            Controls.Add(new MyGuiControlLabel(new Vector2(-0.16f, y + 0.30f), null, "Flares"));
            myGuiControlCheckbox3 = new MyGuiControlCheckbox(new Vector2(0f, y + 0.30f), null, "Renders flares in cameras", false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            myGuiControlCheckbox3.IsChecked = CameraLCD.Settings.Flares;
            myGuiControlCheckbox3.IsCheckedChanged += new Action<MyGuiControlCheckbox>(IsFlaresCheckedChanged);
            myGuiControlCheckbox3.Enabled = true;
            myGuiControlCheckbox3.Visible = true;
            Controls.Add(myGuiControlCheckbox3);

            Controls.Add(new MyGuiControlLabel(new Vector2(-0.16f, y + 0.35f), null, "Fxaa"));
            myGuiControlCheckbox3 = new MyGuiControlCheckbox(new Vector2(0f, y + 0.35f), null, "Renders fxaa in cameras", false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            myGuiControlCheckbox3.IsChecked = CameraLCD.Settings.Fxaa;
            myGuiControlCheckbox3.IsCheckedChanged += new Action<MyGuiControlCheckbox>(IsFxaaCheckedChanged);
            myGuiControlCheckbox3.Enabled = true;
            myGuiControlCheckbox3.Visible = true;
            Controls.Add(myGuiControlCheckbox3);

            Controls.Add(new MyGuiControlLabel(new Vector2(-0.16f, y + 0.40f), null, "Head fix"));
            myGuiControlCheckbox3 = new MyGuiControlCheckbox(new Vector2(0f, y + 0.40f), null, "Fix to render your own head in camera view", false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            myGuiControlCheckbox3.IsChecked = CameraLCD.Settings.HeadFix;
            myGuiControlCheckbox3.IsCheckedChanged += new Action<MyGuiControlCheckbox>(IsHeadfixCheckedChanged);
            myGuiControlCheckbox3.Enabled = true;
            myGuiControlCheckbox3.Visible = true;
            Controls.Add(myGuiControlCheckbox3);

            Controls.Add(new MyGuiControlLabel(new Vector2(-0.16f, y + 0.45f), null, "Update LOD"));
            myGuiControlCheckbox3 = new MyGuiControlCheckbox(new Vector2(0f, y + 0.45f), null, "LOD update between frames", false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            myGuiControlCheckbox3.IsChecked = CameraLCD.Settings.UpdateLOD;
            myGuiControlCheckbox3.IsCheckedChanged += new Action<MyGuiControlCheckbox>(IsLODCheckedChanged);
            myGuiControlCheckbox3.Enabled = true;
            myGuiControlCheckbox3.Visible = true;
            Controls.Add(myGuiControlCheckbox3);

            Controls.Add(new MyGuiControlLabel(new Vector2(-0.16f, y + 0.50f), null, "Show debug stats"));
            myGuiControlCheckbox3 = new MyGuiControlCheckbox(new Vector2(0f, y + 0.50f), null, "debug stats", false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            myGuiControlCheckbox3.IsChecked = CameraLCD.Settings.ShowDebug;
            myGuiControlCheckbox3.IsCheckedChanged += new Action<MyGuiControlCheckbox>(IsShowDebugCheckedChanged);
            myGuiControlCheckbox3.Enabled = true;
            myGuiControlCheckbox3.Visible = true;
            Controls.Add(myGuiControlCheckbox3);

            Controls.Add(new MyGuiControlLabel(new Vector2(-0.16f, y + 0.55f), null, "Render range"));
            MyGuiControlSlider myGuiControlSlider = new MyGuiControlSlider(new Vector2?(new Vector2(0f, y + 0.55f)), 0f, 120f, 0.18f, new float?(0f), null, string.Empty, 0, 0.8f, 0f, "White", null, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, true, false);
            //new MyGuiControlCheckbox(new Vector2(0f, y + 0.45f), null, "How far away before LCDs will start rendering the camera", false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);

            myGuiControlSlider.ValueChanged += new Action<MyGuiControlSlider>(RangeCheckedChanged);
            myGuiControlSlider.Enabled = true;
            myGuiControlSlider.Visible = true;
            Controls.Add(myGuiControlSlider);

            Controls.Add(myGuiControlCombobox);
            myGuiControlCombobox.AddItem(1L, "1x", null, null);
            myGuiControlCombobox.AddItem(2L, "2x", null, null);
            myGuiControlCombobox.AddItem(3L, "3x", null, null);
            myGuiControlCombobox.AddItem(4L, "4x", null, null);
            myGuiControlCombobox.AddItem(5L, "5x", null, null);
            myGuiControlCombobox.AddItem(8L, "8x", null, null);
            myGuiControlCombobox.AddItem(10L, "10x", null, null);
            myGuiControlCombobox.AddItem(15L, "15x", null, null);
            myGuiControlCombobox.AddItem(30L, "30x", null, null);
            myGuiControlCombobox.SelectItemByKey(CameraLCD.Settings.ratio);
            myGuiControlCombobox.ItemSelected += OnModeComboSelect;
        }

        private void OnModeComboSelect()
        {
            CameraLCD.Settings.ratio = (int)myGuiControlCombobox.GetSelectedKey();
        }

        void IsEnabledCheckedChanged(MyGuiControlCheckbox cb)
        {
            CameraLCD.Settings.Enabled = cb.IsChecked;
        }

        void IsStealCheckedChanged(MyGuiControlCheckbox cb)
        {
            CameraLCD.Settings.steal = cb.IsChecked;
        }

        void IsSpriteFixCheckedChanged(MyGuiControlCheckbox cb)
        {
            CameraLCD.Settings.SpriteFix = cb.IsChecked;
        }

        void IsShadowsCheckedChanged(MyGuiControlCheckbox cb)
        {
            CameraLCD.Settings.Shadows = cb.IsChecked;
        }

        void IsBloomCheckedChanged(MyGuiControlCheckbox cb)
        {
            CameraLCD.Settings.Bloom = cb.IsChecked;
        }

        void IsBillboardsCheckedChanged(MyGuiControlCheckbox cb)
        {
            CameraLCD.Settings.BillboardsDynamic = cb.IsChecked;
            CameraLCD.Settings.BillboardsStatic = cb.IsChecked;
        }

        void IsFlaresCheckedChanged(MyGuiControlCheckbox cb)
        {
            CameraLCD.Settings.Flares = cb.IsChecked;
        }

        void IsFxaaCheckedChanged(MyGuiControlCheckbox cb)
        {
            CameraLCD.Settings.Fxaa = cb.IsChecked;
        }

        void IsHeadfixCheckedChanged(MyGuiControlCheckbox cb)
        {
            CameraLCD.Settings.HeadFix = cb.IsChecked;
        }

        void IsLODCheckedChanged(MyGuiControlCheckbox cb)
        {
            CameraLCD.Settings.UpdateLOD = cb.IsChecked;
        }

        void IsShowDebugCheckedChanged(MyGuiControlCheckbox cb)
        {
            CameraLCD.Settings.ShowDebug = cb.IsChecked;
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

        private MyGuiControlButton m_okBtn;
    }

}