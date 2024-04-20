using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace MechanoidFoundry
{
    [HarmonyPatch(typeof(JobGiver_GetEnergy), "GetMinAutorechargeThreshold")]
    public static class JobGiver_GetEnergy_GetMinAutorechargeThreshold_Patch
    {
        public static void Postfix(ref int __result, Pawn pawn)
        {
            if (pawn.IsMechanoidHacked())
            {
                __result = GetMinAutorechargeThreshold(__result, pawn);
            }
        }

        public static int GetMinAutorechargeThreshold(int initialNum, Pawn pawn)
        {
            MechanitorControlGroup mechControlGroup = pawn.GetMechControlGroup();
            if (JobGiver_GetEnergy.UseGroupRechargeLimits(mechControlGroup) is false)
            {
                return Mathf.RoundToInt((float)pawn.RaceProps.maxMechEnergy * 0.33f);
            }
            return initialNum;
        }
    }
}

