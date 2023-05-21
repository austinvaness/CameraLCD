using System;
using System.IO;
using System.Reflection;
using avaness.CameraLCD.Wrappers;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Blocks;
using Sandbox.Game.Entities.Character;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.GameSystems;
using Sandbox.Game.GameSystems.TextSurfaceScripts;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using SharpDX.DXGI;
using VRage.Game.ModAPI;
using VRage.Game.Utils;
using VRage.ModAPI;
using VRage.Render.Scene;
using VRage.Utils;
using VRageMath;
using VRageRender;
using VRageRender.Messages;

namespace avaness.CameraLCD
{
    [MyTextSurfaceScript("TSS_CameraDisplay", "Camera Display")]
    public class CameraTSS : MyTSSCommon
    {
        private static readonly FieldInfo m_fov;

        public override ScriptUpdate NeedsUpdate => ScriptUpdate.Update100;

        private readonly MyTextPanel panel;
        private readonly MyTextPanelComponent panelComponent;
        private readonly MyTerminalBlock terminalBlock;
        private DisplayId Id => new DisplayId(terminalBlock.EntityId, panelComponent.Area);

        private MyCameraBlock camera;
        private bool registered, functional;
        private byte[] buffer = new byte[0];
        private int bufferOffset = 0;

        static CameraTSS()
        {
            m_fov = typeof(MyCameraBlock).GetField("m_fov", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public CameraTSS(IMyTextSurface surface, IMyCubeBlock block, Vector2 size) : base(surface, block, size)
        {
            panel = block as MyTextPanel;
            panelComponent = (MyTextPanelComponent)surface;
            terminalBlock = (MyTerminalBlock)block;
            terminalBlock.OnMarkForClose += BlockMarkedForClose;
            terminalBlock.CustomDataChanged += CustomDataChanged;
            terminalBlock.IsWorkingChanged += IsWorkingChanged;
            CustomDataChanged(terminalBlock);
        }

        public override void Dispose()
        {
            base.Dispose(); // do not remove
            terminalBlock.OnMarkForClose -= BlockMarkedForClose;
            terminalBlock.CustomDataChanged -= CustomDataChanged;
            terminalBlock.IsWorkingChanged -= IsWorkingChanged;
            Unregister();
        }

        void BlockMarkedForClose(IMyEntity ent)
        {
            Dispose();
        }

        private void Register(MyCameraBlock cameraBlock)
        {
            camera = cameraBlock;
            camera.OnClose += Camera_OnClose;
            camera.IsWorkingChanged += IsWorkingChanged;
            CameraLCD.AddDisplay(Id, this);
            registered = true;
            IsWorkingChanged(camera);
        }

        private void IsWorkingChanged(MyCubeBlock b)
        {
            if (camera == null)
                functional = false;
            else
                functional = terminalBlock.IsWorking && camera.IsWorking;
        }

        private void Unregister()
        {
            if (!registered)
                return;

            if(camera != null)
            {
                camera.OnClose -= Camera_OnClose;
                camera.IsWorkingChanged -= IsWorkingChanged;
            }
            camera = null;
            CameraLCD.RemoveDisplay(Id);
            registered = false;
            functional = false;
            panelComponent.Reset();
        }

        private void CustomDataChanged(IMyTerminalBlock block)
        {
            if (string.IsNullOrWhiteSpace(block.CustomData))
            {
                Unregister();
                return;
            }

            string text = GetCameraName(block.CustomData);
            MyGridTerminalSystem terminal = (MyGridTerminalSystem)MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(block.CubeGrid);
            if (terminal == null)
            {
                Unregister();
                return;
            }

            foreach(var b in terminal.Blocks)
            {
                if (b is MyCameraBlock cameraBlock && b.CustomName.ToString() == text)
                {
                    Register(cameraBlock);
                    return;
                }
            }

            Unregister();
        }

        private string GetCameraName(string customData)
        {
            string prefix = panelComponent.Area + ":";
            using (StringReader reader = new StringReader(customData))
            {
                string line = reader.ReadLine();
                while(line != null)
                {
                    if (line.StartsWith(prefix) && line.Length > prefix.Length)
                        return line.Substring(prefix.Length);
                    line = reader.ReadLine();
                }
            }
            return customData;
        }

        private void Camera_OnClose(VRage.Game.Entity.MyEntity e)
        {
            Unregister();
        }

        public override void Run()
        {
            base.Run(); // do not remove
            if (!registered)
                CustomDataChanged(terminalBlock);
        }

        /// <summary>
        /// This is the only method where calls to render can be made directly.
        /// </summary>
        public bool OnDrawScene()
        {
            if (!TryGetTextureName(out string screenName) || camera == null || !functional)
                return false;

            if (panel != null)
            {
                // Textures for panel rotation dont get unregistered
                if (panel.PanelComponent != panelComponent)
                    return false;

                // Block 90 and 270 degree rotation
                // TODO: Figure out why they crash on wide lcd panel
                if (panelComponent.Area % 2 == 1)
                    return false;
            }


            MyCamera renderCamera = MySector.MainCamera;
            if (renderCamera == null)
                return false;

            if (renderCamera.GetDistanceFromPoint(terminalBlock.WorldMatrix.Translation) > CameraLCD.Settings.Range)
                return false;

            // Set temporary settings
            bool initialLods = true;
            if (!CameraLCD.Settings.UpdateLOD)
                initialLods = SetLoddingEnabled(false);
            MyRenderSettings renderSettings = MyRender11.Settings;
            MyRenderSettings oldRenderSettings = renderSettings;
            renderSettings.User.GrassDensityFactor = 0;
            renderSettings.User.GrassDrawDistance = 0;
            MyRender11.Settings = renderSettings;
            MyRenderDebugOverrides debugOverrides = MyRender11.DebugOverrides;
            debugOverrides.Flares = false;
            debugOverrides.Bloom = false;
            debugOverrides.SSAO = false;

            // Set camera position
            float fov = GetCameraFov();
            SetCameraViewMatrix(camera.GetViewMatrix(), renderCamera.ProjectionMatrix, renderCamera.ProjectionMatrixFar, fov, fov, renderCamera.NearPlaneDistance, renderCamera.FarPlaneDistance, renderCamera.FarFarPlaneDistance, camera.WorldMatrix.Translation, smooth: false);

            // Draw the game to the screen
            BorrowedRtvTexture texture = DrawGame();
            DrawOnScreen(screenName, texture);
            texture.Release();

            // Restore camera position
            SetCameraViewMatrix(renderCamera.ViewMatrix, renderCamera.ProjectionMatrix, renderCamera.ProjectionMatrixFar, renderCamera.FieldOfView, renderCamera.FieldOfView, renderCamera.NearPlaneDistance, renderCamera.FarPlaneDistance, renderCamera.FarFarPlaneDistance, renderCamera.Position, lastMomentUpdateIndex: 0, smooth: false);

            // Restore settings
            if (!CameraLCD.Settings.UpdateLOD)
                SetLoddingEnabled(initialLods);
            MyRender11.Settings = oldRenderSettings;
            debugOverrides.Flares = true;
            debugOverrides.Bloom = true;
            debugOverrides.SSAO = true;

            return true;
        }

        private float GetCameraFov()
        {
            return (float)m_fov.GetValue(camera);
        }

        private BorrowedRtvTexture DrawGame()
        {
            Vector2I sourceResolution = MyRender11.ResolutionI;
            Vector2I sourceViewportResolution = MyRender11.ViewportResolution;
            Vector2I targetResolution = (Vector2I)panelComponent.TextureSize;

            if (CameraLCD.Settings.LockAspectRatio)
            {
                Vector2 targetReal = panelComponent.SurfaceSize;
                float targetRatio = targetReal.X / targetReal.Y;
                // Ex 512/512 = 1

                float sourceRatio = (float)sourceResolution.X / sourceResolution.Y;
                // Ex 1920/1080 = 1.7777777

                if (targetRatio < sourceRatio)
                {
                    int newHeight = (int)((targetResolution.Y / sourceRatio) * targetRatio);
                    int barHeight = (targetResolution.Y - newHeight) / 2;
                    if (bufferOffset == 0)
                        buffer = new byte[0];
                    bufferOffset = barHeight * targetResolution.X * 4;
                    if (bufferOffset < 0)
                        bufferOffset = 0; // Theoretically this is impossible
                    else
                        targetResolution.Y = newHeight;
                }
                else
                {
                    bufferOffset = 0;
                }
            }
            else
            {
                bufferOffset = 0;
            }

            MyRender11.ResolutionI = targetResolution;

            BorrowedRtvTexture texture = MyManagers.RwTexturesPool.BorrowRtv("CameraLCDRevivedRenderer", targetResolution.X, targetResolution.Y, Format.R8G8B8A8_UNorm_SRgb);
            DrawCharacterHead();
            MyRender11.DrawGameScene(texture, out _);

            MyRender11.ViewportResolution = sourceViewportResolution;
            MyRender11.ResolutionI = sourceResolution;
            return texture;
        }

        private void DrawCharacterHead()
        {
            if (CameraLCD.Settings.HeadFix && MySession.Static != null)
            {
                MyCharacter cha = MySession.Static.LocalCharacter;
                if (cha != null && cha.Render != null && cha.IsInFirstPersonView)
                {
                    // Reference: MyCharacter.UpdateHeadModelProperties(bool enabled)
                    uint id = cha.Render.RenderObjectIDs[0];
                    if (id != uint.MaxValue)
                    {
                        foreach (string materialName in cha.Definition.MaterialsDisabledIn1st)
                        {
                            // Reference: MyRenderProxy.UpdateModelProperties(id, mat, RenderFlags.Visible, 0, null, null);

                            MyRenderMessageUpdateModelProperties test = MyRenderProxy.MessagePool.Get<MyRenderMessageUpdateModelProperties>(MyRenderMessageEnum.UpdateModelProperties);
                            test.ID = id;
                            test.MaterialName = MyStringId.GetOrCompute(materialName);
                            test.FlagsChange = new RenderFlagsChange()
                            {
                                Add = RenderFlags.Visible,
                                Remove = 0,
                            };
                            test.DiffuseColor = null;
                            test.Emissivity = null;
                            MyRender11.ProcessMessage(test);
                        }
                    }

                }
            }
        }

        private bool SetLoddingEnabled(bool enabled)
        {
            // Reference: MyRender11.ProcessMessageInternal(MyRenderMessageBase message, int frameId)
            //              case MyRenderMessageEnum.UpdateNewLoddingSettings

            MyNewLoddingSettings loddingSettings = MyCommon.LoddingSettings;
            MyGlobalLoddingSettings globalSettings = loddingSettings.Global;
            bool initial = globalSettings.IsUpdateEnabled;
            if (initial == enabled)
                return initial;

            globalSettings.IsUpdateEnabled = enabled;
            loddingSettings.Global = globalSettings;
            MyManagers.GeometryRenderer.IsLodUpdateEnabled = enabled;
            MyManagers.GeometryRenderer.Settings = globalSettings;
            MyManagers.ModelFactory.OnLoddingSettingChanged();
            return initial;
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

        private void DrawOnScreen(string textureName, BorrowedRtvTexture texture)
        {
            int requiredSize = panelComponent.TextureByteCount;
            if (buffer.Length < requiredSize)
                buffer = new byte[requiredSize];
            texture.GetData(buffer, bufferOffset, buffer.Length - (2 * bufferOffset));
            MyManagers.FileTextures.ResetGeneratedTexture(textureName, buffer);
        }
    }
}