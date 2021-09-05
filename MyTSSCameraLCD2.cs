using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.InteropServices;
using avaness.CameraLCDRevived.Wrappers;
using Sandbox.Game.Components;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Blocks;
using Sandbox.Game.GameSystems;
using Sandbox.Game.GameSystems.TextSurfaceScripts;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using VRage.Game;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI;
using VRage.Game.Utils;
using VRage.ModAPI;
using VRage.Render.Image;
using VRage.Render.Scene;
using VRage.Utils;
using VRageMath;
using VRageRender;
using VRageRender.Messages;

namespace avaness.CameraLCDRevived
{
    [MyTextSurfaceScript("TSS_CameraDisplay", "Camera Display Test")]
    public class MyTSSCameraLCD2 : MyTSSCommon
    {
        public MyCameraBlock Camera { get; private set; }

        public override ScriptUpdate NeedsUpdate => ScriptUpdate.Update10; // frequency that Run() is called.

        private readonly ConcurrentQueue<byte[]> queue = new ConcurrentQueue<byte[]>();
        private readonly MyTextPanelComponent panelComponent;
        private readonly MyRenderComponentScreenAreas renderComp;
        private readonly IMyTerminalBlock terminalBlock;
        private bool needsTerminal;
        private bool taken;
        private DisplayId id;

        public MyTSSCameraLCD2(IMyTextSurface surface, IMyCubeBlock block, Vector2 size) : base(surface, block, size)
        {
            panelComponent = (MyTextPanelComponent)surface;
            terminalBlock = (IMyTerminalBlock)block;
            terminalBlock.OnMarkForClose += BlockMarkedForClose;
            terminalBlock.CustomDataChanged += TerminalBlock_CustomDataChanged;
            TerminalBlock_CustomDataChanged(terminalBlock);
            id = new DisplayId(terminalBlock.EntityId, panelComponent.Area);

            MyAPIGateway.Utilities.ShowNotification($"Surface: {panelComponent.Area} {block.GetType().Name} Size: {panelComponent.TextureSize}", 10000);
        }

        public override void Dispose()
        {
            base.Dispose(); // do not remove
            terminalBlock.OnMarkForClose -= BlockMarkedForClose;
            terminalBlock.CustomDataChanged -= TerminalBlock_CustomDataChanged;
            CameraLCD.RemoveDisplay(id);
        }

        void BlockMarkedForClose(IMyEntity ent)
        {
            Dispose();
        }

        private void TerminalBlock_CustomDataChanged(IMyTerminalBlock block)
        {
            string text = block.CustomData ?? "";
            MyGridTerminalSystem terminal = (MyGridTerminalSystem)MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(block.CubeGrid);
            needsTerminal = terminal == null;
            if (needsTerminal)
            {
                CameraLCD.RemoveDisplay(id);
                Camera = null;
                return;
            }
            foreach(var b in terminal.Blocks)
            {
                if (b is MyCameraBlock cameraBlock && b.CustomName.ToString() == text)
                {
                    Camera = cameraBlock;
                    CameraLCD.AddDisplay(id, this);
                    break;
                }
            }
            taken = false;
        }

        public override void Run()
        {
            try
            {
                base.Run(); // do not remove
                if (needsTerminal)
                    TerminalBlock_CustomDataChanged(terminalBlock);
                Draw();
            }
            catch (Exception e) // no reason to crash the entire game just for an LCD script, but do NOT ignore them either, nag user so they report it :}
            {
                DrawError(e);
            }
        }

