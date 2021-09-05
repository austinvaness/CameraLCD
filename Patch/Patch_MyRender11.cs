using HarmonyLib;
using System;
using System.Reflection;
using Sandbox.Game.Weapons;
using Sandbox.Game.World;
using Sandbox.Graphics.GUI;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using VRage.Game.Utils;
using VRage.Render.Scene;
using VRageMath;
using VRageRender;
using VRageRender.Messages;
using avaness.CameraLCD.Wrappers;
using VRage.Render.Image;

namespace avaness.CameraLCD.Patch
{
    public static class Patch_MyRender11
    {
        private static MyCamera camera => MySector.MainCamera;
        //private static MyRenderMessageSetCameraViewMatrix cachedMessage;

        public static void Patch(Harmony harmony)
        {
            Type t = AccessTools.TypeByName("VRageRender.MyRender11");

            harmony.Patch(
                t.GetMethod("Present", BindingFlags.NonPublic | BindingFlags.Static),
                new HarmonyMethod(typeof(Patch_MyRender11), nameof(Prefix_Present)));
            harmony.Patch(
                t.GetMethod("DrawScene", BindingFlags.Public | BindingFlags.Static),
                new HarmonyMethod(typeof(Patch_MyRender11), nameof(Prefix_DrawScene)));
            //harmony.Patch(
            //    t.GetMethod("SetupCameraMatricesInternal", BindingFlags.NonPublic | BindingFlags.Static),
            //    new HarmonyMethod(typeof(Patch_MyRender11), nameof(Prefix_SetupCameraMatricesInternal)));
            //harmony.Patch(
            //    t.GetMethod("ConsumeMainSprites", BindingFlags.NonPublic | BindingFlags.Static),
            //    new HarmonyMethod(typeof(Patch_MyRender11), nameof(Prefix_ConsumeMainSprites)));
            //harmony.Patch(
            //    AccessTools.Method(t, "RenderMainSprites", new Type[0]),
            //    new HarmonyMethod(typeof(Patch_MyRender11), nameof(Prefix_RenderMainSprites)));
        }

        /* Fixes the steam overlay bleeding into OBS etc when steal mode enabled */
        public static bool Prefix_Present()
        {
            if (CameraLCD.Settings.steal && CameraLCD.IsRenderFrame() && CameraLCD.HasNextDisplay())
            {
                //                CameraLCD.log.Log("Prevented present");
                return false;
            }
            return true;
        }

        public static MyRenderMessageSetCameraViewMatrix GetCameraViewMatrix(MatrixD viewMatrix, Matrix projectionMatrix, Matrix projectionFarMatrix, float fov, float fovSkybox, float nearPlane, float farPlane, float farFarPlane, Vector3D cameraPosition, float projectionOffsetX = 0f, float projectionOffsetY = 0f, int lastMomentUpdateIndex = 1)
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
            return renderMessage;
        }

