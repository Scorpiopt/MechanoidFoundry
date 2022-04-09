using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using VFE.Mechanoids;

namespace MechanoidFoundry
{

    [HarmonyPatch(typeof(Building_Bed), "get_DrawColor")]
    static class Building_Bed_get_DrawColor
    {
        static void Postfix(Building_Bed __instance, ref Color __result)
        {
            if (__instance is Building_MechanoidPad)
            {
                __result = new Color(1f, 1f, 1f);
            }
        }
    }

    [HarmonyPatch(typeof(Building_Bed), "GetSleepingSlotPos")]
    static class Building_Bed_GetSleepingSlotPos
    {
        static void Postfix(Building_Bed __instance, ref IntVec3 __result)
        {
            if (__instance is Building_MechanoidPad)
            {
                __result = __instance.InteractionCell;
            }
        }
    }

    [HarmonyPatch(typeof(CompAssignableToPawn), "get_PlayerCanSeeAssignments")]
    static class CompAssignableToPawn_get_PlayerCanSeeAssignments
    {
        static void Postfix(CompAssignableToPawn __instance, ref bool __result)
        {
            if (__instance.parent is Building_MechanoidPad)
            {
                __result = false;
            }
        }
    }

    [HarmonyPatch(typeof(CompAssignableToPawn), "get_MaxAssignedPawnsCount")]
    static class CompAssignableToPawn_get_MaxAssignedPawnsCount
    {
        static void Postfix(CompAssignableToPawn __instance, ref int __result)
        {
            if (__instance.parent is Building_MechanoidPad)
            {
                __result = 1;
            }
        }
    }

    [HarmonyPatch(typeof(CompAssignableToPawn_Bed), "get_AssigningCandidates")]
    static class CompAssignableToPawn_Bed_AssigningCandidates
    {
        static bool Prefix(CompAssignableToPawn __instance, ref IEnumerable<Pawn> __result)
        {
            if (__instance.parent is Building_MechanoidPad)
            {
                __result = __instance.parent.Map.mapPawns.AllPawns.Where((Pawn p) => p.IsMechanoidHacked());
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Building_Bed), "GetCurOccupant")]
    static class Building_Bed_GetCurOccupant
    {
        static bool Prefix(Building_Bed __instance, int slotIndex, ref Pawn __result)
        {
            if (!(__instance is Building_MechanoidPad))
            {
                return true;
            }

            if (!__instance.Spawned)
            {
                return false;
            }

            IntVec3 sleepingSlotPos = __instance.GetSleepingSlotPos(slotIndex);
            List<Thing> list = __instance.Map.thingGrid.ThingsListAt(sleepingSlotPos);
            for (int i = 0; i < list.Count; i++)
            {
                Pawn pawn = list[i] as Pawn;
                if (pawn != null)
                {
                    if (pawn.IsMechanoidHacked())
                    {
                        __result = pawn;
                    }
                    else if (pawn.CurJob != null)
                    {
                        __result = pawn;
                    }
                }
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(RestUtility), "FindBedFor")]
    [HarmonyPatch(new Type[] { typeof(Pawn), typeof(Pawn), typeof(bool), typeof(bool), typeof(GuestStatus) })]
    class RestUtility_FindBedFor
    {
        static bool Prefix(Pawn sleeper, Pawn traveler, ref Building_Bed __result)
        {
            if (!sleeper.IsMechanoidHacked())
            {
                return true;
            }

            if (HealthAIUtility.ShouldSeekMedicalRest(sleeper))
            {
                if (sleeper.OnMechanoidPad(out _))
                {
                    __result = sleeper.CurrentBed();
                    return false;
                }
                else
                {
                    __result = Helpers.GetAvailableMechanoidPad(traveler, sleeper);
                    return false;
                }

            }
            return true;
        }
    }

    [HarmonyPatch(typeof(RestUtility), "GetBedSleepingSlotPosFor")]
    class RestUtility_GetBedSleepingSlotPosFor
    {
        static bool Prefix(Pawn pawn, Building_Bed bed, ref IntVec3 __result)
        {
            if (bed is Building_MechanoidPad)
            {
                __result = bed.GetSleepingSlotPos(1);
                return false;
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(RestUtility), "CanUseBedEver")]
    class RestUtility_CanUseBedEver
    {
        static bool Prefix(ref bool __result, Pawn p, ThingDef bedDef)
        {
            if (!p.RaceProps.IsMechanoid && (bedDef == MechanoidFoundryDefOf.MF_MechanoidPad))
            {
                __result = false;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(RestUtility), "CurrentBed")]
    class RestUtility_CurrentBed
    {
        static bool Prefix(Pawn p, ref Building_Bed __result)
        {
            if (p.jobs == null || p.CurJob == null)
            {
                return true;
            }
            if (!(p.RaceProps != null && p.RaceProps.IsMechanoid) || p.Map == null)
            {
                return true;
            }
            List<Thing> thingList = p.Position.GetThingList(p.Map);
            foreach (Thing thing in thingList)
            {
                if (thing is Building_MechanoidPad pad)
                {
                    __result = (Building_Bed)thing;
                    return false;
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(CompAssignableToPawn_Bed), "TryAssignPawn")]
    class CompAssignableToPawn_Bed_TryAssignPawn
    {
        static void Postfix(CompAssignableToPawn_Bed __instance, Pawn pawn)
        {
            if (__instance.parent is Building_MechanoidPad pad && pawn.IsMechanoidHacked())
            {
                var comp = pad.GetComp<CompMachineChargingStation>();
                comp.myPawn = pawn;
                var comp2 = pawn.GetComp<CompMachine>();
                comp2.myBuilding = pad;
            }
        }
    }

    [HarmonyPatch(typeof(CompAssignableToPawn_Bed), "TryUnassignPawn")]
    class CompAssignableToPawn_Bed_TryUnassignPawn
    {
        static void Postfix(CompAssignableToPawn_Bed __instance, Pawn pawn)
        {
            if (__instance.parent is Building_MechanoidPad pad && pawn.IsMechanoidHacked())
            {
                var comp = pad.GetComp<CompMachineChargingStation>();
                comp.myPawn = null;
                var comp2 = pawn.GetComp<CompMachine>();
                comp2.myBuilding = null;
            }
        }
    }
}
