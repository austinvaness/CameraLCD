using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace avaness.CameraLCDRevived.Wrappers
{
    public class MyGeneratedTextureManager
    {
        static MyGeneratedTextureManager()
        {
            Type t = AccessTools.TypeByName("VRage.Render11.Resources.MyGeneratedTextureManager");
            Type mgt = t.Assembly.GetType("VRage.Render11.Resources.Internal.MyGeneratedTexture");
            reset = AccessTools.Method(t, "Reset", new Type[] { mgt, typeof(byte[]), typeof(int) });
        }

        private static readonly MethodInfo reset; 
        public static void Reset(object mgt, byte[] data, int nchannels)
        {
            reset.Invoke(null, new object[] { mgt, data, nchannels });
        }
    }
}
