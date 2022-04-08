using AnimalBehaviours;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
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
            if (!cachedResults.TryGetValue(pawnKindDef, out var result))
            {
                if (pawnKindDef.RaceProps.IsMechanoid)
                {
                    if (typeof(Machine).IsAssignableFrom(pawnKindDef.race.thingClass)
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
            Log.Message(pawnKindDef + " - " + result);
            return result;
        }

        public static bool IsMechanoidHacked(this Pawn pawn)
        {
            if (pawn.Faction != null && pawn.Faction.IsPlayer && pawn.kindDef.CanBeCreatedAndHacked())
            {
                return true;
            }
            return false;
        }

        public static Building_MechanoidPad GetAvailableMechanoidPad(Pawn pawn, Pawn targetPawn, bool checkForPower = false)
        {
            return (Building_MechanoidPad)GenClosest.ClosestThingReachable(targetPawn.Position, targetPawn.MapHeld, ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial), PathEndMode.OnCell, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, delegate (Thing b)
            {
                if (b is Building_MechanoidPad platform &&
                !b.IsBurning() && (targetPawn.Dead ||
                !b.IsForbidden(targetPawn) &&
                targetPawn.CanReserve(b)) && (!checkForPower || platform.IsActive) &&
                (targetPawn.ownership.OwnedBed == null && !platform.CompAssignableToPawn.AssignedPawns.Any() || platform.CompAssignableToPawn.AssignedPawns.Contains(targetPawn)))
                {
                    CompFlickable flickable = platform.TryGetComp<CompFlickable>();
                    if (flickable != null && !flickable.SwitchIsOn)
                    {
                        return false;
                    }
                    return true;
                }
                return false;
            });
        }

        public static bool OnMechanoidPad(this Pawn pawn)
        {
            if (pawn.CurrentBed() != null && pawn.CurrentBed() is Building_MechanoidPad curBed && curBed == pawn.ownership.OwnedBed)
            {
                return true;
            }
            return false;
        }
    }

    [StaticConstructorOnStartup]
    public static class Startup
    {
        static Startup()
        {
            foreach (var pawn in DefDatabase<PawnKindDef>.AllDefs)
            {
                if (pawn.CanBeCreatedAndHacked())
                {
                    if (pawn.race?.race?.corpseDef != null)
                    {
                        var compProps = pawn.race.GetCompProperties<CompProperties_Draftable>();
                        if (compProps is null)
                        {
                            Log.Message("Adding draftability comp to " + pawn);
                            pawn.race.comps.Add(new CompProperties_Draftable());
                        }
                        pawn.race.AllRecipes.Add(MechanoidFoundryDefOf.MF_HackMechanoid);
                        pawn.race.race.corpseDef.recipes.Add(MechanoidFoundryDefOf.MF_HackMechanoid);
                    }
                    else
                    {
                        Log.Message(pawn + " doesn't have corpse def defined, cannot make it hackable.");
                    }
                }
            }
        }
    }
}

