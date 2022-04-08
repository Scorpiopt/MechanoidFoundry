using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace MechanoidFoundry
{
    [HarmonyPatch(typeof(HealthCardUtility), "DrawPawnHealthCard")]
    static class HealthCardUtility_DrawPawnHealthCard
    {
        static void Prefix(Rect outRect, Pawn pawn, ref bool allowOperations, bool showBloodLoss, Thing thingForMedBills)
        {
            if (allowOperations && pawn.Dead)
            {
                allowOperations = false;
            }
        }
    }
}

