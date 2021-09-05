using HarmonyLib;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.IO;
using System.Reflection;
using VRage.Render.Image;

namespace avaness.CameraLCD.Wrappers
{
    public class MyTextureData
    {
        static MyTextureData()
        {
            Type t = AccessTools.TypeByName("VRageRender.MyTextureData");
            toData = t.GetMethod("ToData", BindingFlags.NonPublic | BindingFlags.Static);
            toFile = t.GetMethod("ToFile", BindingFlags.NonPublic | BindingFlags.Static);
        }

        private static readonly MethodInfo toData;
        public static byte[] ToData(BorrowedRtvTexture res, byte[] screenData, MyImage.FileFormat fmt)
        {
            return (byte[])toData.Invoke(null, new object[] { res.Instance, screenData, fmt });
        }

        private static readonly MethodInfo toFile;
        public static bool ToFile(BorrowedRtvTexture res, string path, MyImage.FileFormat fmt)
        {
            return (bool)toFile.Invoke(null, new object[] { res.Instance, path, fmt });
        }
    }
}
