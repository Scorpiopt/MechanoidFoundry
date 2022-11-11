using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

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
                    __result = job;
                    return false;
                }
            }
            return true;
        }
    }
}

