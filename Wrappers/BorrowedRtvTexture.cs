using HarmonyLib;
using System;
using System.Reflection;

namespace avaness.CameraLCDRevived.Wrappers
{
    public class BorrowedRtvTexture
    {
        public object Instance { get; }

        static BorrowedRtvTexture()
        {
            Type t = AccessTools.TypeByName("VRage.Render11.Resources.Textures.MyBorrowedTexture");
            release = t.GetMethod("Release", BindingFlags.Public | BindingFlags.Instance);
        }

        public BorrowedRtvTexture(object instance)
        {
            Instance = instance;
        }

        private static readonly MethodInfo release;
        public void Release()
        {
            release.Invoke(Instance, new object[0]);
        }
    }
}