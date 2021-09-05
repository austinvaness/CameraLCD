using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VRageRender;
using VRageRender.Messages;

namespace avaness.CameraLCDRevived.Wrappers
{
    public static class MyRender11
    {

        static MyRender11()
        {
            Type t = AccessTools.TypeByName("VRageRender.MyRender11");
            m_debugOverrides = t.GetField("m_debugOverrides", BindingFlags.NonPublic | BindingFlags.Static);
            m_rc = t.GetField("m_rc", BindingFlags.NonPublic | BindingFlags.Static);
            settings = t.GetField("Settings", BindingFlags.NonPublic | BindingFlags.Static);
            postprocess = t.GetField("Postprocess", BindingFlags.NonPublic | BindingFlags.Static);

            setupCameraMatrices = t.GetMethod("SetupCameraMatrices", BindingFlags.Public | BindingFlags.Static);
            drawGameScene = t.GetMethod("DrawGameScene", BindingFlags.NonPublic | BindingFlags.Static);
            processMessageQueue = AccessTools.Method(t, "ProcessMessageQueue", new Type[0]);
        }

        private static readonly FieldInfo m_debugOverrides;
        public static MyRenderDebugOverrides DebugOverrides => (MyRenderDebugOverrides)m_debugOverrides.GetValue(null);


        private static readonly FieldInfo m_rc;
        public static object RenderContext => m_rc.GetValue(null);


        private static readonly FieldInfo settings;
        public static MyRenderSettings Settings
        {
            get
            {
                return (MyRenderSettings)settings.GetValue(null);
            }
            set
            {
                settings.SetValue(null, value);
            }
        }


        private static readonly FieldInfo postprocess;
        public static MyPostprocessSettings Postprocess
        {
            get
            {
                return (MyPostprocessSettings)postprocess.GetValue(null);
            }
            set
            {
                postprocess.SetValue(null, value);
            }
        }


        private static readonly MethodInfo setupCameraMatrices;
        public static void SetupCameraMatrices(MyRenderMessageSetCameraViewMatrix message)
        {
            setupCameraMatrices.Invoke(null, new object[] { message });
        }


        private static readonly MethodInfo drawGameScene;
        public static void DrawGameScene(BorrowedRtvTexture renderTarget, out object debugAmbientOcclusion)
        {
            object[] args = new object[] { renderTarget.Instance, null };
            drawGameScene.Invoke(null, args);
            debugAmbientOcclusion = args[1];
        }

        private static readonly MethodInfo processMessageQueue;
        public static void ProcessMessageQueue()
        {
            processMessageQueue.Invoke(null, new object[0]);
        }
    }
}
