using System;
using avaness.CameraLCD.Wrappers;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Blocks;
using Sandbox.Game.Entities.Character;
using Sandbox.Game.GameSystems;
using Sandbox.Game.GameSystems.TextSurfaceScripts;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using SharpDX.DXGI;
using VRage.Game.ModAPI;
using VRage.Game.Utils;
using VRage.ModAPI;
using VRage.Render.Scene;
using VRageMath;
using VRageRender;
using VRageRender.Messages;

namespace avaness.CameraLCD
{
    [MyTextSurfaceScript("TSS_CameraDisplay", "Camera Display")]
    public class CameraTSS : MyTSSCommon
    {
        public MyCameraBlock Camera { get; private set; }

        public override ScriptUpdate NeedsUpdate => ScriptUpdate.Update100;

        private readonly MyTextPanelComponent panelComponent;
        private readonly IMyTerminalBlock terminalBlock;
        private bool cameraExists;
        private DisplayId id;
        private byte[] buffer = new byte[0];

        public CameraTSS(IMyTextSurface surface, IMyCubeBlock block, Vector2 size) : base(surface, block, size)
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
            if (Camera != null)
                Camera.OnClose -= Camera_OnClose;

            // TODO: Fix freeze on session unload
        }

        void BlockMarkedForClose(IMyEntity ent)
        {
            Dispose();
        }

        private void TerminalBlock_CustomDataChanged(IMyTerminalBlock block)
        {
            string text = block.CustomData ?? "";
            MyGridTerminalSystem terminal = (MyGridTerminalSystem)MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(block.CubeGrid);
            if (terminal == null)
            {
                Camera = null;
                CameraLCD.RemoveDisplay(id);
                cameraExists = false;
                return;
            }

            foreach(var b in terminal.Blocks)
            {
                if (b is MyCameraBlock cameraBlock && b.CustomName.ToString() == text)
                {
                    Camera = cameraBlock;
                    Camera.OnClose += Camera_OnClose;
                    CameraLCD.AddDisplay(id, this);
                    cameraExists = true;
                    return;
                }
            }

            Camera = null;
            CameraLCD.RemoveDisplay(id);
            cameraExists = false;
        }

        private void Camera_OnClose(VRage.Game.Entity.MyEntity e)
        {
            e.OnClose -= Camera_OnClose;
            Camera = null;
            CameraLCD.RemoveDisplay(id);
            cameraExists = false;
        }

        public override void Run()
        {
            base.Run(); // do not remove
            if (!cameraExists)
                TerminalBlock_CustomDataChanged(terminalBlock);
        }

        /// <summary>
        /// This is the only method where calls to render can be made directly.
        /// </summary>
        public void OnDrawScene()
        {
            if (!TryGetTextureName(out string screenName) || Camera == null)
                return;

            MyCamera renderCamera = MySector.MainCamera;
            if (renderCamera == null)
                return;

            if (renderCamera.GetDistanceFromPoint(Camera.WorldMatrix.Translation) > CameraLCD.Settings.Range)
                return;

            MatrixD viewMatrix = Camera.GetViewMatrix();

            bool initialLods = true;
            if (!CameraLCD.Settings.UpdateLOD)
                initialLods = SetLoddingEnabled(false);

            SetCameraViewMatrix(viewMatrix, renderCamera.ProjectionMatrix, renderCamera.ProjectionMatrixFar, renderCamera.FovWithZoom, renderCamera.FovWithZoom, renderCamera.NearPlaneDistance, renderCamera.FarPlaneDistance, renderCamera.FarFarPlaneDistance, Camera.WorldMatrix.Translation, smooth: false);

            BorrowedRtvTexture texture = DrawGame();
            DrawOnScreen(screenName, texture);
            texture.Release();

            SetCameraViewMatrix(renderCamera.ViewMatrix, renderCamera.ProjectionMatrix, renderCamera.ProjectionMatrixFar, renderCamera.FovWithZoom, renderCamera.FovWithZoom, renderCamera.NearPlaneDistance, renderCamera.FarPlaneDistance, renderCamera.FarFarPlaneDistance, renderCamera.Position, lastMomentUpdateIndex: 0, smooth: false);

            if (!CameraLCD.Settings.UpdateLOD)
                SetLoddingEnabled(initialLods);
        }

        private BorrowedRtvTexture DrawGame()
        {
            BorrowedRtvTexture texture = MyManagers.RwTexturesPool.BorrowRtv("CameraLCDRevivedRenderer", (int)panelComponent.TextureSize.X, (int)panelComponent.TextureSize.Y, Format.R8G8B8A8_UNorm_SRgb);
            DrawCharacterHead();
            MyRender11.DrawGameScene(texture, out _);
            return texture;
        }

        private void DrawCharacterHead()
        {
            // TODO: Fix
            if (CameraLCD.Settings.HeadFix && MySession.Static != null)
            {
                MyCharacter cha = MySession.Static.LocalCharacter;
                if (cha != null && cha.Render != null && cha.IsInFirstPersonView)
                {
                    // Reference: MyCharacter.UpdateHeadModelProperties(bool enabled)
                    uint id = cha.Render.RenderObjectIDs[0];
                    if (id != uint.MaxValue)
                    {
                        foreach (string mat in cha.Definition.MaterialsDisabledIn1st)
                        {
                            //MyRenderProxy.UpdateModelProperties(id, mat, RenderFlags.Visible, 0, null, null);
                            
                            MyScene11.AddMaterialRenderFlagChange(id, new MyEntityMaterialKey(mat),
                                new RenderFlagsChange() { Add = RenderFlags.Visible, Remove = 0 });
                        }

                        //MyIDTracker<MyActor>.FindByID(id)?.UpdateBeforeDraw();
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
            texture.GetData(buffer);
            MyManagers.FileTextures.ResetGeneratedTexture(textureName, buffer);
        }
    }
}