        public static long renderCount = 0;
        public static Stopwatch sw = new Stopwatch();
        public static bool Prefix_DrawScene()
        {
            return !CameraLCD.OnDrawScene();

            /*
            CameraLCD.renderedFrame = false;
            if (!CameraLCD.IsRenderFrame())
            {
                return true;
            }

            MyRenderDebugOverrides debugOverrides = MyRender11.DebugOverrides;
            MyGeometryRenderer geoRenderer = MyManagers.GeometryRenderer;

            bool didUpdate = false;
            Display display = CameraLCD.GetNextDisplay();
            MyRenderSettings renderSettings = MyRender11.Settings;
            float oldGrass = renderSettings.User.GrassDensityFactor;
            float oldGrassDist = renderSettings.User.GrassDrawDistance;

            MyPostprocessSettings ppSettings = MyRender11.Postprocess;
            float bright = ppSettings.Data.Brightness;
            //var eyeAdatap = ppSettings.EnableEyeAdaptation;
            float contrast = ppSettings.Data.Contrast;

            if (display != null)
            {
                //var display = e.Value;
                if (display.source == null)
                {

                }
                else if (CameraLCD.MyBorrowedRwTextureManager != null && display.textureName != null)
                {
                    MyCamera mainCamera = MySector.MainCamera;

                    if (mainCamera != null && Vector3D.Distance((MatrixD.CreateTranslation(display.entity.PositionComp.LocalVolume.Center) * display.entity.WorldMatrix).Translation, mainCamera.Position) > (display.range > 0 ? display.range : CameraLCD.Settings.Range))
                    {
                        return !CameraLCD.Settings.steal;
                    }
                    sw.Restart();
                    if (!didUpdate)
                    {
                        geoRenderer.IsLodUpdateEnabled = CameraLCD.Settings.UpdateLOD;

                        debugOverrides.Shadows = CameraLCD.Settings.Shadows;
                        debugOverrides.BillboardsStatic = CameraLCD.Settings.BillboardsStatic;
                        debugOverrides.BillboardsDynamic = CameraLCD.Settings.BillboardsDynamic;
                        debugOverrides.Flares = CameraLCD.Settings.Flares;
                        debugOverrides.Fxaa = CameraLCD.Settings.Fxaa;
                        debugOverrides.Bloom = CameraLCD.Settings.Bloom;
                        CameraLCD.rendering = true;

                        if (CameraLCD.Settings.HeadFix && MySession.Static != null && MySession.Static.LocalCharacter != null && MySession.Static.LocalCharacter.Render != null && MySession.Static.LocalCharacter.IsInFirstPersonView)
                        {
                            uint myChar = MySession.Static.LocalCharacter.Render.RenderObjectIDs[0];
                            if (myChar > 0)
                            {
                                foreach (string mat in MySession.Static.LocalCharacter.Definition.MaterialsDisabledIn1st)
                                {
                                    MyScene11.AddMaterialRenderFlagChange( myChar, new MyEntityMaterialKey(mat), 
                                        new RenderFlagsChange() { Add = RenderFlags.Visible, Remove = 0 });
                                }
                            }

                        }

                        renderSettings.User.GrassDensityFactor = 0;
                        renderSettings.User.GrassDrawDistance = 0;
                        MyRender11.Settings = renderSettings;

                        ppSettings.Data.Brightness = display.brightness != 0 ? display.brightness : bright;
                        ppSettings.Data.Contrast = display.contrast != 0 ? display.contrast : contrast;
                        MyRender11.Postprocess = ppSettings;
                    }
                    var borrowedRtvTexture = MyManagers.RwTexturesPool.BorrowRtv(
                        "secef_Camerarender",
                        display.width,
                        display.height,
                        Format.B8G8R8A8_UNorm,
                        1,
                        0
                    );
                    CameraLCD.log.Log("borrowed " + borrowedRtvTexture);
                    Matrix projMatrix = new Matrix(camera.ProjectionMatrix)
                    {
                        Forward = Vector3.Zero
                    };
                    MatrixD viewMatrix = new MatrixD(camera.ViewMatrix);
                    viewMatrix.Rotation.SetDirectionVector(Base6Directions.Direction.Forward, Vector3.Zero);
                    Vector3D pos = new Vector3D(-30.0, 0.0, -21.24);
                    Vector3 up = new Vector3(0f, 1f, 0f);
                    Vector3D dest = new Vector3D(0.0, 0.0, -21.24);

                    if (display.source != null)
                    {
                        CameraLCD.log.Log("Got cam source");
                        pos = display.source.PositionComp.GetPosition();
                        up = display.source.PositionComp.GetOrientation().Up;
                        dest = pos + display.source.PositionComp.GetOrientation().Forward;
                    }

                    viewMatrix = MatrixD.CreateLookAt(pos, dest, up);

                    if (display.source is MyLargeTurretBase turretBase)
                    {
                        viewMatrix = turretBase.GetViewMatrix();
                    }
                    float oldFov = camera.Zoom.GetFOV();
                    float newFov = (float)Math.Max(0.01, Math.Min(display.fov > 0 ? Math.PI / 180 * display.fov : oldFov, Math.PI));

                    MyRenderMessageSetCameraViewMatrix vm = GetCameraViewMatrix(viewMatrix, projMatrix, camera.ProjectionMatrixFar, newFov, newFov, camera.NearPlaneDistance, camera.FarPlaneDistance, camera.FarFarPlaneDistance, pos, 0f, 0f, 1);
                    CameraLCD.log.Log(string.Concat(new object[]
                    {
                        "vm: ",
                        vm,
                        "camerapos: ",
                        MySector.MainCamera
                    }));

                    // Move player to camera location
                    MyRender11.SetupCameraMatrices(vm);

                    MyRender11.DrawGameScene(borrowedRtvTexture, out _);

                    byte[] data = MyTextureData.ToData(borrowedRtvTexture, null, MyImage.FileFormat.Bmp);

                    CameraLCD.log.Log("dat len: " + data.Length);
                    if (data.Length != 0)
                    {
                        using (MemoryStream ms = new MemoryStream(data))
                        {

                            Bitmap bmp = new Bitmap(new Bitmap(ms), new Size(display.width, display.height));
                            Patch_CreateTexture.GetBGRValues(bmp, ref display.videoData);
                            MyRenderProxy.ResetGeneratedTexture(display.textureName, display.videoData);
                        }
                    }

                    // previous cam
                    vm = GetCameraViewMatrix(camera.ViewMatrix, camera.ProjectionMatrix, camera.ProjectionMatrixFar, oldFov, oldFov, camera.NearPlaneDistance, camera.FarPlaneDistance, camera.FarFarPlaneDistance, camera.Position, 0f, 0f, 0);

                    // Unmove player to camera location
                    MyRender11.SetupCameraMatrices(vm);

                    if (borrowedRtvTexture != null)
                    {
                        borrowedRtvTexture.GetType().GetMethod("Release").Invoke(borrowedRtvTexture, new object[0]);
                        CameraLCD.log.Log("released");
                    }
                    didUpdate = true;
                    sw.Stop();

                    double ft = sw.Elapsed.TotalMilliseconds;
                    CameraLCD.LastFrameTime = ft;
                }
            }
            if (didUpdate)
            {
                if (!CameraLCD.Settings.steal && (MyScreenManager.Screens.Count() == 0))
                {
                    MyRender11.ProcessMessageQueue();
                }
                debugOverrides.Flares = true;
                debugOverrides.Fxaa = true;
                debugOverrides.Bloom = true;
                debugOverrides.Shadows = true;
                debugOverrides.BillboardsStatic = true;
                debugOverrides.BillboardsDynamic = true;

                geoRenderer.IsLodUpdateEnabled = true;

                renderSettings.User.GrassDrawDistance = oldGrassDist;
                renderSettings.User.GrassDensityFactor = oldGrass;
                MyRender11.Settings = renderSettings;

                ppSettings.Data.Brightness = bright;
                ppSettings.Data.Contrast = contrast;

                MyRender11.Postprocess = ppSettings;
                CameraLCD.rendering = false;
                CameraLCD.renderedFrame = true;

                if (CameraLCD.Settings.steal && CameraLCD.Settings.ratio > 1)
                {
                    return false;
                }
            }
            return true;*/
        }

        /*public static bool Prefix_SetupCameraMatricesInternal(MyRenderMessageSetCameraViewMatrix message)
        {
            cachedMessage = message;
            return true;
        }*/

        /*public static bool Prefix_ConsumeMainSprites()
        {
            CameraLCD.renderCount++;
            return true;
        }

        public static bool Prefix_RenderMainSprites()
        {
            if (CameraLCD.Settings.SpriteFix && CameraLCD.Settings.steal && CameraLCD.HasNextDisplay() && CameraLCD.IsRenderFrame())
            {
                return false;
            }
            return true;
        }*/
    }

}