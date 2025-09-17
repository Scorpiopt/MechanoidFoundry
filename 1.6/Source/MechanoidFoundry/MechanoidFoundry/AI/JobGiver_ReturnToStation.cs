using RimWorld;
using Verse;
using Verse.AI;

namespace MechanoidFoundry
{
    public class JobGiver_ReturnToStation : ThinkNode_JobGiver
    {
        public override Job TryGiveJob(Pawn pawn)
        {
            var pad = pawn.ownership.OwnedBed as Building_MechanoidPad;
            if (pad is null)
            {
                pad = Helpers.GetAvailableMechanoidPad(pawn, pawn);
                if (pad != null)
                {
                    pawn.ownership.ClaimBedIfNonMedical(pad);
                }
            }

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
                }
                else
                {
                    pawn.Rotation = Rot4.South;
                    if (Helpers.ShouldRechargeMechanoid(pawn, pad))
                    {
                        return JobMaker.MakeJob(MechanoidFoundryDefOf.MF_Recharge, pad);
                    }

                    return JobMaker.MakeJob(JobDefOf.Wait, 300);
                }
            }
            return null;
        }
    }
}

