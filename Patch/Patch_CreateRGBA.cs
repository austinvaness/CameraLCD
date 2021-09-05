using HarmonyLib;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using VRageMath;
using VRageRender.Messages;

namespace avaness.CameraLCDRevived.Patch
{
    /* Fixes the swapped color channels when initially placing the LCD */
    public static class Patch_CreateRGBA
    {

        private static readonly Texture2DDescription defaultDesc = new Texture2DDescription
        {
            ArraySize = 1,
            BindFlags = BindFlags.ShaderResource,
            Format = Format.R8G8B8A8_UNorm,
            Height = 0,
            Width = 0,
            Usage = ResourceUsage.Immutable,
            MipLevels = 1,
            SampleDescription = new SampleDescription
            {
                Count = 1,
                Quality = 0
            }
        };

        /* Fixes a crash due to missing format */
        public static void Postfix(object tex, string name, Vector2I resolution, bool srgb, byte[] data, bool userTexture = false, bool generateMipmaps = false)
        {
            MethodInfo m = AccessTools.Method(tex.GetType(), "Init");
            CameraLCD.log.Log("Crgba patch " + m + " " + tex + " " + name);
            Texture2DDescription desc = defaultDesc;
            desc.Usage = ResourceUsage.Default;
            desc.Width = resolution.X;
            desc.Height = resolution.Y;
            desc.Format = Format.B8G8R8A8_UNorm;
            desc.OptionFlags = ResourceOptionFlags.None;
            m.Invoke(tex, new object[] { name, desc, new Vector2I(resolution.X, resolution.Y), false });
            return;
        }

        public static bool Prefix(ref MyGeneratedTextureType type)
        {
            type = MyGeneratedTextureType.RGBA_Linear;
            return true;
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            CameraLCD.log.Log("Transpiling");
            List<CodeInstruction> instructionsList = instructions.ToList();
            for (int i = 0; i < instructionsList.Count; i++)
            {

                CodeInstruction instruction = instructionsList[i];
                if (instruction.opcode == OpCodes.Ldloc_2 && instructionsList[i + 1].opcode == OpCodes.Callvirt && instructionsList[i + 2].opcode == OpCodes.Ldarg_S && instructionsList[i + 3].opcode == OpCodes.Pop && instructionsList[i + 4].opcode == OpCodes.Pop)
                {
                    CameraLCD.log.Log("Transpilied");
                    yield return new CodeInstruction(OpCodes.Br_S, instructionsList[i - 1].operand);
                }
                else
                {
                    yield return instruction;
                }
            }
        }
    }

}