using avaness.CameraLCD.Patch;
using HarmonyLib;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using VRage.Plugins;
using VRage.Utils;

namespace avaness.CameraLCD
{
    public class CameraLCD : IPlugin
    {
        public static CameraLCDSettings Settings { get; private set; }

        private static readonly ConcurrentDictionary<DisplayId, CameraTSS> displays = new ConcurrentDictionary<DisplayId, CameraTSS>();
        private static int displayIndex = 0;
        private static int renderCount = 0;

        public CameraLCD()
        {
            Settings = CameraLCDSettings.Load();
        }

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
                CameraTSS display = displays.Values.Skip(displayIndex).First();
                bool result = display != null && display.OnDrawScene();
                displayIndex++;
                return result;
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
            return Settings.Enabled && (renderCount % Settings.Ratio) == 0;
        }
    }

}