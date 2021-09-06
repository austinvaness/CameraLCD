using HarmonyLib;
using SharpDX.Direct3D11;
using System;
using System.Reflection;
using VRageRender;
using VRageRender.Messages;

namespace avaness.CameraLCD.Wrappers
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
            swapChain = t.GetField("m_swapchain", BindingFlags.NonPublic | BindingFlags.Static);

            setupCameraMatrices = t.GetMethod("SetupCameraMatrices", BindingFlags.Public | BindingFlags.Static);
            drawGameScene = t.GetMethod("DrawGameScene", BindingFlags.NonPublic | BindingFlags.Static);
            processMessageQueue = AccessTools.Method(t, "ProcessMessageQueue", new Type[0]);

            get_deviceInstance = t.GetProperty("DeviceInstance", BindingFlags.NonPublic | BindingFlags.Static).GetGetMethod(true);
        }

        private static readonly FieldInfo m_debugOverrides;
        public static MyRenderDebugOverrides DebugOverrides => (MyRenderDebugOverrides)m_debugOverrides.GetValue(null);


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

        private static readonly FieldInfo swapChain;
        public static SharpDX.DXGI.SwapChain SwapChain
        {
            get
            {
                return (SharpDX.DXGI.SwapChain)swapChain.GetValue(null);
            }
        }

        private static readonly MethodInfo get_deviceInstance;
        public static Device1 DeviceInstance
        {
            get
            {
                return (Device1)get_deviceInstance.Invoke(null, new object[0]);
            }
        }

        private static readonly FieldInfo m_rc;
        public static MyRenderContext RC
        {
            get
            {
                return new MyRenderContext(m_rc.GetValue(null));
            }
        }
    }

    public static class MyImmediateRC
    {
        public static MyRenderContext RC => MyRender11.RC;
    }

}
