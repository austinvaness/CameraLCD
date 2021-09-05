using Sandbox.Game.Entities.Cube;
using VRage.Game.Entity;
using VRageMath;

namespace avaness.CameraLCDRevived.Patch
{
    //[HarmonyPatch(typeof(MyTerminalBlock), "SetCustomData_Internal")]
    public static class Patch_SetCustomData
    {
        public static void Postfix(MyTerminalBlock __instance, string value, bool sync)
        {
            MyEntity entity = __instance;
            if (value == null || value.Length == 0)
            {
                return;
            }

            if (!sync)
            {
                return;
            }
            foreach (var display in CameraLCD.displays)
            {
                if (display.Value.entity != null && display.Value.entity.EntityId == entity.EntityId)
                {
                    Patch_CreateTexture.UpdateCustomData(__instance);
                    break;
                }
            }
        }
    }

}