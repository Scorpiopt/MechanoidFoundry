using HarmonyLib;
using RimWorld;
using Verse;

namespace MechanoidFoundry
{
    [HarmonyPatch(typeof(PawnComponentsUtility), "AddAndRemoveDynamicComponents")]
    public static class PawnComponentsUtility_AddAndRemoveDynamicComponents_Patch
    {
        public static void Prefix(Pawn pawn)
        {
            if (pawn.IsMechanoidHacked())
            {
                pawn.StripBiotechComps();
            }
        }
    }
}

