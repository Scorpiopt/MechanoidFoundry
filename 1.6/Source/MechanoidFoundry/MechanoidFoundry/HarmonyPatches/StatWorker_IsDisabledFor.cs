using HarmonyLib;
using RimWorld;
using Verse;

namespace MechanoidFoundry
{

    [HarmonyPatch(typeof(StatWorker), "IsDisabledFor")]
    internal static class StatWorker_IsDisabledFor
    {
        private static bool Prefix(Thing thing, ref bool __result)
        {
            if (thing is Pawn && ((Pawn)thing).RaceProps.IsMechanoid)
            {
                var pawn = (Pawn)thing;
                if (pawn.Faction == Faction.OfPlayer)
                {
                    __result = false;
                    return false;
                }
            }
            return true;
        }
    }
}

