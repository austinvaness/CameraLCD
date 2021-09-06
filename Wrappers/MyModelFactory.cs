using HarmonyLib;
using System;
using System.Reflection;

namespace avaness.CameraLCD.Wrappers
{
    public class MyModelFactory
    {
        private readonly object instance;

        static MyModelFactory()
        {
            Type t = AccessTools.TypeByName("VRage.Render11.GeometryStage2.Model.MyModelFactory");
            onLoddingSettingChanged = t.GetMethod("OnLoddingSettingChanged", BindingFlags.Public | BindingFlags.Instance);
        }

        public MyModelFactory(object instance)
        {
            this.instance = instance;
        }

        private static readonly MethodInfo onLoddingSettingChanged;
        public void OnLoddingSettingChanged()
        {
            onLoddingSettingChanged.Invoke(instance, new object[0]);
        }
    }
}