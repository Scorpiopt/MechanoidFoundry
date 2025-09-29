using HarmonyLib;
using RimWorld;
using Verse;

namespace MechanoidFoundry
{
    [HarmonyPatch(typeof(Pawn_NeedsTracker), "ShouldHaveNeed")]
    public static class Pawn_NeedsTracker_ShouldHaveNeed_Patch
    {
        public static void Postfix(Pawn ___pawn, NeedDef nd, ref bool __result)
        {
            if (ModsConfig.BiotechActive is false && __result is false && ___pawn.IsMechanoidHacked() && nd == MechanoidFoundryDefOf.MF_Energy)
            {
                __result = true;
            }
            if (ModsConfig.BiotechActive && nd == MechanoidFoundryDefOf.MF_Energy)
            {
                __result = false;
            }
        }
    }
}

