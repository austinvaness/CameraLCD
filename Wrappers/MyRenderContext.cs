using SharpDX.Mathematics.Interop;
using System;
using System.Reflection;

namespace avaness.CameraLCDRevived.Wrappers
{
    /*public class MyRenderContext
    {
        object instance;
        private static bool init;
        private static MethodInfo mResetTargets;
        private static MethodInfo mClearRtv;
        private static MethodInfo mClearState;

        public MyRenderContext(object instance)
        {
            this.instance = instance;
            if(!init)
            {
                Type t = instance.GetType();
                mResetTargets = t.GetMethod("ResetTargets", BindingFlags.NonPublic | BindingFlags.Instance);
                mClearRtv = t.GetMethod("ClearRtv", BindingFlags.NonPublic | BindingFlags.Instance);
                mClearState = t.GetMethod("ClearState", BindingFlags.NonPublic | BindingFlags.Instance);
                init = true;
            }
        }

        public void ClearState()
        {
            mClearState.Invoke(instance, new object[0]);
        }

        public void ResetTargets()
        {
            mResetTargets.Invoke(instance, new object[0]);
        }

        public void ClearRtv(object rtv, RawColor4 colorRGBA)
        {
            mClearRtv.Invoke(instance, new object[] { rtv, colorRGBA });
        }
    }*/
}
