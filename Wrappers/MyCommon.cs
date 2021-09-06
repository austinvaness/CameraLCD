using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRageRender;

namespace avaness.CameraLCD.Wrappers
{
    public static class MyCommon
    {
        static MyCommon()
        {
            Type t = AccessTools.TypeByName("VRageRender.MyCommon");
            ReflectionHelper.CreateStaticPropDelegate(t, "LoddingSettings", out get_loddingSettings, out set_loddingSettings);
        }

        private static readonly Func<MyNewLoddingSettings> get_loddingSettings;
        private static readonly Action<MyNewLoddingSettings> set_loddingSettings;
        public static MyNewLoddingSettings LoddingSettings
        {
            get
            {
                return get_loddingSettings();
            }
            set
            {
                set_loddingSettings(value);
            }
        }
    }
}
