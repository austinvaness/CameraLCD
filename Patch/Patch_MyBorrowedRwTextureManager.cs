using HarmonyLib;
using System;
using System.Reflection;

namespace avaness.CameraLCDRevived.Patch
{
    public static class Patch_MyBorrowedRwTextureManager
    {
        public static void Patch(Harmony harmony)
        {
            Type t = AccessTools.TypeByName("VRage.Render11.Resources.MyBorrowedRwTextureManager");
            harmony.Patch(
                t.GetMethod("CreateRtv", BindingFlags.NonPublic | BindingFlags.Instance),
                new HarmonyMethod(AccessTools.Method(typeof(Patch_MyBorrowedRwTextureManager), nameof(Prefix))));
        }

        public static bool Prefix(object __instance)
        {
            //CameraLCD.log.Log("creatertv " + __instance);
            CameraLCD.MyBorrowedRwTextureManager = __instance;
            return true;
        }
    }
}
