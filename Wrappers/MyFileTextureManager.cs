using HarmonyLib;
using System;
using System.Collections;
using System.Reflection;
using VRage.Utils;

namespace avaness.CameraLCD.Wrappers
{
    public class MyFileTextureManager
    {
        private readonly object instance;

        static MyFileTextureManager()
        {
            Type t = AccessTools.TypeByName("VRage.Render11.Resources.MyFileTextureManager");
            resetGeneratedTexture = t.GetMethod("ResetGeneratedTexture", BindingFlags.Public | BindingFlags.Instance);
        }

        public MyFileTextureManager(object instance)
        {
            this.instance = instance;
        }

        private static readonly MethodInfo resetGeneratedTexture;
        public void ResetGeneratedTexture(string name, byte[] data)
        {
            resetGeneratedTexture.Invoke(instance, new object[] { name, data });
        }

    }
}