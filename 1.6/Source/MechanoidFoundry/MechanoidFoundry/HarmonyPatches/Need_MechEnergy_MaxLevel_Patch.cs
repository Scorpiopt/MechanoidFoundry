using HarmonyLib;
using RimWorld;
using Verse;

namespace MechanoidFoundry
{
    [HarmonyPatch(typeof(Need_MechEnergy), "MaxLevel", MethodType.Getter)]
    public class Need_MechEnergy_MaxLevel_Patch
    {
        public static void Postfix(Need_MechEnergy __instance, ref float __result)
        {
            if (__instance.pawn.health.hediffSet.GetFirstHediffOfDef(MechanoidFoundryDefOf.MF_BatteryModule) != null)
            {
                __result *= 1.4f;
            }
        }
    }
}
