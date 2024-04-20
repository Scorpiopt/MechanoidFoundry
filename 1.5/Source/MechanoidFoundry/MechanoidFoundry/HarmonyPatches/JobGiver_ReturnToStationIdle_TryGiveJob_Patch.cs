using HarmonyLib;
using Verse;
using VFE.Mechanoids;
using VFE.Mechanoids.AI.JobGivers;

namespace MechanoidFoundry
{
    [HarmonyPatch(typeof(JobGiver_ReturnToStationIdle), "TryGiveJob")]
    public static class JobGiver_ReturnToStationIdle_TryGiveJob_Patch
    {
        public static bool Prefix(Pawn pawn)
        {
            var building = CompMachine.cachedMachinesPawns[pawn].myBuilding;
            if (building == null)
            {
                var comp = pawn.TryGetComp<CompMachine>();
                if (comp != null && comp.myBuilding is null)
                {
                    building = Helpers.GetAvailableMechanoidPad(pawn, pawn);
                    if (building != null)
                    {
                        CompMachine.cachedMachinesPawns[pawn].myBuilding = building;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}

