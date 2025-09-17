using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace MechanoidFoundry
{
    [HarmonyPatch(typeof(ThinkNode_ConditionalWorkMode), "Satisfied")]
    public static class ThinkNode_ConditionalWorkMode_Satisfied_Patch
    {
        public static void Postfix(ref bool __result, ThinkNode_ConditionalWorkMode __instance, Pawn pawn)
        {
            if (!__result && __instance.workMode == MechWorkModeDefOf.Work && pawn.IsMechanoidHacked())
            {
                __result = true;
            }
        }
    }
}

