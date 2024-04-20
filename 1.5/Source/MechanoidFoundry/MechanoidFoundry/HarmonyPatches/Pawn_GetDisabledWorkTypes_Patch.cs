using HarmonyLib;
using System.Collections.Generic;
using Verse;

namespace MechanoidFoundry
{
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.GetDisabledWorkTypes))]
    public static class Pawn_GetDisabledWorkTypes_Patch
    {
        public static void Postfix(List<WorkTypeDef> __result, Pawn __instance)
        {
            foreach (var hediff in __instance.health.hediffSet.hediffs)
            {
                var extension = hediff.def.GetModExtension<HediffExtension>();
                if (extension?.workTypes != null)
                {
                    foreach (var work in extension.workTypes)
                    {
                        __result.Remove(work);
                    }
                }
            }
        }
    }
}
