using HarmonyLib;
using System;
using System.Reflection;

namespace avaness.CameraLCD.Wrappers
{
    public static class MyManagers
    {
        static MyManagers()
        {
            Type t = AccessTools.TypeByName("VRage.Render11.Common.MyManagers");
            geometryRenderer = t.GetField("GeometryRenderer", BindingFlags.Public | BindingFlags.Static);
            rwTexturesPool = t.GetField("RwTexturesPool", BindingFlags.Public | BindingFlags.Static);
            fileTextures = t.GetField("FileTextures", BindingFlags.Public | BindingFlags.Static);
        }

        private static readonly FieldInfo geometryRenderer;
        public static MyGeometryRenderer GeometryRenderer
        {
            get
            {
                return new MyGeometryRenderer(geometryRenderer.GetValue(null));
            }
        }

        private static readonly FieldInfo rwTexturesPool;
        public static MyBorrowedRwTextureManager RwTexturesPool
        {
            get
            {
                return new MyBorrowedRwTextureManager(rwTexturesPool.GetValue(null));
            }
        }

        private static readonly FieldInfo fileTextures;
        public static MyFileTextureManager FileTextures 
        { 
            get
            {
                return new MyFileTextureManager(fileTextures.GetValue(null));
            }
        }
    }
}
