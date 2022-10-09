using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using VFEMech;

namespace MechanoidFoundry
{
    public static class Helpers
    {
        public static Dictionary<PawnKindDef, bool> cachedResults = new Dictionary<PawnKindDef, bool>();
        public static bool CanBeCreatedAndHacked(this PawnKindDef pawnKindDef)
        {
            if (!cachedResults.TryGetValue(pawnKindDef, out bool result))
            {
                if (pawnKindDef.RaceProps.IsMechanoid)
                {
                    if (MechanoidFoundryMod.settings.buildPropsByDefs.TryGetValue(pawnKindDef.defName, out var value))
                    {
                        cachedResults[pawnKindDef] = result = value.buildable;
                    }
                    else if (typeof(Machine).IsAssignableFrom(pawnKindDef.race.thingClass)
                        || pawnKindDef.race.thingClass.FullName.Contains("AIRobot.X2_AIRobot")
                        || pawnKindDef.race.thingClass.FullName.Contains("ProjectRimFactory.Drones.Pawn_Drone")
                        || pawnKindDef.defName.Contains("Shuttle"))
                    {
                        cachedResults[pawnKindDef] = result = false;
                    }
                    else
                    {
                        cachedResults[pawnKindDef] = result = true;
                    }
                }
                else
                {
                    cachedResults[pawnKindDef] = result = false;
                }
            }
            return result;
        }

        public static bool IsMechanoidHacked(this Pawn pawn)
        {
            if (pawn.Faction != null && pawn.Faction.IsPlayer && pawn.kindDef.CanBeCreatedAndHacked()
                && pawn.health.hediffSet.GetFirstHediffOfDef(MechanoidFoundryDefOf.MF_MechanoidHacked) != null)
            {
                return true;
            }
            return false;
        }

        public static Building_MechanoidPad GetAvailableMechanoidPad(Pawn pawn, Pawn mech, bool checkForPower = false)
        {
            return (Building_MechanoidPad)GenClosest.ClosestThingReachable(mech.Position, mech.MapHeld, ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial), PathEndMode.OnCell, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, delegate (Thing b)
            {
                if (b is Building_MechanoidPad platform && platform.CanTake(mech) &&
                !b.IsBurning() && (mech.Dead ||
                (!b.IsForbidden(mech) &&
                mech.CanReserve(b))) && (!checkForPower || platform.IsActive) &&
                ((mech.ownership.OwnedBed == null && !platform.CompAssignableToPawn.AssignedPawns.Any())
                || platform.CompAssignableToPawn.AssignedPawns.Contains(mech)))
                {
                    var flickable = platform.TryGetComp<CompFlickable>();
                    if (flickable != null && !flickable.SwitchIsOn)
                    {
                        return false;
                    }
                    return true;
                }
                return false;
            });
        }

        public static bool OnMechanoidPad(this Pawn pawn, out bool padIsActive)
        {
            if (pawn.CurrentBed() != null && pawn.CurrentBed() is Building_MechanoidPad curBed && curBed == pawn.ownership.OwnedBed)
            {
                padIsActive = curBed.IsActive;
                return true;
            }
            padIsActive = false;
            return false;
        }
    }
}

