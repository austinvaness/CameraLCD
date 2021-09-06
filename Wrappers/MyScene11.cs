using HarmonyLib;
using Sandbox.Engine.Utils;
using System;
using System.Reflection;
using VRage.Render.Scene;
using VRageRender;

namespace avaness.CameraLCD.Wrappers
{
    public class MyScene11
    {
        static MyScene11()
        {
            Type t = AccessTools.TypeByName("VRage.Render11.Scene.MyScene11");
            addMaterialRenderFlagChange = ReflectionHelper.CreateStaticDelegate<uint, MyEntityMaterialKey, RenderFlagsChange>(t, "AddMaterialRenderFlagChange");
        }

        private static readonly Action<uint, MyEntityMaterialKey, RenderFlagsChange> addMaterialRenderFlagChange;
        public static void AddMaterialRenderFlagChange(uint ID, MyEntityMaterialKey materialKey, RenderFlagsChange value)
        {
            addMaterialRenderFlagChange(ID, materialKey, value);
        }

    }
}
