using HarmonyLib;
using System;
using System.Reflection;
using VRageRender;

namespace avaness.CameraLCD.Wrappers
{
    public class MyGeometryRenderer
    {
        private readonly object instance;

        static MyGeometryRenderer()
        {
            Type t = AccessTools.TypeByName("VRage.Render11.GeometryStage2.Rendering.MyGeometryRenderer");
            isLodUpdateEnabled = t.GetField("IsLodUpdateEnabled", BindingFlags.Public | BindingFlags.Instance);
            m_globalLoddingSettings = t.GetField("m_globalLoddingSettings", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public MyGeometryRenderer(object instance)
        {
            this.instance = instance;
        }

        private static readonly FieldInfo isLodUpdateEnabled;
        public bool IsLodUpdateEnabled
        {
            get
            {
                return (bool)isLodUpdateEnabled.GetValue(instance);
            }
            set
            {
                isLodUpdateEnabled.SetValue(instance, value);
            }
        }

        private static readonly FieldInfo m_globalLoddingSettings;
        public MyGlobalLoddingSettings Settings
        {
            get
            {
                return (MyGlobalLoddingSettings)m_globalLoddingSettings.GetValue(instance);
            }
            set
            {
                m_globalLoddingSettings.SetValue(instance, value);
            }
        }
    }
}