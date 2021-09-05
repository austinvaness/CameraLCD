using HarmonyLib;
using System;
using System.Reflection;
using VRage.Render11.Scene.Components;

namespace avaness.CameraLCDRevived.Patch
{
    public static class Patch_UpdateProxiesObjectData
    {
        public static void Patch(Harmony harmony)
        {
            Type t = AccessTools.TypeByName("VRage.Render11.Scene.Components.MyRenderableComponent");
            harmony.Patch(
                t.GetMethod("RebuildRenderProxies", BindingFlags.NonPublic | BindingFlags.Instance), 
                new HarmonyMethod(typeof(Patch_UpdateProxiesObjectData), nameof(Prefix)));
        }

        public static bool Prefix(object __instance)
        {
            if (CameraLCD.IsRenderFrame() && CameraLCD.HasNextDisplay())
                return false;
            return true;
        }
    }

}