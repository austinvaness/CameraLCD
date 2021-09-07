using avaness.CameraLCD.Patch;
using HarmonyLib;
using Sandbox.ModAPI;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using VRage.Plugins;
using VRage.Utils;

namespace avaness.CameraLCD
{
    public class CameraLCD : IPlugin
    {
        public static CameraLCDSettings Settings = new CameraLCDSettings();

        private static readonly ConcurrentDictionary<DisplayId, CameraTSS> displays = new ConcurrentDictionary<DisplayId, CameraTSS>();
        private static int displayIndex = 0;
        private static int renderCount = 0;

        public void Init(object gameInstance)
        {
            Harmony harmony = new Harmony("avaness.CameraLCDRevived");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            Patch_MyRender11.Patch(harmony);
        }

        /// <summary>
        /// Returns true if a camera was drawn.
        /// </summary>
        public static bool OnDrawScene()
        {
            renderCount++;
            if (IsRenderFrame() && HasNextDisplay())
            {
                if (displayIndex >= displays.Count)
                    displayIndex = 0;
                displays.Values.Skip(displayIndex).First()?.OnDrawScene();
                displayIndex++;
                return true;
            }
            return false;
        }

        public void Update()
        { }

        public void Dispose()
        { }

        public static void AddDisplay(DisplayId id, CameraTSS browser)
        {
            if (id.EntityId != 0)
                displays.TryAdd(id, browser);
        }

        public static void RemoveDisplay(DisplayId id)
        {
            if (id.EntityId != 0)
                displays.TryRemove(id, out _);

        }

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