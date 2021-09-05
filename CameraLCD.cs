using avaness.CameraLCDRevived.Patch;
using HarmonyLib;
using Sandbox.Game.Components;
using Sandbox.Game.Entities.Blocks;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.GameSystems.TextSurfaceScripts;
using Sandbox.Graphics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using VRage.Plugins;
using VRage.Render.Scene;
using VRage.Render11.Resources;
using VRage.Render11.Scene;
using VRage.Utils;
using VRageMath;
using VRageRender;

namespace avaness.CameraLCDRevived
{
    public class CameraLCD : IPlugin
    {
        //private static Type mgtm;
        //private static Type tMyManagers;
        //private static Type tMyGeometryRenderer;
        //private static Type tMyFileTextureManager;
        //private static Type tMyGBuffer;
        public static object MyBorrowedRwTextureManager { get; set; }
        public static object RwTexturesPool;
        private static ConcurrentDictionary<DisplayId, MyTSSCameraLCD2> displays = new ConcurrentDictionary<DisplayId, MyTSSCameraLCD2>();
        //public static List<Display> displayIndexes = new List<Display>();
        public static int displayIndex = 0;
        public static bool rendering = false;
        public static bool renderedFrame = false;
        public static int renderCount = 0;
        public static StringBuilder debugTextSb = new StringBuilder();
        public static CameraLCDSettings Settings = new CameraLCDSettings();
        public static double LastFrameTime = 0;
        public static Logger log = new Logger();

        public void Init(object gameInstance)
        {
            Harmony harmony = new Harmony("avaness.CameraLCDRevived");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            /*Type other = typeof(MyTextureCache);
            mgtm = other.Assembly.GetType("VRage.Render11.Resources.MyGeneratedTextureManager");

            Assembly renderAssembly = typeof(MyMeshData).Assembly;

            Type tMyGpuProfiler = typeof(MyActorFactory).Assembly.GetType("VRage.Render11.Profiler.MyGpuProfiler");
            Type tMyScene11 = typeof(MyActorFactory).Assembly.GetType("VRage.Render11.Scene.MyScene11");
            Type tMyRenderableComponent = typeof(MyActorFactory).Assembly.GetType("VRage.Render11.Scene.Components.MyRenderableComponent");
            
            Type tMyTextureData = renderAssembly.GetType("VRageRender.MyTextureData");
            Type tMyHighlight = renderAssembly.GetType("VRageRender.MyHighlight");
            //AccessTools.Method(tRender11, "DrawScene", null, null);
            //harmony.Patch(AccessTools.Method(tRender11, "ResizeSwapchain", null, null), new HarmonyMethod(AccessTools.Method(typeof(Patch_DrawScene), "Prefix_ResizeSwapchain", null, null)), null, null);
            Type MyCopyToRT = typeof(MyMeshData).Assembly.GetType("VRageRender.MyCopyToRT");
            MethodInfo MyCopyToRT_Run = AccessTools.Method(MyCopyToRT, "Run", null, null);

            MethodInfo rut = AccessTools.Method(mgtm, "ResetUserTexture");
            harmony.Patch(rut, new HarmonyMethod(AccessTools.Method(typeof(Patch_ResetUserTexture), "Prefix")));

            //MethodInfo crgba = AccessTools.Method(mgtm, "CreateRGBA", new Type[] { null, typeof(string), typeof(Vector2I), typeof(bool), typeof(byte[]), typeof(bool), typeof(bool) });
            MethodInfo crgba = AccessTools.FirstMethod(mgtm, (mi) => mi.Name == "CreateRGBA" && mi.GetParameters().Length == 7);
            //harmony.Patch(crgba, null, new HarmonyMethod(AccessTools.Method(typeof(Patch_CreateRGBA), "Postfix")));

            //            harmony.Patch(AccessTools.Method(typeof(MyRenderComponentScreenAreas), "CreateTexture"), new HarmonyMethod(typeof(Patch_CreateTexture), "Prefix"));
            harmony.Patch(AccessTools.Method(typeof(MyTextPanelComponent), "EnsureGeneratedTexture"), new HarmonyMethod(typeof(Patch_EnsureGeneratedTexture), "Prefix"));
            harmony.Patch(AccessTools.Method(typeof(MyRenderComponentScreenAreas), "ReleaseTexture"), new HarmonyMethod(typeof(Patch_CreateTexture), "Prefix_ReleaseTexture"));
            harmony.Patch(AccessTools.Method(typeof(MyTerminalBlock), "SetCustomData_Internal"), null, new HarmonyMethod(typeof(Patch_SetCustomData), "Postfix"));

            harmony.Patch(AccessTools.Method(typeof(MyTextSurfaceScriptFactory), "LoadScripts"), null, new HarmonyMethod(typeof(Patch_LoadScripts), "Postfix"));

            harmony.Patch(AccessTools.Method(tMyHighlight, "Run"), new HarmonyMethod(typeof(Patch_HighlightsRun), "Prefix"));

            tMyManagers = other.Assembly.GetType("VRage.Render11.Common.MyManagers");
            tMyGBuffer = other.Assembly.GetType("VRage.Render11.Resources.MyGBuffer");

            tMyGeometryRenderer = other.Assembly.GetType("VRage.Render11.GeometryStage2.Rendering.MyGeometryRenderer");
            harmony.Patch(AccessTools.Method(tMyGeometryRenderer, "UpdateLods"), new HarmonyMethod(typeof(Patch_UpdateLODs), "Prefix"));

            tMyFileTextureManager = other.Assembly.GetType("VRage.Render11.Resources.MyFileTextureManager");
            harmony.Patch(AccessTools.Method(tMyFileTextureManager, "CreateGeneratedTexture"), new HarmonyMethod(typeof(Patch_CreateRGBA), "Prefix"), null, new HarmonyMethod(typeof(Patch_CreateRGBA), "Transpiler"));

            Type tMyFoliageComponent = other.Assembly.GetType("VRage.Render11.Scene.Components.MyFoliageComponent");
            //harmony.Patch(AccessTools.Method(tMyFoliageComponent, "FillStreams"), new HarmonyMethod(typeof(Patch_FillStreams), "Prefix"));
            harmony.Patch(AccessTools.Method(typeof(MyMeshData).Assembly.GetType("VRageRender.MyFoliageRenderingPass"), "Render"), new HarmonyMethod(typeof(Patch_MyFoliageRenderingPass), "Prefix"));
            harmony.Patch(AccessTools.Method(typeof(MyMeshData).Assembly.GetType("VRageRender.MyFoliageGeneratingPass"), "Render"), new HarmonyMethod(typeof(Patch_MyFoliageRenderingPass), "Prefix"));*/

            //Patch_MyBorrowedRwTextureManager.Patch(harmony);
            Patch_MyRender11.Patch(harmony);
            //Patch_UpdateProxiesObjectData.Patch(harmony);
        }

