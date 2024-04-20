using HarmonyLib;
using RimWorld;
using Verse;
using VFE.Mechanoids.Needs;

namespace MechanoidFoundry
{
    [HarmonyPatch(typeof(Need_Power), "MaxLevel", MethodType.Getter)]
	public class Need_Power_MaxLevel_Patch
    {
		public static void Postfix(Need_Power __instance, ref float __result)
        {
			if (__instance.pawn.health.hediffSet.GetFirstHediffOfDef(MechanoidFoundryDefOf.MF_BatteryModule) != null)
            {
                __result *= 1.4f;
            }
        }
    }

    [HarmonyPatch(typeof(HealthAIUtility), "FindBestMedicine")]
    public class HealthAIUtility_FindBestMedicine_Patch
    {
        public static bool Prefix(Pawn healer, Pawn patient)
        {
            if (patient.RaceProps.IsMechanoid)
            {
                return false;
            }
            return true;
        }
    }
}

