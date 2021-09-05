using System;
using System.Reflection;

namespace avaness.CameraLCDRevived.Wrappers
{
    public class MyRenderScheduler
    {
        object instance;
        private static bool init;
        private static MethodInfo mInit;
        private static MethodInfo mExecute;
        private static MethodInfo mDone;

        public MyRenderScheduler(object instance)
        {
            this.instance = instance;
            if (!init)
            {
                Type t = instance.GetType();
                mInit = t.GetMethod("Init", BindingFlags.Public | BindingFlags.Instance);
                mExecute = t.GetMethod("Execute", BindingFlags.Public | BindingFlags.Instance);
                mDone = t.GetMethod("Done", BindingFlags.Public | BindingFlags.Instance);
                init = true;
            }
        }

        public void Init()
        {
            mInit.Invoke(instance, new object[0]);
        }

        public void Execute()
        {
            mExecute.Invoke(instance, new object[0]);
        }

        public void Done()
        {
            mInit.Invoke(instance, new object[0]);
        }
    }
}
