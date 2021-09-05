using HarmonyLib;
using Sandbox.Game.Components;
using Sandbox.Game.Entities.Blocks;
using System;
using VRage.Game.Entity;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace avaness.CameraLCDRevived.Patch
{
    //[HarmonyPatch(typeof(MyTextPanelComponent))]
    //[HarmonyPatch("EnsureGeneratedTexture")]
    public static class Patch_EnsureGeneratedTexture
    {
        public static Guid guid = new Guid("74DE02B3-27F9-4960-B1C4-27351F2B06D1");
        //[HarmonyPrefix]
        public static bool Prefix(object __instance)
        {
            MyTextPanelComponent instance = (MyTextPanelComponent)__instance;

            MyEntity m_entity = (MyEntity)AccessTools.Field(typeof(MyTextPanelComponent), "m_block").GetValue(__instance);
            int m_area = (int)AccessTools.Field(typeof(MyTextPanelComponent), "m_area").GetValue(__instance);
            if (!CameraLCD.displays.ContainsKey(new DisplayId(m_entity.EntityId, m_area)) || (CameraLCD.displays[new DisplayId(m_entity.EntityId, m_area)].textureName == null))
            {
                if (instance.ContentType == ContentType.SCRIPT && instance.Script == "TSS_CAMLCD")
                {
                    CameraLCD.log.Log("EnsureGenTex reset " + m_entity.DisplayNameText);
                    instance.Reset();

                    //AccessTools.Field(typeof(MyTextPanelComponent), "m_textureGenerated").SetValue(__instance, false);
                    Vector2I textureSize = (Vector2I)AccessTools.Field(typeof(MyTextPanelComponent), "m_textureSize").GetValue(__instance);
                    Patch_CreateTexture.Prefix((MyRenderComponentScreenAreas)AccessTools.Field(typeof(MyTextPanelComponent), "m_render").GetValue(__instance), m_area, textureSize, instance);
                    AccessTools.Field(typeof(MyTextPanelComponent), "m_textureGenerated").SetValue(__instance, true);
                    return false;
                }
            }
            else if ((instance.ContentType != ContentType.SCRIPT || instance.Script != "TSS_CAMLCD"))
            {
                AccessTools.Method(typeof(MyTextPanelComponent), "ReleaseTexture").Invoke(instance, new object[] { true });
                AccessTools.Field(typeof(MyTextPanelComponent), "m_textureGenerated").SetValue(__instance, false);
                return false;
                //Patch_ReleaseTexture.Prefix((MyRenderComponentScreenAreas)AccessTools.Field(typeof(MyTextPanelComponent), "m_render").GetValue(__instance), m_area);
            }
            return true;
        }

    }

}