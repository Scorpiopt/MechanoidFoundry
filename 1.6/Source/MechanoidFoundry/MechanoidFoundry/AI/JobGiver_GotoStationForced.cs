using Verse;
using Verse.AI;

namespace MechanoidFoundry
{
    public class JobGiver_GotoStationForced : ThinkNode_JobGiver
    {
        public override Job TryGiveJob(Pawn pawn)
        {
            var pad = pawn.ownership.OwnedBed as Building_MechanoidPad;
            if (pad != null)
            {
                var comp = pad.GetComp<CompMechanoidPad>();
                if (comp.forceStay)
                {
                    return comp.CreateGotoPadJob(pawn, pad);
                }
            }
            return null;
        }

    }
}

