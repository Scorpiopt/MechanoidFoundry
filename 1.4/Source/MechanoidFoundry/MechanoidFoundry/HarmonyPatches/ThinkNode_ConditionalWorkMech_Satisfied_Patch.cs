using HarmonyLib;
using Verse;
using Verse.AI;

namespace MechanoidFoundry
{
    [HarmonyPatch(typeof(ThinkNode_ConditionalWorkMech), "Satisfied")]
    public static class ThinkNode_ConditionalWorkMech_Satisfied_Patch
    {
        public static void Postfix(Pawn pawn, ref bool __result)
        {
            if (!__result && pawn.IsMechanoidHacked())
            {
                foreach (var hediff in pawn.health.hediffSet.hediffs)
                {
                    var extension = hediff.def.GetModExtension<HediffExtension>();
                    if (extension != null && extension.workTypes.NullOrEmpty() is false)
                    {
                        __result = true;
                    }
                }
            }
        }
    }
}

