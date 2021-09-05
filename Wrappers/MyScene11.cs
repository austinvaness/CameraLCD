using HarmonyLib;
using System;
using System.Reflection;
using VRage.Render.Scene;
using VRageRender;

namespace avaness.CameraLCDRevived.Wrappers
{
    public class MyScene11
    {
        static MyScene11()
        {
            Type t = AccessTools.TypeByName("VRage.Render11.Scene.MyScene11");
            addMaterialRenderFlagChange = AccessTools.Method(t, "AddMaterialRenderFlagChange");
        }

        private static readonly MethodInfo addMaterialRenderFlagChange;
        public static void AddMaterialRenderFlagChange(uint ID, MyEntityMaterialKey materialKey, RenderFlagsChange value)
        {
            addMaterialRenderFlagChange.Invoke(null, new object[] { ID, materialKey, value });
        }

    }
}
