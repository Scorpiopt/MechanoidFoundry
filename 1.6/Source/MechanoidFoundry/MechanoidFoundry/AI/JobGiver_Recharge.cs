using RimWorld;
using Verse;
using Verse.AI;

namespace MechanoidFoundry
{
    public class JobGiver_Recharge : ThinkNode_JobGiver
    {
        public override Job TryGiveJob(Pawn pawn)
        {
            var energy = pawn.needs.TryGetNeed<Need_Energy>();
            if (energy == null || energy.CurLevelPercentage > 0.25f)
            {
                return null;
            }

            var pad = FindChargingPad(pawn);
            if (pad == null)
            {
                return null;
            }

            return JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("MF_Recharge"), pad);
        }

        private Building_MechanoidPad FindChargingPad(Pawn pawn)
        {
            return Helpers.GetAvailableMechanoidPad(pawn, pawn, true);
        }
    }
}
