using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using VFECore;

namespace MechanoidFoundry
{
    [HarmonyPatch(typeof(JobGiver_Wander), "TryGiveJob")]
    public static class JobGiver_Wander_TryGiveJob_Patch
    {
        public static bool Prefix(JobGiver_Wander __instance, Pawn pawn, ref Job __result)
        {
            if (pawn.IsMechanoidHacked() && __instance is JobGiver_WanderColony)
            {
                var jobgiver = new JobGiver_ReturnToStation();
                var job = jobgiver.TryGiveJob(pawn);
                if (job != null)
                {
                    Building_Bed building = null;
                    if (job.def == JobDefOf.Goto)
                    {
                        building = job.targetA.Cell.GetFirstThing<Building_Bed>(pawn.Map);
                    }
                    else if (job.def == VFEDefOf.VFE_Mechanoids_Recharge)
                    {
                        building = job.targetA.Thing as Building_Bed;
                    }
                    if (building != null) 
                    {
                        building.CompAssignableToPawn.TryAssignPawn(pawn);
                    }
                    __result = job;
                    return false;
                }
            }
            return true;
        }
    }
}

