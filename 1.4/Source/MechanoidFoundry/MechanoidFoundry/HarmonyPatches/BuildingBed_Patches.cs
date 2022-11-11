using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using VFE.Mechanoids;

namespace MechanoidFoundry
{
    [HarmonyPatch(typeof(Building_Bed), "get_DrawColor")]
    internal static class Building_Bed_get_DrawColor
    {
        private static void Postfix(Building_Bed __instance, ref Color __result)
        {
            if (__instance is Building_MechanoidPad)
            {
                __result = new Color(1f, 1f, 1f);
            }
        }
    }

    [HarmonyPatch(typeof(Building_Bed), "FindPreferredInteractionCell")]
    internal static class Building_Bed_FindPreferredInteractionCell
    {
        private static bool Prefix(Building_Bed __instance)
        {
            if (__instance is Building_MechanoidPad)
            {
                return false;
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(Building_Bed), "GetSleepingSlotPos")]
    internal static class Building_Bed_GetSleepingSlotPos
    {
        private static void Postfix(Building_Bed __instance, ref IntVec3 __result)
        {
            if (__instance is Building_MechanoidPad)
            {
                __result = __instance.Position;
            }
        }
    }

    [HarmonyPatch(typeof(CompAssignableToPawn), "get_PlayerCanSeeAssignments")]
    internal static class CompAssignableToPawn_get_PlayerCanSeeAssignments
    {
        private static void Postfix(CompAssignableToPawn __instance, ref bool __result)
        {
            if (__instance.parent is Building_MechanoidPad)
            {
                __result = false;
            }
        }
    }

    [HarmonyPatch(typeof(CompAssignableToPawn), "get_MaxAssignedPawnsCount")]
    internal static class CompAssignableToPawn_get_MaxAssignedPawnsCount
    {
        private static void Postfix(CompAssignableToPawn __instance, ref int __result)
        {
            if (__instance.parent is Building_MechanoidPad)
            {
                __result = 1;
            }
        }
    }

    [HarmonyPatch(typeof(CompAssignableToPawn_Bed), "get_AssigningCandidates")]
    public static class CompAssignableToPawn_Bed_AssigningCandidates
    {
        public static bool Prefix(CompAssignableToPawn __instance, ref IEnumerable<Pawn> __result)
        {
            if (__instance.parent is Building_MechanoidPad pad)
            {
                __result = __instance.parent.Map.mapPawns.AllPawns.Where(p => p.IsMechanoidHacked() && pad.CanTake(p));
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Building_Bed), "GetCurOccupant")]
    public static class Building_Bed_GetCurOccupant
    {
        public static bool Prefix(Building_Bed __instance, int slotIndex, ref Pawn __result)
        {
            if (!(__instance is Building_MechanoidPad pad))
            {
                return true;
            }

            if (!__instance.Spawned)
            {
                return false;
            }

            var sleepingSlotPos = __instance.GetSleepingSlotPos(slotIndex);
            var list = __instance.Map.thingGrid.ThingsListAt(sleepingSlotPos);
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] is Pawn pawn && pad.CanTake(pawn) && pawn.IsMechanoidHacked())
                {
                    __result = pawn;
                }
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(RestUtility), "FindBedFor")]
    [HarmonyPatch(new Type[] { typeof(Pawn), typeof(Pawn), typeof(bool), typeof(bool), typeof(GuestStatus) })]
    internal class RestUtility_FindBedFor
    {
        private static bool Prefix(Pawn sleeper, Pawn traveler, ref Building_Bed __result)
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
    internal class RestUtility_GetBedSleepingSlotPosFor
    {
        private static bool Prefix(Pawn pawn, Building_Bed bed, ref IntVec3 __result)
        {
            if (bed is Building_MechanoidPad)
            {
                __result = bed.GetSleepingSlotPos(0);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(RestUtility), "CanUseBedEver")]
    public static class RestUtility_CanUseBedEver
    {
        public static bool Prefix(ref bool __result, Pawn p, ThingDef bedDef)
        {
            if (!p.RaceProps.IsMechanoid && typeof(Building_MechanoidPad).IsAssignableFrom(bedDef.thingClass))
            {
                __result = false;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(RestUtility), "CanUseBedNow")]
    public static class RestUtility_CanUseBedNow
    {
        public static void Postfix(ref bool __result, Thing bedThing, Pawn sleeper)
        {
            if (!__result && sleeper.IsMechanoidHacked() && bedThing is Building_MechanoidPad)
            {
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(RestUtility), "CurrentBed",
        new Type[] { typeof(Pawn), typeof(int?) }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Out })]
    internal class RestUtility_CurrentBed
    {
        private static bool Prefix(Pawn p, ref Building_Bed __result)
        {
            if (p.jobs == null || p.CurJob == null)
            {
                return true;
            }
            if (!(p.RaceProps != null && p.RaceProps.IsMechanoid) || p.Map == null)
            {
                return true;
            }
            var thingList = p.Position.GetThingList(p.Map);
            foreach (var thing in thingList)
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
    internal class CompAssignableToPawn_Bed_TryAssignPawn
    {
        private static void Postfix(CompAssignableToPawn_Bed __instance, Pawn pawn)
        {
            if (__instance.parent is Building_MechanoidPad pad && pawn.IsMechanoidHacked())
            {
                var comp = pad.GetComp<CompMachineChargingStation>();
                if (comp != null)
                {
                    comp.myPawn = pawn;
                    var comp2 = pawn.GetComp<CompMachine>();
                    if (comp2 != null)
                    {
                        comp2.myBuilding = pad;
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(CompAssignableToPawn_Bed), "TryUnassignPawn")]
    internal class CompAssignableToPawn_Bed_TryUnassignPawn
    {
        private static void Postfix(CompAssignableToPawn_Bed __instance, Pawn pawn)
        {
            if (__instance.parent is Building_MechanoidPad pad && pawn.IsMechanoidHacked())
            {
                var comp = pad.GetComp<CompMachineChargingStation>();
                if (comp != null)
                {
                    comp.myPawn = null;
                    var comp2 = pawn.GetComp<CompMachine>();
                    if (comp2 != null)
                    {
                        comp2.myBuilding = null;
                    }
                }
            }
        }
    }
}
