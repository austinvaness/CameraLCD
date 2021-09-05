using avaness.CameraLCD.Gui;
using HarmonyLib;
using Sandbox.Game.GameSystems.TextSurfaceScripts;
using Sandbox.Graphics.GUI;
using Sandbox.ModAPI;
using System.Reflection;

namespace avaness.CameraLCD.Patch
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
            if (!message.StartsWith("/cameralcd"))
                return;
            
            sendToOthers = false;

            if (!isConfigScreenOpen)
            {
                isConfigScreenOpen = true;
                MyGuiScreenPluginConfig myGuiScreenModConfig = new MyGuiScreenPluginConfig();
                myGuiScreenModConfig.Closed += OnWindowClosed;
                MyGuiSandbox.AddScreen(myGuiScreenModConfig);
            }

        }

        private static void OnWindowClosed(MyGuiScreenBase source, bool isUnloading)
        {
            source.Closed -= OnWindowClosed;
            isConfigScreenOpen = false;
        }
    }

}