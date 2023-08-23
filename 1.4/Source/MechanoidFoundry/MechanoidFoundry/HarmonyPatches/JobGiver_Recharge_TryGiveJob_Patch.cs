using HarmonyLib;
using Verse;
using VFE.Mechanoids;
using VFE.Mechanoids.AI.JobGivers;
using VFEMech;

namespace MechanoidFoundry
{
    [HarmonyPatch(typeof(JobGiver_Recharge), "TryGiveJob")]
    public static class JobGiver_Recharge_TryGiveJob_Patch
    {
        public static void Prefix(Pawn pawn)
        {
            if (pawn.IsMechanoidHacked())
            {
                var comp = pawn.TryGetComp<CompMachine>();
                if (comp != null && comp.myBuilding is null)
                {
                    var building = Helpers.GetAvailableMechanoidPad(pawn, pawn);
                    if (building != null)
                    {
                        building.CompAssignableToPawn.TryAssignPawn(pawn);
                        comp.myBuilding = building;
                    }
                }
            }
        }
    }
}

