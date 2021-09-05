using avaness.CameraLCDRevived.Wrappers;
using HarmonyLib;
using System;
using System.Reflection;
using VRage.Render11.Resources;

namespace avaness.CameraLCDRevived.Patch
{
    public static class Patch_ResetUserTexture
    {
        /* Fixes a crash due to missing format */
        public static bool Prefix(object texture, byte[] data)
        {
            MyGeneratedTextureManager.Reset(texture, data, 4);
            return false;
        }
    }

}