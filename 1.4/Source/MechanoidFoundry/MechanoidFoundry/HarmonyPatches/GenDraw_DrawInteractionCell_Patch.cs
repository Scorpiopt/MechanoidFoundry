using HarmonyLib;
using Verse;

namespace MechanoidFoundry
{
    [HarmonyPatch(typeof(GenDraw), nameof(GenDraw.DrawInteractionCell))]
    public static class GenDraw_DrawInteractionCell_Patch
    {
        public static bool Prefix(ThingDef tDef, IntVec3 center, Rot4 placingRot)
        {
            if (tDef == MechanoidFoundryDefOf.MF_MechanoidFoundry)
            {
                return false;
            }
            return true;
        }
    }
}

