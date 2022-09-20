using RimWorld;
using Verse;
using Verse.AI;
using VFE.Mechanoids;
using VFE.Mechanoids.Needs;
using VFECore;

namespace MechanoidFoundry
{
    public class JobGiver_ReturnToStation : ThinkNode_JobGiver
    {
        public override Job TryGiveJob(Pawn pawn)
        {
            var myBuilding = CompMachine.cachedMachinesPawns[pawn].myBuilding;
            var buildingPosition = myBuilding.Position;
            if (myBuilding != null)
            {
                if (pawn.Position != buildingPosition)
                {
                    if (myBuilding.Spawned && myBuilding.Map == pawn.Map &&
                        pawn.CanReserveAndReach(myBuilding, PathEndMode.OnCell, Danger.Deadly))
                    {
                        return JobMaker.MakeJob(JobDefOf.Goto, buildingPosition);
                    }
                }
                else
                {
                    pawn.Rotation = Rot4.South;
                    Need_Power power = pawn.needs.TryGetNeed<Need_Power>();
                    if (myBuilding.TryGetComp<CompPowerTrader>().PowerOn && power != null && power.CurLevel < 0.95f)
                    {
                        return JobMaker.MakeJob(VFEDefOf.VFE_Mechanoids_Recharge, myBuilding);
                    }
                }
                return JobMaker.MakeJob(JobDefOf.Wait, 300);
            }
            return null;
        }
    }
}