        void Draw()
        {
            /*
            Vector2 screenSize = Surface.SurfaceSize;
            Vector2 screenCorner = (Surface.TextureSize - screenSize) * 0.5f;

            var frame = Surface.DrawFrame();

            // Drawing sprites works exactly like in PB API.
            // Therefore this guide applies: https://github.com/malware-dev/MDK-SE/wiki/Text-Panels-and-Drawing-Sprites

            // there are also some helper methods from the MyTSSCommon that this extends.
            // like: AddBackground(frame, Surface.ScriptBackgroundColor); - a grid-textured background

            // the colors in the terminal are Surface.ScriptBackgroundColor and Surface.ScriptForegroundColor, the other ones without Script in name are for text/image mode.
            string camera = "null";
            if (Camera != null)
                camera = Camera.GetType().ToString();
            var text = MySprite.CreateText(terminalBlock.CustomData + " " + needsTerminal + " " + camera, "Monospace", Surface.ScriptForegroundColor, 0.3f, TextAlignment.LEFT);
            text.Position = screenCorner + new Vector2(16, 16); // 16px from topleft corner of the visible surface
            frame.Add(text);

            // add more sprites and stuff

            frame.Dispose(); // send sprites to the screen
            */
        }

        void DrawError(Exception e)
        {
            MyLog.Default.WriteLineAndConsole($"{e.Message}\n{e.StackTrace}");

            try // first try printing the error on the LCD
            {
                Vector2 screenSize = Surface.SurfaceSize;
                Vector2 screenCorner = (Surface.TextureSize - screenSize) * 0.5f;

                var frame = Surface.DrawFrame();

                var bg = new MySprite(SpriteType.TEXTURE, "SquareSimple", null, null, Color.Black);
                frame.Add(bg);

                var text = MySprite.CreateText($"ERROR: {e.Message}\n{e.StackTrace}\n\nPlease send screenshot of this to mod author.\n{MyAPIGateway.Utilities.GamePaths.ModScopeName}", "White", Color.Red, 0.7f, TextAlignment.LEFT);
                text.Position = screenCorner + new Vector2(16, 16);
                frame.Add(text);

                frame.Dispose();
            }
            catch (Exception e2)
            {
                MyLog.Default.WriteLineAndConsole($"Also failed to draw error on screen: {e2.Message}\n{e2.StackTrace}");

                if (MyAPIGateway.Session?.Player != null)
                    MyAPIGateway.Utilities.ShowNotification($"[ ERROR: {GetType().FullName}: {e.Message} | Send SpaceEngineers.Log to mod author ]", 10000, MyFontEnum.Red);
            }
        }

        public void OnDrawScene()
        {
            if (!TryGetTextureName(out string textureName) && Camera != null)
                return;

            MyCamera camera = MySector.MainCamera;
            if (camera == null)
                return;

            MatrixD viewMatrix = GetViewMatrix();
            Matrix projMatrix = new Matrix(camera.ProjectionMatrix)
            {
                Forward = Vector3.Zero
            };

            SetCameraViewMatrix(viewMatrix, projMatrix, camera.ProjectionMatrixFar, camera.FovWithZoom, camera.FovWithZoom, camera.NearPlaneDistance, camera.FarPlaneDistance, camera.FarFarPlaneDistance, Camera.WorldMatrix.Translation, smooth: false);
            BorrowedRtvTexture texture = MyManagers.RwTexturesPool.BorrowRtv("CameraLCDRevivedRenderer", (int)panelComponent.TextureSize.X, (int)panelComponent.TextureSize.Y, Format.R8G8B8A8_UNorm_SRgb);
            MyRender11.DrawGameScene(texture, out _);
            DrawCharacterHead();


            //MyTextureData.ToFile(texture, @"C:\Users\austi\Desktop\test.png", MyImage.FileFormat.Png);
            byte[] data = texture.GetData();//MyTextureData.ToData(texture, null, MyImage.FileFormat.Bmp);
            int requiredSize = panelComponent.TextureByteCount;
            if(data.Length < requiredSize)
                Array.Resize(ref data, requiredSize);
            Draw(textureName, data);

            texture.Release();

            SetCameraViewMatrix(camera.ViewMatrix, camera.ProjectionMatrix, camera.ProjectionMatrixFar, camera.FovWithZoom, camera.FovWithZoom, camera.NearPlaneDistance, camera.FarPlaneDistance, camera.FarFarPlaneDistance, camera.Position, lastMomentUpdateIndex: 0, smooth: false);
            
        }

