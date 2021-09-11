using HarmonyLib;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.IO;
using System.Reflection;
using VRageMath;

namespace avaness.CameraLCD.Wrappers
{
    public class BorrowedRtvTexture
    {
        public object Instance { get; }

        static BorrowedRtvTexture()
        {
            Type t = AccessTools.TypeByName("VRage.Render11.Resources.Textures.MyBorrowedTexture");
            release = t.GetMethod("Release", BindingFlags.Public | BindingFlags.Instance);
            t = AccessTools.TypeByName("VRage.Render11.Resources.Textures.MyBorrowedRtvTexture");
            get_Resource = t.GetProperty("Resource", BindingFlags.Public | BindingFlags.Instance).GetGetMethod();
        }

        public BorrowedRtvTexture(object instance)
        {
            Instance = instance;
        }

        private static readonly MethodInfo release;
        public void Release()
        {
            release.Invoke(Instance, new object[0]);
        }

        public void GetData(byte[] buffer, int bufferOffset, int bufferLength)
        {
            // Reference: MyTextureData.ToData(IResource res, byte[] screenData, MyImage.FileFormat fmt)
            using (MemoryStream memoryStream = new MemoryStream(buffer, bufferOffset, bufferLength, true))
            {
                Save(Instance, memoryStream);
            }
        }

        private static readonly MethodInfo get_Resource;
        private void Save(object res, Stream stream)
        {
            // Reference: MyTextureData.Save(IResource res, Stream stream, MyImage.FileFormat fileFormat)
            Texture2D texture2D = (Texture2D)get_Resource.Invoke(Instance, new object[0]);
            Format format = texture2D.Description.Format;
            Texture2D texture2D2 = new Texture2D(MyRender11.DeviceInstance, new Texture2DDescription
            {
                Width = texture2D.Description.Width,
                Height = texture2D.Description.Height,
                MipLevels = 1,
                ArraySize = 1,
                Format = format,
                Usage = ResourceUsage.Staging,
                SampleDescription = new SampleDescription(1, 0),
                BindFlags = BindFlags.None,
                CpuAccessFlags = CpuAccessFlags.Read,
                OptionFlags = ResourceOptionFlags.None
            });
            MyImmediateRC.RC.CopyResource(res, texture2D2);
            MyImmediateRC.RC.MapSubresource(texture2D2, 0, 0, MapMode.Read, SharpDX.Direct3D11.MapFlags.None, out DataStream stream2);
            stream2.CopyTo(stream);
            MyImmediateRC.RC.UnmapSubresource(texture2D2, 0);
            texture2D2.Dispose();
        }

    }
}