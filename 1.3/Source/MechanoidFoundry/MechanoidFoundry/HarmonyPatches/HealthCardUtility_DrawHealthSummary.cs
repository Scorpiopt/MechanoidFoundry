using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace MechanoidFoundry
{
    [HarmonyPatch(typeof(HealthCardUtility), "DrawHealthSummary")]
    static class HealthCardUtility_DrawHealthSummary
    {
        static void Prefix(Rect rect, Pawn pawn, ref bool allowOperations, Thing thingForMedBills)
        {
            if (!allowOperations && pawn.Dead && pawn.RaceProps.IsMechanoid)
            {
                allowOperations = ITab_Pawn_Health_ShouldAllowOperations.ShouldAllowOperations(pawn);
            }
        }
    }
}

