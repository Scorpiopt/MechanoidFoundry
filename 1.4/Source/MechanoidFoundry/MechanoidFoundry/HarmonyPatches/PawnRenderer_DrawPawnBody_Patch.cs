using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;

namespace MechanoidFoundry
{
    [HarmonyPatch(typeof(PawnRenderer), nameof(PawnRenderer.DrawPawnBody))]
    public static class PawnRenderer_DrawPawnBody_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var shouldSkip = AccessTools.Method(typeof(PawnRenderer_DrawPawnBody_Patch), nameof(AllowOverlay));
            var get_IsMechanoidInfo = AccessTools.PropertyGetter(typeof(RaceProperties), nameof(RaceProperties.IsMechanoid));
            var pawnInfo = AccessTools.Field(typeof(PawnRenderer), nameof(PawnRenderer.pawn));
            var codes = instructions.ToList();
            for (int i = 0; i < codes.Count; i++)
            {
                yield return codes[i];
                if (i > 0 && codes[i].opcode == OpCodes.Brfalse_S && codes[i - 1].Calls(get_IsMechanoidInfo))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, pawnInfo);
                    yield return new CodeInstruction(OpCodes.Call, shouldSkip);
                    yield return new CodeInstruction(OpCodes.Brfalse_S, codes[i].operand);
                }
            }
        }

        public static bool AllowOverlay(Pawn pawn)
        {
            return ModsConfig.BiotechActive;
        }
    }
}

