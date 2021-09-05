using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace avaness.CameraLCDRevived.Wrappers
{
    public static class MyManagers
    {
        static MyManagers()
        {
            Type t = AccessTools.TypeByName("VRage.Render11.Common.MyManagers");
            geometryRenderer = t.GetField("GeometryRenderer", BindingFlags.Public | BindingFlags.Static);
            rwTexturesPool = t.GetField("RwTexturesPool", BindingFlags.Public | BindingFlags.Static);
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
    }
}
