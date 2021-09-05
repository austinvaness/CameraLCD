using Sandbox.Engine.Utils;
using System;
using VRage.Game.Entity;
using VRageMath;
using VRageRender;

namespace avaness.CameraLCDRevived
{
    public class Display
    {
        public byte[] videoData = new byte[1024 * 1024 * 4];
        public string textureName = null;
        public DisplayId id;
        public int width;
        public int height;
        public int tick = 0;
        public int fov = 0;
        public int range = 0;
        public float brightness = 0;
        public float contrast = 0;
        public int resMult = 1;
        public MyEntity source;
        public MySpectatorCameraController cam;
        public MyEntity entity;

        public Display(DisplayId _id, string _textureName, int width = 512, int height = 512)
        {
            textureName = _textureName;
            //videoData = new byte[1024 * 1024 * 4];
            this.width = width;
            this.height = height;
            this.id = _id;
        }

        public void SetSource(MyEntity e)
        {
            source = e;
            if (source != null)
            {
                source.OnClosing += (MyEntity) =>
                {
                    source = null;
                };
            }
        }

        public void SetFov(int newFov)
        {
            fov = newFov;
        }

        public void SetRange(int newRange)
        {
            range = newRange;
        }

        public void OnPaint(object sender, byte[] data)
        {
            if (this.textureName != null)
            {
                //Marshal.Copy(data, videoData, 0, data.Length);
                if (videoData.Length < 1024 * 1024 * 4)
                {
                    Array.Resize(ref videoData, 1024 * 1024 * 4);
                }
                MyRenderProxy.ResetGeneratedTexture(textureName, videoData);

            }
        }
    }

}