        public static void OnDrawScene()
        {
            if (IsRenderFrame() && HasNextDisplay())
            {
                if (displayIndex > displays.Count)
                    displayIndex = 0;
                displays.Values.First().OnDrawScene();
            }
        }

        public void Update()
        {
            if (!Settings.ShowDebug)
            {
                return;
            }
            StringBuilder sb = debugTextSb;
            Vector2 normalizedCoord = MyGuiManager.ComputeFullscreenGuiCoordinate(MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM, 24, 256);
            normalizedCoord.Y -= 0f;
            Color labelColor = Color.Yellow;
            if ((renderCount % 60) == 0)
            {
                sb.Clear();
                sb.AppendLine("CameraLCD public build 3");
                sb.AppendLine("Active displays " + displays.Count);
                //sb.AppendLine("Active cameras " + DisplaysWithSourceCount());
                sb.AppendLine("Frame time " + LastFrameTime);
                sb.AppendLine("Steal " + Settings.steal);
                sb.AppendLine("Ratio " + Settings.ratio);
                sb.AppendLine("SpriteFix " + Settings.SpriteFix);
                sb.AppendLine("Shadows " + Settings.Shadows);
                sb.AppendLine("Billboards " + Settings.BillboardsDynamic);
                sb.AppendLine("Bloom " + Settings.Bloom);
                sb.AppendLine("Flares " + Settings.Flares);
                sb.AppendLine("Fxaa " + Settings.Fxaa);
                sb.AppendLine("Headfix " + Settings.HeadFix);
                sb.AppendLine("LOD " + Settings.UpdateLOD);
                sb.AppendLine("Range " + Settings.Range);
            }
            MyGuiManager.DrawString("BuildInfo", sb.ToString(), normalizedCoord, 0.6f, labelColor, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM, false, float.PositiveInfinity);
        }

        public void Dispose()
        {
        }

        public static void AddDisplay(DisplayId id, MyTSSCameraLCD2 browser)
        {
            if (!displays.ContainsKey(id))
            {
                displays.TryAdd(id, browser);
                //displayIndexes.Add(browser);
            }
        }

        public static void RemoveDisplay(DisplayId id)
        {
            if (displays.ContainsKey(id))
            {
                displays.Remove(id);
                //displayIndexes.RemoveAtFast(displayIndexes.FindIndex(e => e.id == id));
            }
        }

        /*public static int DisplaysWithSourceCount()
        {
            int c = 0;
            for (int i = 0; i < displayIndexes.Count; i++)
            {
                if (displayIndexes[i].source != null)
                {
                    c++;
                }
            }
            return c;
        }*/

        /*private static MyTSSCameraLCD2 GetNextDisplay()
        {
            if (displayIndexes.Count == 0)
            {
                return null;
            }
            if (displayIndex >= displayIndexes.Count)
            {
                displayIndex = 0;
            }
            Display d = displayIndexes[displayIndex];
            displayIndex++;
            if (displayIndex >= displayIndexes.Count)
            {
                displayIndex = 0;
            }
            return d;
        }*/

        public static bool HasNextDisplay()
        {
            return displays.Count > 0;
        }

        public static bool IsRenderFrame()
        {
            return Settings.Enabled && (renderCount % Settings.ratio) == 0;
        }
    }

}