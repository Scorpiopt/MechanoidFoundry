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
            if (CompMachine.cachedMachinesPawns.TryGetValue(pawn, out var comp))
            {
                var myBuilding = comp.myBuilding;
                if (myBuilding != null)
                {
                    var buildingPosition = myBuilding.Position;
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
                        var power = pawn.needs.TryGetNeed<Need_Power>();
                        if (myBuilding.TryGetComp<CompPowerTrader>().PowerOn && power != null && power.CurLevel < 0.95f)
                        {
                            return JobMaker.MakeJob(VFEDefOf.VFE_Mechanoids_Recharge, myBuilding);
                        }
                    }
                    return JobMaker.MakeJob(JobDefOf.Wait, 300);
                }
            }

            var pad = Helpers.GetAvailableMechanoidPad(pawn, pawn);
            if (pad != null)
            {
                var buildingPosition = pad.Position;
                if (pawn.Position != buildingPosition)
                {
                    if (pad.Spawned && pad.Map == pawn.Map &&
                        pawn.CanReserveAndReach(pad, PathEndMode.OnCell, Danger.Deadly))
                    {
                        return JobMaker.MakeJob(JobDefOf.Goto, buildingPosition);
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    pawn.Rotation = Rot4.South;
                    return JobMaker.MakeJob(JobDefOf.Wait, 300);
                }
            }
            return null;
        }
    }
}

