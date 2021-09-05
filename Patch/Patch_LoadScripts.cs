using HarmonyLib;
using Sandbox.Game.GameSystems.TextSurfaceScripts;
using Sandbox.Graphics.GUI;
using Sandbox.ModAPI;
using System.Reflection;

namespace avaness.CameraLCDRevived.Patch
{
    [HarmonyPatch(typeof(MyTextSurfaceScriptFactory), "LoadScripts")]
    public static class Patch_LoadScripts
    {
        public static bool isConfigScreenOpen = false;
        public static void Postfix()
        {
            MyTextSurfaceScriptFactory.Instance.RegisterFromAssembly(Assembly.GetExecutingAssembly());
            MyAPIUtilities.Static.MessageEntered -= MessageHandler;
            MyAPIUtilities.Static.MessageEntered += MessageHandler;
        }
        private static void MessageHandler(string message, ref bool sendToOthers)
        {
            if (message == "/cameralcd")
            {
                if (!isConfigScreenOpen)
                {
                    isConfigScreenOpen = true;
                    MyGuiScreenPluginConfig myGuiScreenModConfig = new MyGuiScreenPluginConfig();
                    myGuiScreenModConfig.Closed += OnWindowClosed;
                    MyGuiSandbox.AddScreen(myGuiScreenModConfig);

                }

            }
            else if (message == "/cameralcd debug off")
            {
                CameraLCD.Settings.ShowDebug = false;
            }
            else if (message == "/cameralcd debug on")
            {
                CameraLCD.Settings.ShowDebug = true;
            }
            if (message.StartsWith("/cameralcd"))
            {
                sendToOthers = false;
            }
        }

        private static void OnWindowClosed(MyGuiScreenBase source, bool isUnloading)
        {
            source.Closed -= OnWindowClosed;
            isConfigScreenOpen = false;
        }
    }

}