using avaness.CameraLCD.Gui;
using avaness.CameraLCD.Patch;
using HarmonyLib;
using Sandbox.Graphics.GUI;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using VRage.Plugins;

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
                // Try to draw displays at index -> end
                int i = displayIndex;
                if(i < displays.Count)
                {
                    foreach (var display in displays.Values.Skip(displayIndex))
                    {
                        i++;
                        if (display != null && display.OnDrawScene())
                        {
                            displayIndex = i;
                            return true;
                        }
                    }
                }

                // Try to draw displays at start -> index
                i = 0;
                foreach(var display in displays.Values)
                {
                    if (i == displayIndex)
                    {
                        displayIndex++;
                        return false;
                    }

                    i++;
                    if (display != null && display.OnDrawScene())
                    {
                        displayIndex = i;
                        return true;
                    }
                }

                // This point will only be reached if displayIndex is out of bounds and no displays could render
                displayIndex = 0;
            }
            return false;
        }

        public void Update()
        { }

        public void Dispose()
        { }

        public void OpenConfigDialog()
        {
            MyGuiSandbox.AddScreen(new MyGuiScreenPluginConfig());
        }

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