using HarmonyLib;
using Sandbox.Game.Entities;
using Sandbox.Game.Gui;
using Sandbox.Game.Localization;
using System;
using System.Reflection;
using System.Text;
using VRage.Utils;
using VRageMath;

namespace avaness.CameraLCD.Patch
{
    [HarmonyPatch(typeof(MyCameraBlock), "CreateTerminalControls")]
    public static class Patch_MyCameraBlock
    {
        private static readonly FieldInfo fov;
        static Patch_MyCameraBlock()
        {
            Type t = typeof(MyCameraBlock);
            fov = t.GetField("m_fov", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public static void Prefix(ref bool __state)
        {
            __state = MyTerminalControlFactory.AreControlsCreated<MyCameraBlock>();
        }

        public static void Postfix(bool __state)
        {
            if (!__state)
            {
                MyTerminalControlSlider<MyCameraBlock> slider = new MyTerminalControlSlider<MyCameraBlock>("Zoom", MySpaceTexts.ControlDescCameraZoom, MySpaceTexts.Blank)
                {
                    Enabled = x => x.IsWorking,
                    Setter = SetCameraFov,
                    Getter = GetCameraFov,
                    Writer = WriteCameraFov,
                };
                slider.SetLimits(GetZoomMin, GetZoomMax);
                slider.EnableActions(MathHelper.ToRadians(10), slider.Enabled, slider.Enabled);
                MyTerminalControlFactory.AddControl(slider);
            }

        }

        private static void WriteCameraFov(MyCameraBlock block, StringBuilder writeTo)
        {
            writeTo.Append(MathHelper.ToDegrees(GetCameraFov(block)));
        }

        private static float GetCameraFov(MyCameraBlock block)
        {
            return (float)fov.GetValue(block);
        }

        private static void SetCameraFov(MyCameraBlock block, float value)
        {
            fov.SetValue(block, value);
        }

        private static float GetZoomMin(MyCameraBlock block)
        {
            return block.BlockDefinition.MinFov;
        }

        private static float GetZoomMax(MyCameraBlock block)
        {
            return block.BlockDefinition.MaxFov;
        }
    }
}