        private void DrawCharacterHead()
        {
            if (CameraLCD.Settings.HeadFix && MySession.Static != null && MySession.Static.LocalCharacter != null && MySession.Static.LocalCharacter.Render != null && MySession.Static.LocalCharacter.IsInFirstPersonView)
            {
                uint myChar = MySession.Static.LocalCharacter.Render.RenderObjectIDs[0];
                if (myChar > 0)
                {
                    foreach (string mat in MySession.Static.LocalCharacter.Definition.MaterialsDisabledIn1st)
                    {
                        MyScene11.AddMaterialRenderFlagChange(myChar, new MyEntityMaterialKey(mat),
                            new RenderFlagsChange() { Add = RenderFlags.Visible, Remove = 0 });
                    }
                }

            }
        }

        private MatrixD GetViewMatrix()
        {

            return Camera.GetViewMatrix();
            /*
            var position = Camera.PositionComp;
            MatrixD orientation = position.GetOrientation();
            Vector3D pos = position.GetPosition();
            Vector3D up = orientation.Up;
            Vector3D target = pos + orientation.Forward;
            return MatrixD.CreateLookAt(pos, target, up);
            */
        }

        private void SetCameraViewMatrix(MatrixD viewMatrix, Matrix projectionMatrix, Matrix projectionFarMatrix, float fov, float fovSkybox, float nearPlane, float farPlane, float farFarPlane, Vector3D cameraPosition, float projectionOffsetX = 0f, float projectionOffsetY = 0f, int lastMomentUpdateIndex = 1, bool smooth = true)
        {
            MyRenderMessageSetCameraViewMatrix renderMessage = MyRenderProxy.MessagePool.Get<MyRenderMessageSetCameraViewMatrix>(MyRenderMessageEnum.SetCameraViewMatrix);
            renderMessage.ViewMatrix = viewMatrix;
            renderMessage.ProjectionMatrix = projectionMatrix;
            renderMessage.ProjectionFarMatrix = projectionFarMatrix;
            renderMessage.FOV = fov;
            renderMessage.FOVForSkybox = fovSkybox;
            renderMessage.NearPlane = nearPlane;
            renderMessage.FarPlane = farPlane;
            renderMessage.FarFarPlane = farFarPlane;
            renderMessage.CameraPosition = cameraPosition;
            renderMessage.LastMomentUpdateIndex = lastMomentUpdateIndex;
            renderMessage.ProjectionOffsetX = projectionOffsetX;
            renderMessage.ProjectionOffsetY = projectionOffsetY;
            renderMessage.Smooth = smooth;
            MyRender11.SetupCameraMatrices(renderMessage);
        }

        private bool TryGetTextureName(out string name)
        {
            try
            {
                name = panelComponent.GetRenderTextureName();
                return true;
            }
            catch (NullReferenceException)
            {
                name = null;
                return false;
            }
        }

        public void Draw(string textureName, byte[] videoData)
        {

            MyManagers.FileTextures.ResetGeneratedTexture(textureName, videoData);//GetBGRValues(videoData));
            //MyRenderProxy.ResetGeneratedTexture(textureName, videoData);

            //Marshal.Copy(e.BufferHandle, videoData, 0, e.Width * e.Height * 4);
            /*if (videoData.Length < 1024 * 1024 * 4)
                Array.Resize(ref videoData, 1024 * 1024 * 4);*/


            //e.Handled = true;
            //_browser.GetBrowserHost().Invalidate(PaintElementType.View);
        }

        private byte[] GetBGRValues(byte[] data)
        {
            var bmp = new System.Drawing.Bitmap(new MemoryStream(data));
            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height);
            bmp = bmp.Clone(rect, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            System.Drawing.Imaging.BitmapData bmpData = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);
            bmpData.Stride = -bmpData.Stride;

            int rowBytes = bmpData.Width * System.Drawing.Image.GetPixelFormatSize(bmp.PixelFormat) / 8;
            int imgBytes = bmp.Height * rowBytes;

            IntPtr ptr = bmpData.Scan0;
            Marshal.Copy(ptr, data, 0, imgBytes);
            bmp.UnlockBits(bmpData);
            return data;
        }
    }
}