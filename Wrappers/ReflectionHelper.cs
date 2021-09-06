using HarmonyLib;
using System;
using System.Reflection;

namespace avaness.CameraLCD.Wrappers
{
    public static class ReflectionHelper
    {
        public static void CreateStaticPropDelegate<T>(Type t, string name, out Func<T> getter, out Action<T> setter)
        {
            PropertyInfo prop = t.GetProperty(name, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, null, typeof(T), new Type[0], null);
            getter = (Func<T>)Delegate.CreateDelegate(typeof(Func<T>), prop.GetGetMethod(true));
            setter = (Action<T>)Delegate.CreateDelegate(typeof(Action<T>), prop.GetSetMethod(true));
        }

        public static Action CreateStaticDelegate(Type t, string name)
        {
            return (Action)Delegate.CreateDelegate(typeof(Action), t.GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, null, new Type[0], null));
        }

        public static Action<T> CreateStaticDelegate<T>(Type t, string name)
        {
            return (Action<T>)Delegate.CreateDelegate(typeof(Action<T>), t.GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, null, new Type[] { typeof(T) }, null));
        }

        public static Action<T1, T2> CreateStaticDelegate<T1, T2>(Type t, string name)
        {
            return (Action<T1, T2>)Delegate.CreateDelegate(typeof(Action<T1, T2>), t.GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, null, new Type[] { typeof(T1), typeof(T2) }, null));
        }

        public static Action<T1, T2, T3> CreateStaticDelegate<T1, T2, T3>(Type t, string name)
        {
            return (Action<T1, T2, T3>)Delegate.CreateDelegate(typeof(Action<T1, T2, T3>), t.GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, null, new Type[] { typeof(T1), typeof(T2), typeof(T3) }, null));
        }
    }
}
