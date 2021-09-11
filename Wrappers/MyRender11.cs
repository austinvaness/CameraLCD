using HarmonyLib;
using SharpDX.Direct3D11;
using System;
using System.Reflection;
using VRageMath;
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
            m_resolution = t.GetField("m_resolution", BindingFlags.NonPublic | BindingFlags.Static);

            setupCameraMatrices = ReflectionHelper.CreateStaticDelegate<MyRenderMessageSetCameraViewMatrix>(t, "SetupCameraMatrices");
            drawGameScene = t.GetMethod("DrawGameScene", BindingFlags.NonPublic | BindingFlags.Static);
            processMessageQueue = ReflectionHelper.CreateStaticDelegate(t, "ProcessMessageQueue");
            processMessageInternal = ReflectionHelper.CreateStaticDelegate<MyRenderMessageBase, int>(t, "ProcessMessageInternal");

            get_deviceInstance = ReflectionHelper.CreateStaticPropDelegate<Device1>(t, "DeviceInstance");
            get_resolutionI = ReflectionHelper.CreateStaticPropDelegate<Vector2I>(t, "ResolutionI");

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


        private static readonly Action<MyRenderMessageSetCameraViewMatrix> setupCameraMatrices;
        public static void SetupCameraMatrices(MyRenderMessageSetCameraViewMatrix message)
        {
            setupCameraMatrices(message);
        }


        private static readonly MethodInfo drawGameScene;
        public static void DrawGameScene(BorrowedRtvTexture renderTarget, out object debugAmbientOcclusion)
        {
            object[] args = new object[] { renderTarget.Instance, null };
            drawGameScene.Invoke(null, args);
            debugAmbientOcclusion = args[1];
        }

        private static readonly Action processMessageQueue;
        public static void ProcessMessageQueue()
        {
            processMessageQueue();
        }

        private static readonly Func<Device1> get_deviceInstance;
        public static Device1 DeviceInstance
        {
            get
            {
                return get_deviceInstance();
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

        private static readonly Func<Vector2I> get_resolutionI;
        private static readonly FieldInfo m_resolution;
        public static Vector2I ResolutionI
        {
            get
            {
                return get_resolutionI();
            }
            set
            {
                m_resolution.SetValue(null, value);
            }
        }

        private static readonly Action<MyRenderMessageBase, int> processMessageInternal;
        public static void ProcessMessage(MyRenderMessageBase message)
        {
            processMessageInternal(message, 0);
        }
    }

    public static class MyImmediateRC
    {
        public static MyRenderContext RC => MyRender11.RC;
    }
}
