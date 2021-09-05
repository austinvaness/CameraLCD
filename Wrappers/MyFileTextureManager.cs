using HarmonyLib;
using System;
using System.Collections;
using System.Reflection;
using VRage.Utils;

namespace avaness.CameraLCD.Wrappers
{
    public class MyFileTextureManager
    {
        private readonly object instance;

        static MyFileTextureManager()
        {
            Type t = AccessTools.TypeByName("VRage.Render11.Resources.MyFileTextureManager");
            m_generatedTextures = t.GetField("m_generatedTextures", BindingFlags.NonPublic | BindingFlags.Instance);
            tIUserGneratedTexture = AccessTools.TypeByName("VRage.Render11.Resources.IUserGeneratedTexture");
            resetGeneratedTexture = tIUserGneratedTexture.GetMethod("Reset", BindingFlags.Public | BindingFlags.Instance);
            Type tITexture = AccessTools.TypeByName("VRage.Render11.Resources.ITexture");
            //Type tIResource = AccessTools.TypeByName("VRage.Render11.Resources.IResource");
            textureSize = tITexture.GetProperty("Format", BindingFlags.Public | BindingFlags.Instance);
        }

        public MyFileTextureManager(object instance)
        {
            this.instance = instance;
        }

        private static readonly FieldInfo m_generatedTextures;
        private static readonly Type tIUserGneratedTexture;
        private static readonly MethodInfo resetGeneratedTexture;
        private static readonly PropertyInfo textureSize;
        public void ResetGeneratedTexture(string name, byte[] data)
        {
            IDictionary textures = (IDictionary)m_generatedTextures.GetValue(instance);
            if(textures.Contains(name))
            {
                object userGeneratedTexture = textures[name];
                if(userGeneratedTexture != null && tIUserGneratedTexture.IsInstanceOfType(userGeneratedTexture))
                {
                    resetGeneratedTexture.Invoke(userGeneratedTexture, new object[] { data });
                    //MyLog.Default.WriteLine(textureSize.GetGetMethod().Invoke(userGeneratedTexture, new object[0]).ToString());
                }
            }

            //resetGeneratedTexture.Invoke(instance, new object[] { name, data });
        }

    }
}